using System;
using System.Collections.Generic;
using System.IO;
using System.util;
using iTextSharp.text.error_messages;
using iTextSharp.text.exceptions;
using iTextSharp.text.log;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf {
    /**
    *
    * @author  psoares
    */
    [Obsolete]
    internal class PdfCopyFieldsImp : PdfWriter {

        internal static readonly PdfName iTextTag = new PdfName("_iTextTag_");
        internal static int zero = 0;
        internal List<PdfReader> readers = new List<PdfReader>();
        internal Dictionary<PdfReader, IntHashtable> readers2intrefs = new Dictionary<PdfReader, IntHashtable>();
        internal Dictionary<PdfReader, IntHashtable> pages2intrefs = new Dictionary<PdfReader, IntHashtable>();
        internal Dictionary<PdfReader, IntHashtable> visited = new Dictionary<PdfReader, IntHashtable>();
        internal List<AcroFields> fields = new List<AcroFields>();
        internal RandomAccessFileOrArray file;
        internal Dictionary<String, Object> fieldTree = new Dictionary<string,object>();
        internal List<PdfIndirectReference> pageRefs = new List<PdfIndirectReference>();
        internal List<PdfDictionary> pageDics = new List<PdfDictionary>();
        internal PdfDictionary resources = new PdfDictionary();
        internal PdfDictionary form;
        bool closing = false;
        internal Document nd;
        private Dictionary<PdfArray, List<int>> tabOrder;
        private List<String> calculationOrder = new List<String>();
        private List<Object> calculationOrderRefs;
        private bool hasSignature;
        private bool needAppearances = false;
        private Dictionary<Object, object> mergedRadioButtons = new Dictionary<object, object>();
        
        protected ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfCopyFields));
        protected override ICounter GetCounter() {
    	    return COUNTER;
        }

        internal PdfCopyFieldsImp(Stream os) : this(os, '\0') {
        }
        
        internal PdfCopyFieldsImp(Stream os, char pdfVersion) : base(new PdfDocument(), os) {
            pdf.AddWriter(this);
            if (pdfVersion != 0)
                base.PdfVersion = pdfVersion;
            nd = new Document();
            nd.AddDocListener(pdf);
        }
        
        internal void AddDocument(PdfReader reader, ICollection<int> pagesToKeep) {
            if (!readers2intrefs.ContainsKey(reader) && reader.Tampered)
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.was.reused"));
            reader = new PdfReader(reader);        
            reader.SelectPages(pagesToKeep);
            if (reader.NumberOfPages == 0)
                return;
            reader.Tampered = false;
            AddDocument(reader);
        }
        
        internal void AddDocument(PdfReader reader) {
            if (!reader.IsOpenedWithFullPermissions)
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            OpenDoc();
            if (readers2intrefs.ContainsKey(reader)) {
                reader = new PdfReader(reader);
            }
            else {
                if (reader.Tampered)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.was.reused"));
                reader.ConsolidateNamedDestinations();
                reader.Tampered = true;
            }
            reader.ShuffleSubsetNames();
            readers2intrefs[reader] =  new IntHashtable();
            readers.Add(reader);
            int len = reader.NumberOfPages;
            IntHashtable refs = new IntHashtable();
            for (int p = 1; p <= len; ++p) {
                refs[reader.GetPageOrigRef(p).Number] = 1;
                reader.ReleasePage(p);
            }
            pages2intrefs[reader] =  refs;
            visited[reader] =  new IntHashtable();
            AcroFields acro = reader.AcroFields;
            // when a document with NeedAppearances is encountered, the flag is set
            // in the resulting document.
            bool needapp = !acro.GenerateAppearances;
            if(needapp)
                needAppearances = true;
            fields.Add(acro);
            UpdateCalculationOrder(reader);
        }
        
        internal static String GetCOName(PdfReader reader, PRIndirectReference refi) {
            String name = "";
            while (refi != null) {
                PdfObject obj = PdfReader.GetPdfObject(refi);
                if (obj == null || obj.Type != PdfObject.DICTIONARY)
                    break;
                PdfDictionary dic = (PdfDictionary)obj;
                PdfString t = dic.GetAsString(PdfName.T);
                if (t != null) {
                    name = t.ToUnicodeString()+ "." + name;
                }
                refi = (PRIndirectReference)dic.Get(PdfName.PARENT);
            }
            if (name.EndsWith("."))
                name = name.Substring(0, name.Length - 1);
            return name;
        }
        
        virtual protected internal void UpdateCalculationOrder(PdfReader reader) {
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acro = catalog.GetAsDict(PdfName.ACROFORM);
            if (acro == null)
                return;
            PdfArray co = acro.GetAsArray(PdfName.CO);
            if (co == null || co.Size == 0)
                return;
            AcroFields af = reader.AcroFields;
            for (int k = 0; k < co.Size; ++k) {
                PdfObject obj = co[k];
                if (obj == null || !obj.IsIndirect())
                    continue;
                String name = GetCOName(reader, (PRIndirectReference)obj) ;
                if (af.GetFieldItem(name) == null)
                    continue;
                name = "." + name;
                if (calculationOrder.Contains(name))
                    continue;
                calculationOrder.Add(name);
            }
        }
        
        internal void Propagate(PdfObject obj, PdfIndirectReference refo, bool restricted) {
            if (obj == null)
                return;
    //        if (refo != null)
    //            AddToBody(obj, refo);
            if (obj is PdfIndirectReference)
                return;
            switch (obj.Type) {
                case PdfObject.DICTIONARY:
                case PdfObject.STREAM: {
                    PdfDictionary dic = (PdfDictionary)obj;
                    foreach (PdfName key in dic.Keys) {
                        if (restricted && (key.Equals(PdfName.PARENT) || key.Equals(PdfName.KIDS)))
                            continue;
                        PdfObject ob = dic.Get(key);
                        if (ob != null && ob.IsIndirect()) {
                            PRIndirectReference ind = (PRIndirectReference)ob;
                            if (!SetVisited(ind) && !IsPage(ind)) {
                                PdfIndirectReference refi = GetNewReference(ind);
                                Propagate(PdfReader.GetPdfObjectRelease(ind), refi, restricted);
                            }
                        }
                        else
                            Propagate(ob, null, restricted);
                    }
                    break;
                }
                case PdfObject.ARRAY: {
                    //PdfArray arr = new PdfArray();
                    for (ListIterator<PdfObject> it = ((PdfArray)obj).GetListIterator(); it.HasNext();) {
                        PdfObject ob = it.Next();
                        if (ob != null && ob.IsIndirect()) {
                            PRIndirectReference ind = (PRIndirectReference)ob;
                            if (!IsVisited(ind) && !IsPage(ind)) {
                                PdfIndirectReference refi = GetNewReference(ind);
                                Propagate(PdfReader.GetPdfObjectRelease(ind), refi, restricted);
                            }
                        }
                        else
                            Propagate(ob, null, restricted);
                    }
                    break;
                }
                case PdfObject.INDIRECT: {
                    throw new Exception(MessageLocalization.GetComposedMessage("reference.pointing.to.reference"));
                }
            }
        }
        
        private void AdjustTabOrder(PdfArray annots, PdfIndirectReference ind, PdfNumber nn) {
            int v = nn.IntValue;
            List<int> t;
            if (!tabOrder.TryGetValue(annots, out t)) {
                t = new List<int>();
                int size = annots.Size - 1;
                for (int k = 0; k < size; ++k) {
                    t.Add(zero);
                }
                t.Add(v);
                tabOrder[annots] =  t;
                annots.Add(ind);
            }
            else {
                int size = t.Count - 1;
                for (int k = size; k >= 0; --k) {
                    if (t[k] <= v) {
                        t.Insert(k + 1, v);
                        annots.Add(k + 1, ind);
                        size = -2;
                        break;
                    }
                }
                if (size != -2) {
                    t.Insert(0, v);
                    annots.Add(0, ind);
                }
            }
        }
        
        virtual protected PdfArray BranchForm(Dictionary<string,object> level, PdfIndirectReference parent, String fname) {
            PdfArray arr = new PdfArray();
            foreach (KeyValuePair<string,object> entry in level) {
                String name = entry.Key;
                Object obj = entry.Value;
                PdfIndirectReference ind = PdfIndirectReference;
                PdfDictionary dic = new PdfDictionary();
                if (parent != null)
                    dic.Put(PdfName.PARENT, parent);
                dic.Put(PdfName.T, new PdfString(name, PdfObject.TEXT_UNICODE));
                String fname2 = fname + "." + name;
                int coidx = calculationOrder.IndexOf(fname2);
                if (coidx >= 0)
                    calculationOrderRefs[coidx] = ind;
                if (obj is Dictionary<string,object>) {
                    dic.Put(PdfName.KIDS, BranchForm((Dictionary<string,object>)obj, ind, fname2));
                    arr.Add(ind);
                    AddToBody(dic, ind);
                }
                else {
                    List<object> list = (List<object>)obj;
                    dic.MergeDifferent((PdfDictionary)list[0]);
                    if (list.Count == 3) {
                        dic.MergeDifferent((PdfDictionary)list[2]);
                        int page = (int)list[1];
                        PdfDictionary pageDic = pageDics[page - 1];
                        PdfArray annots = pageDic.GetAsArray(PdfName.ANNOTS);
                        if (annots == null) {
                            annots = new PdfArray();
                            pageDic.Put(PdfName.ANNOTS, annots);
                        }
                        PdfNumber nn = (PdfNumber)dic.Get(iTextTag);
                        dic.Remove(iTextTag);
                        AdjustTabOrder(annots, ind, nn);
                    }
                    else {
                        PdfDictionary field = (PdfDictionary) list[0];
                        PdfName v = field.GetAsName(PdfName.V);
                        PdfArray kids = new PdfArray();
                        for (int k = 1; k < list.Count; k += 2) {
                            int page = (int)list[k];
                            PdfDictionary pageDic = pageDics[page - 1];
                            PdfArray annots = pageDic.GetAsArray(PdfName.ANNOTS);
                            if (annots == null) {
                                annots = new PdfArray();
                                pageDic.Put(PdfName.ANNOTS, annots);
                            }
                            PdfDictionary widget = new PdfDictionary();
                            widget.Merge((PdfDictionary) list[k + 1]);
                            widget.Put(PdfName.PARENT, ind);
                            PdfNumber nn = (PdfNumber) widget.Get(iTextTag);
                            widget.Remove(iTextTag);
                            if (PdfCopy.IsCheckButton(field)) {
                                PdfName _as = widget.GetAsName(PdfName.AS);
                                if (v != null && _as != null)
                                    widget.Put(PdfName.AS, v);
                            } else if (PdfCopy.IsRadioButton(field)) {
                                PdfName _as = widget.GetAsName(PdfName.AS);
                                if (v != null && _as != null && !_as.Equals(GetOffStateName(widget))) {
                                    if (!mergedRadioButtons.ContainsKey(list)) {
                                        mergedRadioButtons[list] = null;
                                        widget.Put(PdfName.AS, v);
                                    } else {
                                        widget.Put(PdfName.AS, GetOffStateName(widget));
                                    }
                                }
                            }
                            PdfIndirectReference wref = AddToBody(widget).IndirectReference;
                            AdjustTabOrder(annots, wref, nn);
                            kids.Add(wref);
                            Propagate(widget, null, false);
                        }
                        dic.Put(PdfName.KIDS, kids);
                    }
                    arr.Add(ind);
                    AddToBody(dic, ind);
                    Propagate(dic, null, false);
                }
            }
            return arr;
        }

        virtual protected PdfName GetOffStateName(PdfDictionary widget) {
            return PdfName.Off_;
        }
        
        virtual protected void CreateAcroForms() {
            if (fieldTree.Count == 0)
                return;
            form = new PdfDictionary();
            form.Put(PdfName.DR, resources);
            Propagate(resources, null, false);
            if(needAppearances) {
                form.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
            }
            form.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            tabOrder = new Dictionary<PdfArray,List<int>>();
            calculationOrderRefs = new List<object>();
            foreach (string sc in calculationOrder)
                calculationOrderRefs.Add(sc);
            form.Put(PdfName.FIELDS, BranchForm(fieldTree, null, ""));
            if (hasSignature)
                form.Put(PdfName.SIGFLAGS, new PdfNumber(3));
            PdfArray co = new PdfArray();
            for (int k = 0; k < calculationOrderRefs.Count; ++k) {
                Object obj = calculationOrderRefs[k];
                if (obj is PdfIndirectReference)
                    co.Add((PdfIndirectReference)obj);
            }
            if (co.Size > 0)
                form.Put(PdfName.CO, co);
        }
        
        public override void Close() {
            if (closing) {
                base.Close();
                return;
            }
            closing = true;
            CloseIt();
        }
        
        virtual protected void CloseIt() {
            for (int k = 0; k < readers.Count; ++k) {
                readers[k].RemoveFields();
            }
            for (int r = 0; r < readers.Count; ++r) {
                PdfReader reader = readers[r];
                for (int page = 1; page <= reader.NumberOfPages; ++page) {
                    pageRefs.Add(GetNewReference(reader.GetPageOrigRef(page)));
                    pageDics.Add(reader.GetPageN(page));
                }
            }
            MergeFields();
            CreateAcroForms();
            for (int r = 0; r < readers.Count; ++r) {
                    PdfReader reader = readers[r];
                    for (int page = 1; page <= reader.NumberOfPages; ++page) {
                        PdfDictionary dic = reader.GetPageN(page);
                        PdfIndirectReference pageRef = GetNewReference(reader.GetPageOrigRef(page));
                        PdfIndirectReference parent = root.AddPageRef(pageRef);
                        dic.Put(PdfName.PARENT, parent);
                        Propagate(dic, pageRef, false);
                    }
            }
            foreach (KeyValuePair<PdfReader, IntHashtable> entry in readers2intrefs) {
                PdfReader reader = entry.Key;
                try {
                    file = reader.SafeFile;
                    file.ReOpen();
                    IntHashtable t = entry.Value;
                    int[] keys = t.ToOrderedKeys();
                    for (int k = 0; k < keys.Length; ++k) {
                        PRIndirectReference refi = new PRIndirectReference(reader, keys[k]);
                        AddToBody(PdfReader.GetPdfObjectRelease(refi), t[keys[k]]);
                    }
                }
                finally {
                    try {
                        file.Close();
                        // TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
                        //reader.Close();
                    }
                    catch  {
                        // empty on purpose
                    }
                }
            }
            pdf.Close();
        }
        
        internal void AddPageOffsetToField(IDictionary<String, AcroFields.Item> fd, int pageOffset) {
            if (pageOffset == 0)
                return;
            foreach (AcroFields.Item item in fd.Values) {
                List<int> page = item.page;
                for (int k = 0; k < page.Count; ++k) {
                    int p = item.GetPage(k);
                    item.ForcePage(k, p + pageOffset);
                }
            }
        }

        internal void CreateWidgets(List<Object> list, AcroFields.Item item) {
            for (int k = 0; k < item.Size; ++k) {
                list.Add(item.GetPage(k));
                PdfDictionary merged = item.GetMerged(k);
                PdfObject dr = merged.Get(PdfName.DR);
                if (dr != null)
                    PdfFormField.MergeResources(resources, (PdfDictionary)PdfReader.GetPdfObject(dr));
                PdfDictionary widget = new PdfDictionary();
                foreach (PdfName key in merged.Keys) {
                    if (widgetKeys.ContainsKey(key))
                        widget.Put(key, merged.Get(key));
                }
                widget.Put(iTextTag, new PdfNumber(item.GetTabOrder(k) + 1));
                list.Add(widget);
            }
        }
        
        internal void MergeField(String name, AcroFields.Item item) {
            Dictionary<string,object> map = fieldTree;
            StringTokenizer tk = new StringTokenizer(name, ".");
            if (!tk.HasMoreTokens())
                return;
            while (true) {
                String s = tk.NextToken();
                Object obj;
                map.TryGetValue(s, out obj);
                if (tk.HasMoreTokens()) {
                    if (obj == null) {
                        obj = new Dictionary<string,object>();
                        map[s] = obj;
                        map = (Dictionary<string,object>)obj;
                        continue;
                    }
                    else if (obj is Dictionary<string,object>)
                        map = (Dictionary<string,object>)obj;
                    else
                        return;
                }
                else {
                    if (obj is Dictionary<string,object>)
                        return;
                    PdfDictionary merged = item.GetMerged(0);
                    if (obj == null) {
                        PdfDictionary field = new PdfDictionary();
                        if (PdfName.SIG.Equals(merged.Get(PdfName.FT)))
                            hasSignature = true;
                        foreach (PdfName key in merged.Keys) {
                            if (fieldKeys.ContainsKey(key))
                                field.Put(key, merged.Get(key));
                        }
                        List<object> list = new List<object>();
                        list.Add(field);
                        CreateWidgets(list, item);
                        map[s] =  list;
                    }
                    else {
                        List<object> list = (List<object>)obj;
                        PdfDictionary field = (PdfDictionary)list[0];
                        PdfName type1 = (PdfName)field.Get(PdfName.FT);
                        PdfName type2 = (PdfName)merged.Get(PdfName.FT);
                        if (type1 == null || !type1.Equals(type2))
                            return;
                        int flag1 = 0;
                        PdfObject f1 = field.Get(PdfName.FF);
                        if (f1 != null && f1.IsNumber())
                            flag1 = ((PdfNumber)f1).IntValue;
                        int flag2 = 0;
                        PdfObject f2 = merged.Get(PdfName.FF);
                        if (f2 != null && f2.IsNumber())
                            flag2 = ((PdfNumber)f2).IntValue;
                        if (type1.Equals(PdfName.BTN)) {
                            if (((flag1 ^ flag2) & PdfFormField.FF_PUSHBUTTON) != 0)
                                return;
                            if ((flag1 & PdfFormField.FF_PUSHBUTTON) == 0 && ((flag1 ^ flag2) & PdfFormField.FF_RADIO) != 0)
                                return;
                        }
                        else if (type1.Equals(PdfName.CH)) {
                            if (((flag1 ^ flag2) & PdfFormField.FF_COMBO) != 0)
                                return;
                        }
                        CreateWidgets(list, item);
                    }
                    return;
                }
            }
        }
        
        internal void MergeWithMaster(IDictionary<string,AcroFields.Item> fd) {
            foreach (KeyValuePair<string,AcroFields.Item> entry in fd) {
                String name = entry.Key;
                MergeField(name, entry.Value);
            }
        }
        
        internal virtual void MergeFields() {
            int pageOffset = 0;
            for (int k = 0; k < fields.Count; ++k) {
                IDictionary<string,AcroFields.Item> fd = fields[k].Fields;
                AddPageOffsetToField(fd, pageOffset);
                MergeWithMaster(fd);
                pageOffset += readers[k].NumberOfPages;
            }
        }

        public override PdfIndirectReference GetPageReference(int page) {
            return pageRefs[page - 1];
        }
        
        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj) {
            PdfDictionary cat = pdf.GetCatalog(rootObj);
            if (form != null) {
                PdfIndirectReference refi = AddToBody(form).IndirectReference;
                cat.Put(PdfName.ACROFORM, refi);
            }
            return cat;
        }

        virtual protected PdfIndirectReference GetNewReference(PRIndirectReference refi) {
            return new PdfIndirectReference(0, GetNewObjectNumber(refi.Reader, refi.Number, 0));
        }
        
        protected internal override int GetNewObjectNumber(PdfReader reader, int number, int generation) {
            IntHashtable refs = readers2intrefs[reader];
            int n = refs[number];
            if (n == 0) {
                n = IndirectReferenceNumber;
                refs[number] = n;
            }
            return n;
        }
        
        /**
        * Sets a reference to "visited" in the copy process.
        * @param   ref the reference that needs to be set to "visited"
        * @return  true if the reference was set to visited
        */
        virtual protected internal bool SetVisited(PRIndirectReference refi) {
            IntHashtable refs;
            if (visited.TryGetValue(refi.Reader, out refs)) {
                int old = refs[refi.Number];
                refs[refi.Number] = 1;
                return (old != 0);
            }
            else
                return false;
        }
        
        /**
        * Checks if a reference has already been "visited" in the copy process.
        * @param   ref the reference that needs to be checked
        * @return  true if the reference was already visited
        */
        virtual protected internal bool IsVisited(PRIndirectReference refi) {
            IntHashtable refs;
            if (visited.TryGetValue(refi.Reader, out refs))
                return refs.ContainsKey(refi.Number);
            else
                return false;
        }
        
        virtual protected internal bool IsVisited(PdfReader reader, int number, int generation) {
            IntHashtable refs = readers2intrefs[reader];
            return refs.ContainsKey(number);
        }
        
        /**
        * Checks if a reference refers to a page object.
        * @param   ref the reference that needs to be checked
        * @return  true is the reference refers to a page object.
        */
        virtual protected internal bool IsPage(PRIndirectReference refi) {
            IntHashtable refs;
            if (pages2intrefs.TryGetValue(refi.Reader, out refs))
                return refs.ContainsKey(refi.Number);
            else
                return false;
        }

        internal override RandomAccessFileOrArray GetReaderFile(PdfReader reader) {
            return file;
        }

        virtual public void OpenDoc() {
            if (!nd.IsOpen())
                nd.Open();
        }    
        
        protected internal static Dictionary<PdfName,int> widgetKeys = new Dictionary<PdfName,int>();
        protected internal static Dictionary<PdfName,int> fieldKeys = new Dictionary<PdfName,int>();
        static PdfCopyFieldsImp() {
            int one = 1;
            widgetKeys[PdfName.SUBTYPE] =  one;
            widgetKeys[PdfName.CONTENTS] =  one;
            widgetKeys[PdfName.RECT] =  one;
            widgetKeys[PdfName.NM] =  one;
            widgetKeys[PdfName.M] =  one;
            widgetKeys[PdfName.F] =  one;
            widgetKeys[PdfName.BS] =  one;
            widgetKeys[PdfName.BORDER] =  one;
            widgetKeys[PdfName.AP] =  one;
            widgetKeys[PdfName.AS] =  one;
            widgetKeys[PdfName.C] =  one;
            widgetKeys[PdfName.A] =  one;
            widgetKeys[PdfName.STRUCTPARENT] =  one;
            widgetKeys[PdfName.OC] =  one;
            widgetKeys[PdfName.H] =  one;
            widgetKeys[PdfName.MK] =  one;
            widgetKeys[PdfName.DA] =  one;
            widgetKeys[PdfName.Q] =  one;
            widgetKeys[PdfName.P] =  one;
            fieldKeys[PdfName.AA] =  one;
            fieldKeys[PdfName.FT] =  one;
            fieldKeys[PdfName.TU] =  one;
            fieldKeys[PdfName.TM] =  one;
            fieldKeys[PdfName.FF] =  one;
            fieldKeys[PdfName.V] =  one;
            fieldKeys[PdfName.DV] =  one;
            fieldKeys[PdfName.DS] =  one;
            fieldKeys[PdfName.RV] =  one;
            fieldKeys[PdfName.OPT] =  one;
            fieldKeys[PdfName.MAXLEN] =  one;
            fieldKeys[PdfName.TI] =  one;
            fieldKeys[PdfName.I] =  one;
            fieldKeys[PdfName.LOCK] =  one;
            fieldKeys[PdfName.SV] =  one;
        }
    }
}
