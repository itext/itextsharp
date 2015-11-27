using System;
using System.Collections.Generic;
using System.IO;
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.exceptions;
using iTextSharp.text.log;
using System.util;

/*
 * $Id$
 * 
 *
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
    * Make copies of PDF documents. Documents can be edited after reading and
    * before writing them out.
    * @author Mark Thompson
    */

    public class PdfCopy : PdfWriter {
        /**
        * This class holds information about indirect references, since they are
        * renumbered by iText.
        */
        public class IndirectReferences {
            PdfIndirectReference theRef;
            bool hasCopied;
            internal IndirectReferences(PdfIndirectReference refi) {
                theRef = refi;
                hasCopied = false;
            }
            internal void SetCopied() { hasCopied = true; }
            internal bool Copied {
                get {
                    return hasCopied; 
                }
                set {
                    hasCopied = value;
                }
            }
            internal PdfIndirectReference Ref {
                get {
                    return theRef; 
                }
            }

            public override String ToString() {
                String ext = "";
                if (hasCopied)
                    ext += " Copied";
                return Ref + ext;
            }
        }

        protected new static ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfCopy));
        protected override ICounter GetCounter() {
    	    return COUNTER;
        }
        protected internal Dictionary<RefKey, IndirectReferences> indirects;
        protected Dictionary<PdfReader,Dictionary<RefKey, IndirectReferences>>  indirectMap;
        protected Dictionary<PdfObject, PdfObject> parentObjects;
        protected HashSet2<PdfObject> disableIndirects;
        protected PdfReader reader;
        protected int[] namePtr = {0};
        /** Holds value of property rotateContents. */
        private bool rotateContents = true;
        protected internal PdfArray fieldArray;
        protected internal HashSet2<PdfTemplate> fieldTemplates; 
        private PdfStructTreeController structTreeController = null;
        private int currentStructArrayNumber = 0;
        //remember to avoid coping
        protected PRIndirectReference structTreeRootReference;
        //to remove unused objects
        protected Dictionary<RefKey, PdfIndirectObject> indirectObjects;
        //PdfIndirectObjects, that generate PdfWriter.addToBody(PdfObject) method, already saved to PdfBody
        protected List<PdfIndirectObject> savedObjects;
        //imported pages from getImportedPage(PdfReader, int, bool)
        protected List<ImportedPage> importedPages;
        //for correct update of kids in StructTreeRootController
        internal bool updateRootKids = false;

        static private readonly PdfName annotId = new PdfName("iTextAnnotId");
        static private int annotIdCnt = 0;

        protected bool mergeFields = false;
        private bool needAppearances = false;
        private bool hasSignature;
        private PdfIndirectReference acroForm;
        private Dictionary<PdfArray, List<int>> tabOrder;
        private List<Object> calculationOrderRefs;
        private PdfDictionary resources;
        protected List<AcroFields> fields;
        private List<String> calculationOrder;
        private Dictionary<String, Object> fieldTree;
        private Dictionary<int, PdfIndirectObject> unmergedMap;
        private Dictionary<RefKey, PdfIndirectObject> unmergedIndirectRefsMap;
        private Dictionary<int, PdfIndirectObject> mergedMap;
        private HashSet2<PdfIndirectObject> mergedSet;
        private bool mergeFieldsInternalCall = false;
        private static readonly PdfName iTextTag = new PdfName("_iTextTag_");
        internal static int zero = 0;
        private HashSet2<Object> mergedRadioButtons = new HashSet2<object>();
        private Dictionary<Object, PdfString> mergedTextFields = new Dictionary<Object, PdfString>();

        private HashSet2<PdfReader> readersWithImportedStructureTreeRootKids = new HashSet2<PdfReader>();

        protected class ImportedPage {
            internal readonly int pageNumber;
            internal readonly PdfReader reader;
            internal readonly PdfArray mergedFields;
            internal PdfIndirectReference annotsIndirectReference;
            internal ImportedPage(PdfReader reader, int pageNumber, bool keepFields) {
                this.pageNumber = pageNumber;
                this.reader = reader;
                if (keepFields) {
                    mergedFields = new PdfArray();
                }
            }

            public override bool Equals(Object o) {
                if (!(o is ImportedPage))
                    return false;
                ImportedPage other = (ImportedPage)o;
                return this.pageNumber == other.pageNumber && this.reader.Equals(other.reader);
            }

            public override String ToString() {
                return pageNumber.ToString();
            }
        }

        
        /**
        * Constructor
        * @param document
        * @param os outputstream
        */
        public PdfCopy(Document document, Stream os) : base(new PdfDocument(), os) {
            document.AddDocListener(pdf);
            pdf.AddWriter(this);
            indirectMap = new Dictionary<PdfReader,Dictionary<RefKey,IndirectReferences>>();
            parentObjects = new Dictionary<PdfObject, PdfObject>();
            disableIndirects = new HashSet2<PdfObject>();

            indirectObjects = new Dictionary<RefKey, PdfIndirectObject>();
            savedObjects = new List<PdfIndirectObject>();
            importedPages = new List<ImportedPage>();
        }

        /**
         * Setting page events isn't possible with Pdf(Smart)Copy.
         * Use the PageStamp class if you want to add content to copied pages.
         * @see com.itextpdf.text.pdf.PdfWriter#setPageEvent(com.itextpdf.text.pdf.PdfPageEvent)
         */
        public override IPdfPageEvent PageEvent {
            set {
                throw new InvalidOperationException();
            }
        }

        /** Checks if the content is automatically adjusted to compensate
        * the original page rotation.
        * @return the auto-rotation status
        */    
        /** Flags the content to be automatically adjusted to compensate
        * the original page rotation. The default is <CODE>true</CODE>.
        * @param rotateContents <CODE>true</CODE> to set auto-rotation, <CODE>false</CODE>
        * otherwise
        */    
        virtual public bool RotateContents {
            set {
                rotateContents = value;
            }
            get {
                return rotateContents;
            }
        }

        virtual public void SetMergeFields() {
            mergeFields = true;
            resources = new PdfDictionary();
            fields = new List<AcroFields>();
            calculationOrder = new List<String>();
            fieldTree = new Dictionary<string, object>();
            unmergedMap = new Dictionary<int, PdfIndirectObject>();
            unmergedIndirectRefsMap = new Dictionary<RefKey, PdfIndirectObject>();
            mergedMap = new Dictionary<int, PdfIndirectObject>();
            mergedSet = new HashSet2<PdfIndirectObject>();
        }

        /**
        * Grabs a page from the input document
        * @param reader the reader of the document
        * @param pageNumber which page to get
        * @return the page
        */
        public override PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber) {
            if (mergeFields && !mergeFieldsInternalCall) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "getImportedPage"));
            }
            if (mergeFields) {
                ImportedPage newPage = new ImportedPage(reader, pageNumber, mergeFields);
                importedPages.Add(newPage);
            }
            if (structTreeController != null)
                structTreeController.reader = null;
            disableIndirects.Clear();
            parentObjects.Clear();
            return GetImportedPageImpl(reader, pageNumber);
        }

        virtual public PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber, bool keepTaggedPdfStructure) {
            if (mergeFields && !mergeFieldsInternalCall) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "getImportedPage"));
            }
            ImportedPage newPage = null;
            updateRootKids = false;
            if (!keepTaggedPdfStructure) {
                if (mergeFields) {
                    newPage = new ImportedPage(reader, pageNumber, mergeFields);
                    importedPages.Add(newPage);
                }
                return GetImportedPageImpl(reader, pageNumber);
            }
            if (structTreeController != null) {
                if (reader != structTreeController.reader)
                    structTreeController.SetReader(reader);
            } else {
                structTreeController = new PdfStructTreeController(reader, this);
            }
            newPage = new ImportedPage(reader, pageNumber, mergeFields);
            switch (CheckStructureTreeRootKids(newPage)) {
                case -1: //-1 - clear , update
                    ClearIndirects(reader);
                    updateRootKids = true;
                    break;
                case 0: //0 - not clear, not update
                    updateRootKids = false;
                    break;
                case 1: //1 - not clear, update
                    updateRootKids = true;
                    break;
            }
            importedPages.Add(newPage);

            disableIndirects.Clear();
            parentObjects.Clear();
            return GetImportedPageImpl(reader, pageNumber);
        }

        private void ClearIndirects(PdfReader reader)
        {
            Dictionary<RefKey, IndirectReferences> currIndirects = indirectMap[reader];
            List<RefKey> forDelete = new List<RefKey>();
            foreach (KeyValuePair<RefKey, IndirectReferences> entry in currIndirects) {
                PdfIndirectReference iRef = entry.Value.Ref;
                RefKey key = new RefKey(iRef);
                PdfIndirectObject iobj;
                if (!indirectObjects.TryGetValue(key, out iobj)) {
                    forDelete.Add(entry.Key);
                }
                else if (iobj.objecti.IsArray() || iobj.objecti.IsDictionary() || iobj.objecti.IsStream()) {
                    forDelete.Add(entry.Key);
                }
            }

            foreach (RefKey key in forDelete)
                currIndirects.Remove(key);
        }

        //0 - not clear, not update
        //-1 - clear, update
        //1 - not clear, update
        private int CheckStructureTreeRootKids(ImportedPage newPage)
        {
            //start of document;
            if (importedPages.Count == 0)
                return 1;
            bool readerExist = false;
            foreach (ImportedPage page in importedPages)
                if (page.reader.Equals(newPage.reader))
                {
                    readerExist = true;
                    break;
                }

            //add new reader;
            if (!readerExist)
                return 1;

            ImportedPage lastPage = importedPages[importedPages.Count - 1];
            bool equalReader = lastPage.reader.Equals(newPage.reader);
            //reader exist, correct order;
            if (equalReader && newPage.pageNumber > lastPage.pageNumber) {
                if (readersWithImportedStructureTreeRootKids.Contains(newPage.reader))
                    return 0;
                else
                    return 1;
            }
            //reader exist, incorrect order;
            return -1;
        }

        protected internal virtual void StructureTreeRootKidsForReaderImported(PdfReader reader) {
            readersWithImportedStructureTreeRootKids.Add(reader);
        }

        internal virtual void FixStructureTreeRoot(HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps) {
            Dictionary<PdfName, PdfObject> newClassMap = new Dictionary<PdfName, PdfObject>(activeClassMaps.Count);
            foreach (PdfName key in activeClassMaps) {
                PdfObject cm = structureTreeRoot.classes[key];
                if (cm != null)
                    newClassMap[key] = cm;
            }

            structureTreeRoot.classes = newClassMap;

            PdfArray kids = structureTreeRoot.GetAsArray(PdfName.K);
            if (kids != null)
                for (int i = 0; i < kids.Size; ++i) {
                    PdfIndirectReference iref = (PdfIndirectReference) kids.GetPdfObject(i);
                    RefKey key = new RefKey(iref);
                    if (!activeKeys.Contains(key))
                        kids.Remove(i--);
                }
        }

        virtual protected PdfImportedPage GetImportedPageImpl(PdfReader reader, int pageNumber) {
                if (currentPdfReaderInstance != null) {
                    if (currentPdfReaderInstance.Reader != reader) {
                        // TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
                        //try {
                        //    currentPdfReaderInstance.Reader.Close();
                        //    currentPdfReaderInstance.ReaderFile.Close();
                        //}
                        //catch (IOException) {
                        //    // empty on purpose
                        //}
                        currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
                    }
                }
                else {
                    currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
                }
                return currentPdfReaderInstance.GetImportedPage(pageNumber);            
            }
        
        
        /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file.
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected internal virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp, bool keepStructure, bool directRootKids) {
            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iref;
            indirects.TryGetValue(key, out iref);
            PdfObject obj = PdfReader.GetPdfObjectRelease(inp);
            if ((keepStructure) && (directRootKids))
                if (obj is PdfDictionary) {
                    PdfDictionary dict = (PdfDictionary) obj;
                    if (dict.Contains(PdfName.PG))
                        return null;
                }
            if (iref != null) {
                theRef = iref.Ref;
                if (iref.Copied) {
                    return theRef;
                }
            }
            else {
                theRef = body.PdfIndirectReference;
                iref = new IndirectReferences(theRef);
                indirects[key] =  iref;
            }
            if (obj != null && obj.IsDictionary()) {
                PdfObject type = PdfReader.GetPdfObjectRelease(((PdfDictionary)obj).Get(PdfName.TYPE));
                if (type != null && PdfName.PAGE.Equals(type)) {
                    return theRef;
                }
            }
            iref.SetCopied();
            if (obj != null) parentObjects[obj] = inp;
            PdfObject res = CopyObject(obj, keepStructure, directRootKids);
            if (obj != null && disableIndirects.Contains(obj))
                iref.Copied = false;
            if (res != null) {
                AddToBody(res, theRef);
                return theRef;
            }
            else {
                indirects.Remove(key);
                return null;
            }
        }
         /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file.
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp) {
            return CopyIndirect(inp, false, false);
        }

        /**
        * Translate a PRDictionary to a PdfDictionary. Also translate all of the
        * objects contained in it.
        */
        virtual protected PdfDictionary CopyDictionary(PdfDictionary inp, bool keepStruct, bool directRootKids) {
            PdfDictionary outp = new PdfDictionary(inp.Size);
            PdfObject type = PdfReader.GetPdfObjectRelease(inp.Get(PdfName.TYPE));

            if (keepStruct)
            {
                if ((directRootKids) && (inp.Contains(PdfName.PG)))
                {
                    PdfObject curr = inp;
                    disableIndirects.Add(curr);
                    while (parentObjects.ContainsKey(curr) && !(disableIndirects.Contains(curr))) {
                        curr = parentObjects[curr];
                        disableIndirects.Add(curr);
                    }
                    return null;
                }

                PdfName structType = inp.GetAsName(PdfName.S);
                structTreeController.AddRole(structType);
                structTreeController.AddClass(inp);
            }
            if (structTreeController != null && structTreeController.reader != null && (inp.Contains(PdfName.STRUCTPARENTS) || inp.Contains(PdfName.STRUCTPARENT))) {
                PdfName key = PdfName.STRUCTPARENT;
                if (inp.Contains(PdfName.STRUCTPARENTS)) {
                    key = PdfName.STRUCTPARENTS;
                }
                PdfObject value = inp.Get(key);
                outp.Put(key, new PdfNumber(currentStructArrayNumber));
                structTreeController.CopyStructTreeForPage((PdfNumber) value, currentStructArrayNumber++);
            }

            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                if (structTreeController != null && structTreeController.reader != null &&
                    (key.Equals(PdfName.STRUCTPARENTS) || key.Equals(PdfName.STRUCTPARENT))) {
                    continue;
                }
                if (PdfName.PAGE.Equals(type)) {
                    if (!key.Equals(PdfName.B) && !key.Equals(PdfName.PARENT)) {
                        parentObjects[value] = inp;
                        PdfObject res = CopyObject(value, keepStruct, directRootKids);
                        if (res != null)
                            outp.Put(key, res);
                    }
                }
                else {
                    PdfObject res;
                    if (tagged && value.IsIndirect() && IsStructTreeRootReference((PRIndirectReference) value))
                        res = structureTreeRoot.Reference;
                    else
                        res = CopyObject(value, keepStruct, directRootKids);
                    if (res != null)
                        outp.Put(key, res);
                }
            }
            return outp;
        }

        /**
        * Translate a PRDictionary to a PdfDictionary. Also translate all of the
        * objects contained in it.
        */
        virtual protected PdfDictionary CopyDictionary(PdfDictionary inp) {
            return CopyDictionary(inp, false, false);
        }

        /**
        * Translate a PRStream to a PdfStream. The data part copies itself.
        */
        virtual protected PdfStream CopyStream(PRStream inp) {
            PRStream outp = new PRStream(inp, null);
            
            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                parentObjects[value] = inp;
                PdfObject res = CopyObject(value);
                if (res != null)
                    outp.Put(key, res);
            }
            
            return outp;
        }
        
        
        /**
        * Translate a PRArray to a PdfArray. Also translate all of the objects contained
        * in it
        */
        virtual protected PdfArray CopyArray(PdfArray inp, bool keepStruct, bool directRootKids) {
            PdfArray outp = new PdfArray(inp.Size);
            
            foreach (PdfObject value in inp.ArrayList) {
                parentObjects[value] = inp;
                PdfObject res = CopyObject(value, keepStruct, directRootKids);
                if (res != null)
                    outp.Add(res);
            }
            return outp;
        }

        /**
        * Translate a PRArray to a PdfArray. Also translate all of the objects contained
        * in it
        */
        virtual protected PdfArray CopyArray(PdfArray inp) {
            return CopyArray(inp, false, false);
        }

        /**
        * Translate a PR-object to a Pdf-object
        */
        virtual protected internal PdfObject CopyObject(PdfObject inp, bool keepStruct, bool directRootKids) {
            if (inp == null)
                return PdfNull.PDFNULL;
            switch (inp.Type) {
                case PdfObject.DICTIONARY:
                    return CopyDictionary((PdfDictionary)inp, keepStruct, directRootKids);
                case PdfObject.INDIRECT:
                    if (!keepStruct && !directRootKids)
                        // fix for PdfSmartCopy
                        return CopyIndirect((PRIndirectReference)inp);
                    else
                        return CopyIndirect((PRIndirectReference)inp, keepStruct, directRootKids);
                case PdfObject.ARRAY:
                    return CopyArray((PdfArray)inp, keepStruct, directRootKids);
                case PdfObject.NUMBER:
                case PdfObject.NAME:
                case PdfObject.STRING:
                case PdfObject.NULL:
                case PdfObject.BOOLEAN:
                case 0://PdfIndirectReference
                    return inp;
                case PdfObject.STREAM:
                    return CopyStream((PRStream)inp);
                    //                return in;
                default:
                    if (inp.Type < 0) {
                        String lit = ((PdfLiteral)inp).ToString();
                        if (lit.Equals("true") || lit.Equals("false")) {
                            return new PdfBoolean(lit);
                        }
                        return new PdfLiteral(lit);
                    }
                    return null;
            }
        }

         /**
        * Translate a PR-object to a Pdf-object
        */
        virtual protected internal PdfObject CopyObject(PdfObject inp) {
            return CopyObject(inp, false, false);
        }

        /**
        * convenience method. Given an importedpage, set our "globals"
        */
        virtual protected int SetFromIPage(PdfImportedPage iPage) {
            int pageNum = iPage.PageNumber;
            PdfReaderInstance inst = currentPdfReaderInstance = iPage.PdfReaderInstance;
            reader = inst.Reader;
            SetFromReader(reader);
            return pageNum;
        }
        
        /**
        * convenience method. Given a reader, set our "globals"
        */
        virtual protected void SetFromReader(PdfReader reader) {
            this.reader = reader;
            
            if (!indirectMap.TryGetValue(reader, out indirects))
            {
                indirects = new Dictionary<RefKey,IndirectReferences>();
                indirectMap[reader] = indirects;
            }
        }
        /**
        * Add an imported page to our output
        * @param iPage an imported page
        * @throws IOException, BadPdfFormatException
        */
        public virtual void AddPage(PdfImportedPage iPage) {
            if (mergeFields && !mergeFieldsInternalCall) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "addPage"));
            }
            int pageNum = SetFromIPage(iPage);
            
            PdfDictionary thePage = reader.GetPageN(pageNum);
            PRIndirectReference origRef = reader.GetPageOrigRef(pageNum);
            reader.ReleasePage(pageNum);
            RefKey key = new RefKey(origRef);
            PdfIndirectReference pageRef;
            IndirectReferences iRef;
            if (indirects.TryGetValue(key, out iRef) && !iRef.Copied)
            {
                pageReferences.Add(iRef.Ref);
                iRef.SetCopied();
            }
            pageRef = CurrentPage;
            if (iRef == null) {
                iRef = new IndirectReferences(pageRef);
                indirects[key] = iRef;
            }
            iRef.SetCopied();
            if (tagged)
                structTreeRootReference = (PRIndirectReference)reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            PdfDictionary newPage = CopyDictionary(thePage);
            if (mergeFields) {
                ImportedPage importedPage = importedPages[importedPages.Count - 1];
                importedPage.annotsIndirectReference = body.PdfIndirectReference;
                newPage.Put(PdfName.ANNOTS, importedPage.annotsIndirectReference);
            }
            root.AddPage(newPage);
            iPage.SetCopied();
            ++currentPageNumber;
            pdf.PageCount = currentPageNumber;
            structTreeRootReference = null;
        }

        /**
         * Adds a blank page.
         * @param	rect The page dimension
         * @param	rotation The rotation angle in degrees
         * @since	2.1.5
         * @throws DocumentException
         */
        virtual public void AddPage(Rectangle rect, int rotation) {
            if (mergeFields && !mergeFieldsInternalCall) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.method.cannot.be.used.in.mergeFields.mode.please.use.addDocument", "addPage"));
            }
            PdfRectangle mediabox = new PdfRectangle(rect, rotation);
            PageResources resources = new PageResources();
            PdfPage page = new PdfPage(mediabox, new Dictionary<String, PdfRectangle>(), resources.Resources, 0);
            page.Put(PdfName.TABS, Tabs);
            root.AddPage(page);
            ++currentPageNumber;
            pdf.PageCount = currentPageNumber;
        }

        virtual public void AddDocument(PdfReader reader, List<int> pagesToKeep) {
            if (indirectMap.ContainsKey(reader)) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added", reader.ToString()));
            }
            reader.SelectPages(pagesToKeep, false);
            AddDocument(reader);
        }

        /**
         * Copy document fields to a destination document.
         * @param reader a document where fields are copied from.
         * @throws DocumentException
         * @throws IOException
         */
        public virtual void CopyDocumentFields(PdfReader reader) {
            if (!document.IsOpen()) {
                throw new DocumentException(
                    MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }

            if (indirectMap.ContainsKey(reader)) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added",
                    reader.ToString()));
            }

            if (!reader.IsOpenedWithFullPermissions)
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));

            if (!mergeFields)
                throw new ArgumentException(
                    MessageLocalization.GetComposedMessage(
                        "1.method.can.be.only.used.in.mergeFields.mode.please.use.addDocument", "copyDocumentFields"));

            indirects = new Dictionary<RefKey, IndirectReferences>();
            indirectMap[reader] = indirects;

            reader.ConsolidateNamedDestinations();
            reader.ShuffleSubsetNames();
            if (tagged && PdfStructTreeController.CheckTagged(reader)) {
                structTreeRootReference = (PRIndirectReference)reader.Catalog.Get(PdfName.STRUCTTREEROOT);
                if (structTreeController != null) {
                    if (reader != structTreeController.reader)
                        structTreeController.SetReader(reader);
                } else {
                    structTreeController = new PdfStructTreeController(reader, this);
                }
            }

            IList<PdfObject> annotationsToBeCopied = new List<PdfObject>();

            for (int i = 1; i <= reader.NumberOfPages; i++) {
                PdfDictionary page = reader.GetPageNRelease(i);
                if (page != null && page.Contains(PdfName.ANNOTS)) {
                    PdfArray annots = page.GetAsArray(PdfName.ANNOTS);
                    if (annots != null && annots.Size > 0) {
                        if (importedPages.Count < i)
                            throw new DocumentException(
                                MessageLocalization.GetComposedMessage("there.are.not.enough.imported.pages.for.copied.fields"));
                        indirectMap[reader][new RefKey(reader.pageRefs.GetPageOrigRef(i))] = new IndirectReferences(pageReferences[i - 1]);
                        for (int j = 0; j < annots.Size; j++) {
                            PdfDictionary annot = annots.GetAsDict(j);
                            if (annot != null) {
                                annot.Put(annotId, new PdfNumber(++annotIdCnt));
                                annotationsToBeCopied.Add(annots.GetPdfObject(j));
                            }
                        }
                    }
                }
            }

            foreach (PdfObject annot in annotationsToBeCopied) {
                CopyObject(annot);
            }

            if (tagged && structTreeController != null)
                structTreeController.AttachStructTreeRootKids(null);

            AcroFields acro = reader.AcroFields;
            bool needapp = !acro.GenerateAppearances;
            if (needapp)
                needAppearances = true;
            fields.Add(acro);
            UpdateCalculationOrder(reader);
            structTreeRootReference = null;
        }

        virtual public void AddDocument(PdfReader reader) {
            if (!document.IsOpen()) {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }
            if (indirectMap.ContainsKey(reader)) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("document.1.has.already.been.added", reader.ToString()));
            }
            if (!reader.IsOpenedWithFullPermissions)
                throw new BadPasswordException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            if (mergeFields) {
                reader.ConsolidateNamedDestinations();
                reader.ShuffleSubsetNames();
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    PdfDictionary page = reader.GetPageNRelease(i);
                    if (page != null && page.Contains(PdfName.ANNOTS)) {
                        PdfArray annots = page.GetAsArray(PdfName.ANNOTS);
                        if (annots != null) {
                            for (int j = 0; j < annots.Size; j++) {
                                PdfDictionary annot = annots.GetAsDict(j);
                                if (annot != null)
                                    annot.Put(annotId, new PdfNumber(++annotIdCnt));
                            }
                        }
                    }
                }
                AcroFields acro = reader.AcroFields;
                // when a document with NeedAppearances is encountered, the flag is set
                // in the resulting document.
                bool needapp = !acro.GenerateAppearances;
                if (needapp)
                    needAppearances = true;
                fields.Add(acro);
                UpdateCalculationOrder(reader);
            }
            bool tagged = this.tagged && PdfStructTreeController.CheckTagged(reader);
            mergeFieldsInternalCall = true;
            for (int i = 1; i <= reader.NumberOfPages; i++) {
                AddPage(GetImportedPage(reader, i, tagged));
            }
            mergeFieldsInternalCall = false;
        }


        public override PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa) {
            return AddToBody(objecta, refa, false);
        }

        public override PdfIndirectObject AddToBody(PdfObject objecta, PdfIndirectReference refa, bool formBranching) {
            if (formBranching) {
                UpdateReferences(objecta);
            }
            PdfIndirectObject iobj;
            if ((tagged || mergeFields) && indirectObjects != null && (objecta.IsArray() || objecta.IsDictionary() || objecta.IsStream() || objecta.IsNull()))
            {
                RefKey key = new RefKey(refa);
                PdfIndirectObject obj;
                if (!indirectObjects.TryGetValue(key, out obj)) {
                    obj = new PdfIndirectObject(refa, objecta, this);
                    indirectObjects[key] = obj;
                }
                iobj = obj;
            } else {
                iobj = base.AddToBody(objecta, refa);
            }
            if (mergeFields && objecta.IsDictionary()) {
                PdfNumber annotId = ((PdfDictionary)objecta).GetAsNumber(PdfCopy.annotId);
                if (annotId != null) {
                    if (formBranching) {
                        mergedMap[annotId.IntValue] = iobj;
                        mergedSet.Add(iobj);
                    } else {
                        unmergedMap[annotId.IntValue] = iobj;
                        unmergedIndirectRefsMap[new RefKey(iobj.Number, iobj.Generation)] = iobj;
                    }
                }
            }
            return iobj;
        }

        protected internal override void CacheObject(PdfIndirectObject iobj) {
            if ((tagged || mergeFields) && indirectObjects != null) {
                savedObjects.Add(iobj);
                RefKey key = new RefKey(iobj.Number, iobj.Generation);
                if (!indirectObjects.ContainsKey(key))
                    indirectObjects[key] = iobj;
            }
        }

        internal override void FlushTaggedObjects() {
            try
            {
                FixTaggedStructure();
            }
            catch (InvalidCastException)
            { }
            finally
            {
                FlushIndirectObjects();
            }
        }

        internal override void FlushAcroFields() {
            if (mergeFields) {
                try {
                    //save annotations that appear just at page level (comments, popups)
                    foreach (ImportedPage page in importedPages) {
                        PdfDictionary pageDict = page.reader.GetPageN(page.pageNumber);
                        if (pageDict != null) {
                            PdfArray pageFields = pageDict.GetAsArray(PdfName.ANNOTS);
                            if (pageFields == null || pageFields.Size == 0)
                                continue;
                            foreach (AcroFields.Item items in page.reader.AcroFields.Fields.Values) {
                                foreach (PdfIndirectReference @ref in items.widget_refs) {
                                    pageFields.ArrayList.Remove(@ref);
                                }
                            }
                            indirectMap.TryGetValue(page.reader, out indirects);
                            foreach (PdfObject @ref in pageFields.ArrayList)
                                page.mergedFields.Add(CopyObject(@ref));
                        }
                    }
                    //ok, remove old fields and build create new one
                    foreach (PdfReader reader in indirectMap.Keys) {
                        reader.RemoveFields();
                    }
                    MergeFields();
                    CreateAcroForms();
                } catch (InvalidCastException) {
                } finally {
                    if (!tagged)
                        FlushIndirectObjects();
                }
            }
        }

        virtual protected void FixTaggedStructure()
        {
            Dictionary<int, PdfIndirectReference> numTree = structureTreeRoot.NumTree;
            HashSet2<RefKey> activeKeys = new HashSet2<RefKey>();
            List<PdfIndirectReference> actives = new List<PdfIndirectReference>();
            int pageRefIndex = 0;

            if (mergeFields && acroForm != null) {
                actives.Add(acroForm);
                activeKeys.Add(new RefKey(acroForm));
            }
            foreach (PdfIndirectReference page in pageReferences) {
                actives.Add(page);
                activeKeys.Add(new RefKey(page));
            }

            //from end, because some objects can appear on several pages because of MCR (out16.pdf)
            for (int i = numTree.Count - 1; i >= 0; --i) {
                PdfIndirectReference currNum = numTree[i];
                RefKey numKey = new RefKey(currNum);
                PdfObject obj = indirectObjects[numKey].objecti;
                if (obj.IsDictionary()) {
                    bool addActiveKeys = false;
                    if (pageReferences.Contains((PdfIndirectReference) ((PdfDictionary) obj).Get(PdfName.PG))) {
                        addActiveKeys = true;
                    }
                    else {
                        PdfDictionary k = PdfStructTreeController.GetKDict((PdfDictionary) obj);
                        if (k != null && pageReferences.Contains((PdfIndirectReference) k.Get(PdfName.PG))) {
                            addActiveKeys = true;
                        }
                    }
                    if (addActiveKeys) {
                        activeKeys.Add(numKey);
                        actives.Add(currNum);
                    }
                    else {
                        numTree.Remove(i);
                    }
                }
                else if (obj.IsArray()) {
                    activeKeys.Add(numKey);
                    actives.Add(currNum);
                    PdfArray currNums = (PdfArray) obj;
                    PdfIndirectReference currPage = pageReferences[pageRefIndex++];
                    actives.Add(currPage);
                    activeKeys.Add(new RefKey(currPage));
                    PdfIndirectReference prevKid = null;
                    for (int j = 0; j < currNums.Size; j++) {
                        PdfIndirectReference currKid = (PdfIndirectReference) currNums.GetDirectObject(j);
                        if (currKid.Equals(prevKid))
                            continue;
                        RefKey kidKey = new RefKey(currKid);
                        activeKeys.Add(kidKey);
                        actives.Add(currKid);

                        PdfIndirectObject iobj = indirectObjects[kidKey];
                        if (iobj.objecti.IsDictionary()) {
                            PdfDictionary dict = (PdfDictionary) iobj.objecti;
                            PdfIndirectReference pg = (PdfIndirectReference) dict.Get(PdfName.PG);
                            //if pg is real page - do nothing, else set correct pg and remove first MCID if exists
                            if (pg != null && !pageReferences.Contains(pg) && !pg.Equals(currPage)) {
                                dict.Put(PdfName.PG, currPage);
                                PdfArray kids = dict.GetAsArray(PdfName.K);
                                if (kids != null) {
                                    PdfObject firstKid = kids.GetDirectObject(0);
                                    if (firstKid.IsNumber()) kids.Remove(0);
                                }
                            }
                        }
                        prevKid = currKid;
                    }
                }
            }

            HashSet2<PdfName> activeClassMaps = new HashSet2<PdfName>();
            //collect all active objects from current active set (include kids, classmap, attributes)
            FindActives(actives, activeKeys, activeClassMaps);
            //find parents of active objects
            List<PdfIndirectReference> newRefs = FindActiveParents(activeKeys);
            //find new objects with incorrect Pg; if find, set Pg from first correct kid. This correct kid must be.
            FixPgKey(newRefs, activeKeys);
            //remove unused kids of StructTreeRoot and remove unused objects from class map
            FixStructureTreeRoot(activeKeys, activeClassMaps);
            List<RefKey> inactiveKeys = new List<RefKey>();
            foreach(KeyValuePair<RefKey, PdfIndirectObject> entry in indirectObjects) {
                if (!activeKeys.Contains(entry.Key)) {
                    inactiveKeys.Add(entry.Key);
                }
                else {
                    if (entry.Value.objecti.IsArray()) {
                        RemoveInactiveReferences((PdfArray)entry.Value.objecti, activeKeys);
                    } else if (entry.Value.objecti.IsDictionary()) {
                        PdfObject kids = ((PdfDictionary)entry.Value.objecti).Get(PdfName.K);
                        if (kids != null && kids.IsArray())
                            RemoveInactiveReferences((PdfArray)kids, activeKeys);
                    }
                }
            }

            //because of concurrent modification detected by CLR
            foreach (RefKey key in inactiveKeys)
                indirectObjects[key] = null;
        }

        private void RemoveInactiveReferences(PdfArray array, HashSet2<RefKey> activeKeys) {
            for (int i = 0; i < array.Size; ++i) {
                PdfObject obj = array.GetPdfObject(i);
                if ((obj.Type == 0 && !activeKeys.Contains(new RefKey((PdfIndirectReference)obj))) ||
                        (obj.IsDictionary() && ContainsInactivePg((PdfDictionary)obj, activeKeys)))
                    array.Remove(i--);
            }
        }

        private bool ContainsInactivePg(PdfDictionary dict, HashSet2<RefKey> activeKeys) {
            PdfObject pg = dict.Get(PdfName.PG);
            if (pg != null && !activeKeys.Contains(new RefKey((PdfIndirectReference)pg)))
                return true;
            return false;
        }

        //return new found objects
        private List<PdfIndirectReference> FindActiveParents(HashSet2<RefKey> activeKeys){
            List<PdfIndirectReference> newRefs = new List<PdfIndirectReference>();
            List<RefKey> tmpActiveKeys = new List<RefKey>(activeKeys);
            for (int i = 0; i < tmpActiveKeys.Count; ++i) {
                PdfIndirectObject iobj;
                if (!indirectObjects.TryGetValue(tmpActiveKeys[i], out iobj)
                    || !iobj.objecti.IsDictionary())
                    continue;
                PdfObject parent = ((PdfDictionary)iobj.objecti).Get(PdfName.P);
                if (parent != null && parent.Type == 0) {
                    RefKey key = new RefKey((PdfIndirectReference)parent);
                    if (!activeKeys.Contains(key)) {
                        activeKeys.Add(key);
                        tmpActiveKeys.Add(key);
                        newRefs.Add((PdfIndirectReference) parent);
                    }
                }
            }
            return newRefs;
        }

        private void FixPgKey(List<PdfIndirectReference> newRefs, HashSet2<RefKey> activeKeys){
            foreach (PdfIndirectReference iref in newRefs) {
                PdfIndirectObject iobj;
                if (!indirectObjects.TryGetValue(new RefKey(iref), out iobj)
                    || !iobj.objecti.IsDictionary())
                    continue;
                PdfDictionary dict = (PdfDictionary)iobj.objecti;
                PdfObject pg = dict.Get(PdfName.PG);
                if (pg == null || activeKeys.Contains(new RefKey((PdfIndirectReference)pg))) continue;
                PdfArray kids = dict.GetAsArray(PdfName.K);
                if (kids == null) continue;
                for (int i = 0; i < kids.Size; ++i) {
                    PdfObject obj = kids.GetPdfObject(i);
                    if (obj.Type != 0) {
                        kids.Remove(i--);
                    } else {
                        PdfIndirectObject kid;
                        if (indirectObjects.TryGetValue(new RefKey((PdfIndirectReference)obj), out kid)
                            && kid.objecti.IsDictionary())
                        {
                            PdfObject kidPg = ((PdfDictionary)kid.objecti).Get(PdfName.PG);
                            if (kidPg != null && activeKeys.Contains(new RefKey((PdfIndirectReference)kidPg))) {
                                dict.Put(PdfName.PG, kidPg);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void FindActives(List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps){
            //collect all active objects from current active set (include kids, classmap, attributes)
            for (int i = 0; i < actives.Count; ++i) {
                RefKey key = new RefKey(actives[i]);
                PdfIndirectObject iobj;
                if (!indirectObjects.TryGetValue(key, out iobj) || iobj.objecti == null)
                    continue;
                switch (iobj.objecti.Type){
                    case 0://PdfIndirectReference
                        FindActivesFromReference((PdfIndirectReference)iobj.objecti, actives, activeKeys);
                        break;
                    case PdfObject.ARRAY:
                        FindActivesFromArray((PdfArray)iobj.objecti, actives, activeKeys, activeClassMaps);
                        break;
                    case PdfObject.DICTIONARY:
                    case PdfObject.STREAM:
                        FindActivesFromDict((PdfDictionary)iobj.objecti, actives, activeKeys, activeClassMaps);
                        break;
                }
            }
        }

        private void FindActivesFromReference(PdfIndirectReference iref, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys) {
            RefKey key = new RefKey(iref);
            PdfIndirectObject iobj;
            if (indirectObjects.TryGetValue(key, out iobj)
                && iobj.objecti.IsDictionary() && ContainsInactivePg((PdfDictionary) iobj.objecti, activeKeys))
                return;

            if(!activeKeys.Contains(key)) {
                activeKeys.Add(key);
                actives.Add(iref);
            }
        }

        private void FindActivesFromArray(PdfArray array, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys, HashSet2<PdfName> activeClassMaps) {
            foreach (PdfObject obj in array) {
                switch (obj.Type) {
                    case 0://PdfIndirectReference
                        FindActivesFromReference((PdfIndirectReference)obj, actives, activeKeys);
                        break;
                    case PdfObject.ARRAY:
                        FindActivesFromArray((PdfArray)obj, actives, activeKeys, activeClassMaps);
                        break;
                    case PdfObject.DICTIONARY:
                    case PdfObject.STREAM:
                        FindActivesFromDict((PdfDictionary)obj, actives, activeKeys, activeClassMaps);
                        break;
                }
            }
        }

        private void FindActivesFromDict(PdfDictionary dict, List<PdfIndirectReference> actives, HashSet2<RefKey> activeKeys,  HashSet2<PdfName> activeClassMaps) {
            if (ContainsInactivePg(dict, activeKeys))
                return;
            foreach (PdfName key in dict.Keys) {
                PdfObject obj = dict.Get(key);
                if (key.Equals(PdfName.P))
                    continue;
                else if (key.Equals(PdfName.C)) { //classmap
                    if (obj.IsArray()) {
                        foreach (PdfObject cm in (PdfArray)obj) {
                            if (cm.IsName())
                                activeClassMaps.Add((PdfName)cm);
                        }
                    }
                    else if (obj.IsName()) activeClassMaps.Add((PdfName)obj);
                    continue;
                }
                switch (obj.Type) {
                    case 0://PdfIndirectReference
                        FindActivesFromReference((PdfIndirectReference)obj, actives, activeKeys);
                        break;
                    case PdfObject.ARRAY:
                        FindActivesFromArray((PdfArray)obj, actives, activeKeys, activeClassMaps);
                        break;
                    case PdfObject.DICTIONARY:
                    case PdfObject.STREAM:
                        FindActivesFromDict((PdfDictionary)obj, actives, activeKeys, activeClassMaps);
                        break;
                }
            }
        }

        virtual protected void FlushIndirectObjects()
        {
            foreach (PdfIndirectObject iobj in savedObjects)
                indirectObjects.Remove(new RefKey(iobj.Number, iobj.Generation));
            HashSet2<RefKey> inactives = new HashSet2<RefKey>();
            foreach (KeyValuePair<RefKey, PdfIndirectObject> entry in indirectObjects) {
                if (entry.Value != null)
                    WriteObjectToBody(entry.Value);
                else
                    inactives.Add(entry.Key);
            }
            List<PdfBody.PdfCrossReference> xrefs = new List<PdfBody.PdfCrossReference>();
            foreach (PdfBody.PdfCrossReference xref in body.xrefs.Keys)
                xrefs.Add(xref);
            foreach (PdfBody.PdfCrossReference cr in xrefs) {
                if (cr == null)
                    continue;
                RefKey key = new RefKey(cr.Refnum, 0);
                if (inactives.Contains(key))
                    body.xrefs.Remove(cr);
            }
            indirectObjects = null;
        }


        private void WriteObjectToBody(PdfIndirectObject objecta) {
            bool skipWriting = false;
            if (mergeFields) {
                UpdateAnnotationReferences(objecta.objecti);
                if (objecta.objecti.IsDictionary() || objecta.objecti.IsStream()) {
                    PdfDictionary dictionary = (PdfDictionary)objecta.objecti;
                    if (unmergedIndirectRefsMap.ContainsKey(new RefKey(objecta.Number, objecta.Generation))) {
                        PdfNumber annotId = dictionary.GetAsNumber(PdfCopy.annotId);
                        if (annotId != null && mergedMap.ContainsKey(annotId.IntValue))
                            skipWriting = true;
                    }
                    if (mergedSet.Contains(objecta)) {
                        PdfNumber annotId = dictionary.GetAsNumber(PdfCopy.annotId);
                        if (annotId != null) {
                            PdfIndirectObject unmerged;
                            if (unmergedMap.TryGetValue(annotId.IntValue, out unmerged) && unmerged.objecti.IsDictionary()) {
                                PdfNumber structParent = ((PdfDictionary)unmerged.objecti).GetAsNumber(PdfName.STRUCTPARENT);
                                if (structParent != null) {
                                    dictionary.Put(PdfName.STRUCTPARENT, structParent);
                                }
                            }
                        }
                    }
                }
            }
            if (!skipWriting) {
                PdfDictionary dictionary = null;
                PdfNumber annotId = null;
                if (mergeFields && objecta.objecti.IsDictionary()) {
                    dictionary = (PdfDictionary)objecta.objecti;
                    annotId = dictionary.GetAsNumber(PdfCopy.annotId);
                    if (annotId != null)
                        dictionary.Remove(PdfCopy.annotId);
                }
                body.Add(objecta.objecti, objecta.Number, objecta.Generation, true);
                if (annotId != null) {
                    dictionary.Put(PdfCopy.annotId, annotId);
                }
            }
        }

        private void UpdateAnnotationReferences(PdfObject obj) {
            if (obj.IsArray()) {
                PdfArray array = (PdfArray)obj;
                for (int i = 0; i < array.Size; i++) {
                    PdfObject o = array.GetPdfObject(i);
                    if (o != null && o.Type == 0) {
                        PdfIndirectObject entry;
                        bool contains = unmergedIndirectRefsMap.TryGetValue(new RefKey((PdfIndirectReference) o), out entry);
                        if (contains) {
                            if (entry.objecti.IsDictionary()) {
                                PdfNumber annotId = ((PdfDictionary) entry.objecti).GetAsNumber(PdfCopy.annotId);
                                if (annotId != null) {
                                    PdfIndirectObject merged;
                                    if (mergedMap.TryGetValue(annotId.IntValue, out merged)) {
                                        array.Set(i, merged.IndirectReference);
                                    }
                                }
                            }
                        }
                    } else {
                        UpdateAnnotationReferences(o);
                    }
                }
            } else if (obj.IsDictionary() || obj.IsStream()) {
                PdfDictionary dictionary = (PdfDictionary)obj;
                List<PdfName> keys = new List<PdfName>(dictionary.Keys);
                foreach (PdfName key in keys) {
                    PdfObject o = dictionary.Get(key);
                    if (o != null && o.Type == 0) {
                        PdfIndirectObject entry;
                        bool contains = unmergedIndirectRefsMap.TryGetValue(new RefKey((PdfIndirectReference) o), out entry);
                        if (contains) {
                            if (entry.objecti.IsDictionary()) {
                                PdfNumber annotId = ((PdfDictionary) entry.objecti).GetAsNumber(PdfCopy.annotId);
                                if (annotId != null) {
                                    PdfIndirectObject merged;
                                    if (mergedMap.TryGetValue(annotId.IntValue, out merged)) {
                                        dictionary.Put(key, merged.IndirectReference);
                                    }
                                }
                            }
                        }
                    } else {
                        UpdateAnnotationReferences(o);
                    }
                }
            }
        }

        private void UpdateCalculationOrder(PdfReader reader) {
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acro = catalog.GetAsDict(PdfName.ACROFORM);
            if (acro == null)
                return;
            PdfArray co = acro.GetAsArray(PdfName.CO);
            if (co == null || co.Size == 0)
                return;
            AcroFields af = reader.AcroFields;
            for (int k = 0; k < co.Size; ++k) {
                PdfObject obj = co.GetPdfObject(k);
                if (obj == null || !obj.IsIndirect())
                    continue;
                String name = GetCOName(reader, (PRIndirectReference)obj);
                if (af.GetFieldItem(name) == null)
                    continue;
                name = "." + name;
                if (calculationOrder.Contains(name))
                    continue;
                calculationOrder.Add(name);
            }
        }

        private static String GetCOName(PdfReader reader, PRIndirectReference refa) {
            String name = "";
            while (refa != null) {
                PdfObject obj = PdfReader.GetPdfObject(refa);
                if (obj == null || obj.Type != PdfObject.DICTIONARY)
                    break;
                PdfDictionary dic = (PdfDictionary)obj;
                PdfString t = dic.GetAsString(PdfName.T);
                if (t != null) {
                    name = t.ToUnicodeString()+ "." + name;
                }
                refa = (PRIndirectReference)dic.Get(PdfName.PARENT);
            }
            if (name.EndsWith("."))
                name = name.Substring(0, name.Length - 2);
            return name;
        }

        private void MergeFields() {
            int pageOffset = 0;
            for (int k = 0; k < fields.Count; ++k) {
                AcroFields af = fields[k];
                IDictionary<String, AcroFields.Item> fd = af.Fields;
                if (pageOffset < importedPages.Count && importedPages[pageOffset].reader == af.reader) {
                    AddPageOffsetToField(fd, pageOffset);
                    pageOffset += af.reader.NumberOfPages;
                }
                MergeWithMaster(fd);
            }
        }

        private void AddPageOffsetToField(IDictionary<String, AcroFields.Item> fd, int pageOffset) {
            if (pageOffset == 0)
                return;
            foreach (AcroFields.Item item in fd.Values) {
                for (int k = 0; k < item.Size; ++k) {
                    int p = item.GetPage(k);
                    item.ForcePage(k, p + pageOffset);
                }
            }
        }

        private void MergeWithMaster(IDictionary<String, AcroFields.Item> fd) {
            foreach (KeyValuePair<String, AcroFields.Item> entry in fd) {
                MergeField(entry.Key, entry.Value);
            }
        }

        internal void MergeField(String name, AcroFields.Item item) {
            Dictionary<String, Object> map = fieldTree;
            StringTokenizer tk = new StringTokenizer(name, ".");
            if (!tk.HasMoreTokens())
                return;
            while (true) {
                String s = tk.NextToken();
                Object obj;
                map.TryGetValue(s, out obj);
                if (tk.HasMoreTokens()) {
                    if (obj == null) {
                        obj = new Dictionary<String, Object>();
                        map[s] = obj;
                        map = (Dictionary<String, Object>)obj;
                        continue;
                    } else if (obj is Dictionary<String, Object>) {
                        map = (Dictionary<String, Object>) obj;
                    } else {
                        return;
                    }
                } else {
                    if (obj is Dictionary<String, Object>)
                        return;
                    PdfDictionary merged = item.GetMerged(0);

                    if (obj == null) {
                        PdfDictionary field = new PdfDictionary();
                        if (PdfName.SIG.Equals(merged.Get(PdfName.FT)))
                            hasSignature = true;
                        foreach (PdfName key in merged.Keys) {
                            if (fieldKeys.Contains(key))
                                field.Put(key, merged.Get(key));
                        }
                        List<Object> list = new List<Object>();
                        list.Add(field);
                        CreateWidgets(list, item);
                        map[s] = list;
                    }
                    else {
                        List<Object> list = (List<object>) obj;
                        PdfDictionary field = (PdfDictionary) list[0];
                        PdfName type1 = field.GetAsName(PdfName.FT);
                        PdfName type2 = merged.GetAsName(PdfName.FT);
                        if (type1 == null || !type1.Equals(type2)) {
                            return;
                        }
                        int flag1 = 0;
                        PdfNumber f1 = field.GetAsNumber(PdfName.FF);
                        if (f1 != null) {
                            flag1 = f1.IntValue;
                        }
                        int flag2 = 0;
                        PdfNumber f2 = merged.GetAsNumber(PdfName.FF);
                        if (f2 != null) {
                            flag2 = f2.IntValue;
                        }
                        if (type1.Equals(PdfName.BTN)) {
                            if (((flag1 ^ flag2) & PdfFormField.FF_PUSHBUTTON) != 0) {
                                return;
                            }
                            if ((flag1 & PdfFormField.FF_PUSHBUTTON) == 0 &&
                                ((flag1 ^ flag2) & PdfFormField.FF_RADIO) != 0) {
                                return;
                            }
                        } else if (type1.Equals(PdfName.CH)) {
                            if (((flag1 ^ flag2) & PdfFormField.FF_COMBO) != 0) {
                                return;
                            }
                        }
                        CreateWidgets(list, item);
                    }
                    
                    return;
                }
            }
        }

        private void CreateWidgets(List<Object> list, AcroFields.Item item) {
            for (int k = 0; k < item.Size; ++k) {
                list.Add(item.GetPage(k));
                PdfDictionary merged = item.GetMerged(k);
                PdfObject dr = merged.Get(PdfName.DR);
                if (dr != null)
                    PdfFormField.MergeResources(resources, (PdfDictionary)PdfReader.GetPdfObject(dr));
                PdfDictionary widget = new PdfDictionary();
                foreach (Object element in merged.Keys) {
                    PdfName key = (PdfName)element;
                    if (widgetKeys.Contains(key))
                        widget.Put(key, merged.Get(key));
                }
                widget.Put(iTextTag, new PdfNumber(item.GetTabOrder(k) + 1));
                list.Add(widget);
            }
        }

        private PdfObject Propagate(PdfObject obj) {
            if (obj == null) {
                return new PdfNull();
            } else if (obj.IsArray()) {
                PdfArray a = (PdfArray)obj;
                for (int i = 0; i < a.Size; i++) {
                    a.Set(i, Propagate(a.GetPdfObject(i)));
                }
                return a;
            } else if (obj.IsDictionary() || obj.IsStream()) {
                PdfDictionary d = (PdfDictionary)obj;
                List<PdfName> keys = new List<PdfName>(d.Keys);
                foreach (PdfName key in keys) {
                    d.Put(key, Propagate(d.Get(key)));
                }
                return d;
            } else if (obj.IsIndirect()) {
                obj = PdfReader.GetPdfObject(obj);
                return AddToBody(Propagate(obj)).IndirectReference;
            } else
                return obj;
        }

        private void CreateAcroForms() {
            if (fieldTree.Count == 0) {
                //write annotations that appear just at page level (comments, popups)
                foreach (ImportedPage importedPage in importedPages) {
                    if (importedPage.mergedFields.Size > 0)
                        AddToBody(importedPage.mergedFields, importedPage.annotsIndirectReference);
                }
                return;
            }
            PdfDictionary form = new PdfDictionary();
            form.Put(PdfName.DR, Propagate(resources));

            if (needAppearances) {
                form.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
            }
            form.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            tabOrder = new Dictionary<PdfArray, List<int>>();
            calculationOrderRefs = new List<Object>();
            foreach (string order in calculationOrder)
                calculationOrderRefs.Add(order);
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
            this.acroForm = AddToBody(form).IndirectReference;
            foreach (ImportedPage importedPage in importedPages) {
                AddToBody(importedPage.mergedFields, importedPage.annotsIndirectReference);
            }
        }

        private void UpdateReferences(PdfObject obj) {
            if (obj.IsDictionary() || obj.IsStream()) {
                PdfDictionary dictionary = (PdfDictionary)obj;
                List<PdfName> keys = new List<PdfName>(dictionary.Keys);
                foreach (PdfName key in keys) {
                    PdfObject o = dictionary.Get(key);
                    if (o.IsIndirect()) {
                        PdfReader reader = ((PRIndirectReference)o).Reader;
                        Dictionary<RefKey,IndirectReferences> indirects = indirectMap[reader];
                        IndirectReferences indRef;
                        if (indirects.TryGetValue(new RefKey((PRIndirectReference)o), out indRef)) {
                            dictionary.Put(key, indRef.Ref);
                        }
                    } else {
                        UpdateReferences(o);
                    }
                }
            } else if (obj.IsArray()) {
                PdfArray array = (PdfArray)obj;
                for (int i = 0; i < array.Size; i++) {
                    PdfObject o = array.GetPdfObject(i);
                    if (o.IsIndirect()) {
                        PdfReader reader = ((PRIndirectReference)o).Reader;
                        Dictionary<RefKey,IndirectReferences> indirects = indirectMap[reader];
                        IndirectReferences indRef;
                        if (indirects.TryGetValue(new RefKey((PRIndirectReference)o), out indRef)) {
                            array.Set(i, indRef.Ref);
                        }
                    } else {
                        UpdateReferences(o);
                    }
                }
            }
        }

        private PdfArray BranchForm(Dictionary<String, Object> level, PdfIndirectReference parent, String fname) {
            PdfArray arr = new PdfArray();
            foreach (KeyValuePair<String, Object> entry in level) {
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
                if (obj is Dictionary<String, Object>) {
                    dic.Put(PdfName.KIDS, BranchForm((Dictionary<String, Object>) obj, ind, fname2));
                    arr.Add(ind);
                    AddToBody(dic, ind, true);
                } else {
                    List<Object> list = (List<Object>)obj;
                    dic.MergeDifferent((PdfDictionary) list[0]);
                    if (list.Count == 3) {
                        dic.MergeDifferent((PdfDictionary)list[2]);
                        int page = (int)list[1];
                        PdfArray annots = importedPages[page - 1].mergedFields;
                        PdfNumber nn = (PdfNumber)dic.Get(iTextTag);
                        dic.Remove(iTextTag);
                        dic.Put(PdfName.TYPE, PdfName.ANNOT);
                        AdjustTabOrder(annots, ind, nn);
                    } else {
                        PdfDictionary field = (PdfDictionary)list[0];
                        PdfArray kids = new PdfArray();
                        for (int k = 1; k < list.Count; k += 2) {
                            int page = (int)list[k];
                            PdfArray annots = importedPages[page - 1].mergedFields;
                            PdfDictionary widget = new PdfDictionary();
                            widget.Merge((PdfDictionary)list[k + 1]);
                            widget.Put(PdfName.PARENT, ind);
                            PdfNumber nn = (PdfNumber)widget.Get(iTextTag);
                            widget.Remove(iTextTag);
                            if (PdfCopy.IsTextField(field)) {
                                PdfString v = field.GetAsString(PdfName.V);
                                PdfObject ap = widget.GetDirectObject(PdfName.AP);
                                if (v != null && ap != null) {
                                    if (!mergedTextFields.ContainsKey(list)) {
                                        mergedTextFields[list] = v;
                                    } else {
                                        try {
                                            TextField tx = new TextField(this, null, null);
                                            fields[0].DecodeGenericDictionary(widget, tx);
                                            Rectangle box =
                                                PdfReader.GetNormalizedRectangle(widget.GetAsArray(PdfName.RECT));
                                            if (tx.Rotation == 90 || tx.Rotation == 270) {
                                                box = box.Rotate();
                                            }
                                            tx.Box = box;
                                            tx.Text = mergedTextFields[list].ToUnicodeString();
                                            PdfAppearance app = tx.GetAppearance();
                                            ((PdfDictionary) ap).Put(PdfName.N, app.IndirectReference);
                                        }
                                        catch (DocumentException ex) {
                                            //do nothing
                                        }
                                    }
                                }
                            } else if (PdfCopy.IsCheckButton(field)) {
                                PdfName v = field.GetAsName(PdfName.V);
                                PdfName as_ = widget.GetAsName(PdfName.AS);
                                if (v != null && as_ != null)
                                    widget.Put(PdfName.AS, v);
                            } else if (PdfCopy.IsRadioButton(field)) {
                                PdfName v = field.GetAsName(PdfName.V);
                                PdfName as_ = widget.GetAsName(PdfName.AS);
                                if (v != null && as_ != null && !as_.Equals(GetOffStateName(widget))) {
                                    if (!mergedRadioButtons.Contains(list)) {
                                        mergedRadioButtons.Add(list);
                                        widget.Put(PdfName.AS, v);
                                    } else {
                                        widget.Put(PdfName.AS, GetOffStateName(widget));
                                    }
                                }
                            }
                            widget.Put(PdfName.TYPE, PdfName.ANNOT);
                            PdfIndirectReference wref = AddToBody(widget, PdfIndirectReference, true).IndirectReference;
                            AdjustTabOrder(annots, wref, nn);
                            kids.Add(wref);
                        }
                        dic.Put(PdfName.KIDS, kids);
                    }
                    arr.Add(ind);
                    AddToBody(dic, ind, true);
                }
            }
            return arr;
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
                tabOrder[annots] = t;
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


       
        /*
        * the getCatalog method is part of PdfWriter.
        * we wrap this so that we can extend it
        */
        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj) {
            PdfDictionary theCat = pdf.GetCatalog(rootObj);
            BuildStructTreeRootForTagged(theCat);
            if (fieldArray != null) {
                AddFieldResources(theCat);
            } else if (mergeFields && acroForm != null) {
                theCat.Put(PdfName.ACROFORM, acroForm);
            } 
            return theCat;
        }

        virtual protected bool IsStructTreeRootReference(PdfIndirectReference prRef)
        {
            if (prRef == null || structTreeRootReference == null)
                return false;
            return prRef.Number == structTreeRootReference.Number &&
                   prRef.Generation == structTreeRootReference.Generation;
        }

        private void AddFieldResources(PdfDictionary catalog) {
            if (fieldArray == null)
                return;
            PdfDictionary acroForm = new PdfDictionary();
            catalog.Put(PdfName.ACROFORM, acroForm);
            acroForm.Put(PdfName.FIELDS, fieldArray);
            acroForm.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            if (fieldTemplates.Count == 0)
                return;
            PdfDictionary dr = new PdfDictionary();
            acroForm.Put(PdfName.DR, dr);
            foreach (PdfTemplate template in fieldTemplates) {
                PdfFormField.MergeResources(dr, (PdfDictionary)template.Resources);
            }
            PdfDictionary fonts = dr.GetAsDict(PdfName.FONT);
            if (fonts == null) {
                fonts = new PdfDictionary();
                dr.Put(PdfName.FONT, fonts);
            }
            if (!fonts.Contains(PdfName.HELV)) {
                PdfDictionary dic = new PdfDictionary(PdfName.FONT);
                dic.Put(PdfName.BASEFONT, PdfName.HELVETICA);
                dic.Put(PdfName.ENCODING, PdfName.WIN_ANSI_ENCODING);
                dic.Put(PdfName.NAME, PdfName.HELV);
                dic.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                fonts.Put(PdfName.HELV, AddToBody(dic).IndirectReference);
            }
            if (!fonts.Contains(PdfName.ZADB)) {
                PdfDictionary dic = new PdfDictionary(PdfName.FONT);
                dic.Put(PdfName.BASEFONT, PdfName.ZAPFDINGBATS);
                dic.Put(PdfName.NAME, PdfName.ZADB);
                dic.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                fonts.Put(PdfName.ZADB, AddToBody(dic).IndirectReference);
            }
        }
        
        /**
        * Signals that the <CODE>Document</CODE> was closed and that no other
        * <CODE>Elements</CODE> will be added.
        * <P>
        * The pages-tree is built and written to the outputstream.
        * A Catalog is constructed, as well as an Info-object,
        * the referencetable is composed and everything is written
        * to the outputstream embedded in a Trailer.
        */
        
        public override void Close() {
            if (open) {
                pdf.Close();
                base.Close();
                // Users are responsible for closing PdfReader
                //if (ri != null) {
                //    try {
                //        ri.Reader.Close();
                //        ri.ReaderFile.Close();
                //    }
                //    catch (IOException) {
                //        // empty on purpose
                //    }
                //}
            }
        }

        public override void AddAnnotation(PdfAnnotation annot) {  }

        internal override PdfIndirectReference Add(PdfPage page, PdfContents contents) { return null; }

        public override void FreeReader(PdfReader reader) {
            if (mergeFields)
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("it.is.not.possible.to.free.reader.in.merge.fields.mode"));
            PdfArray array = reader.trailer.GetAsArray(PdfName.ID);
            if (array != null)
                originalFileID = array.GetAsString(0).GetBytes();
            indirectMap.Remove(reader);
// TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
            //if (currentPdfReaderInstance != null) {
            //    if (currentPdfReaderInstance.Reader == reader) {
            //        try {
            //            currentPdfReaderInstance.Reader.Close();
            //            currentPdfReaderInstance.ReaderFile.Close();
            //        }
            //        catch (IOException) {
            //            // empty on purpose
            //        }
                    currentPdfReaderInstance = null;
            //    }
            //}
            base.FreeReader(reader);
        }

        virtual protected PdfName GetOffStateName(PdfDictionary widget) {
            return PdfName.Off_;
        }

        protected static readonly HashSet2<PdfName> widgetKeys = new HashSet2<PdfName>();
        protected static readonly HashSet2<PdfName> fieldKeys = new HashSet2<PdfName>();
        static PdfCopy() {
            widgetKeys.Add(PdfName.SUBTYPE);
            widgetKeys.Add(PdfName.CONTENTS);
            widgetKeys.Add(PdfName.RECT);
            widgetKeys.Add(PdfName.NM);
            widgetKeys.Add(PdfName.M);
            widgetKeys.Add(PdfName.F);
            widgetKeys.Add(PdfName.BS);
            widgetKeys.Add(PdfName.BORDER);
            widgetKeys.Add(PdfName.AP);
            widgetKeys.Add(PdfName.AS);
            widgetKeys.Add(PdfName.C);
            widgetKeys.Add(PdfName.A);
            widgetKeys.Add(PdfName.STRUCTPARENT);
            widgetKeys.Add(PdfName.OC);
            widgetKeys.Add(PdfName.H);
            widgetKeys.Add(PdfName.MK);
            widgetKeys.Add(PdfName.DA);
            widgetKeys.Add(PdfName.Q);
            widgetKeys.Add(PdfName.P);
            widgetKeys.Add(PdfName.TYPE);
            widgetKeys.Add(annotId);
            fieldKeys.Add(PdfName.AA);
            fieldKeys.Add(PdfName.FT);
            fieldKeys.Add(PdfName.TU);
            fieldKeys.Add(PdfName.TM);
            fieldKeys.Add(PdfName.FF);
            fieldKeys.Add(PdfName.V);
            fieldKeys.Add(PdfName.DV);
            fieldKeys.Add(PdfName.DS);
            fieldKeys.Add(PdfName.RV);
            fieldKeys.Add(PdfName.OPT);
            fieldKeys.Add(PdfName.MAXLEN);
            fieldKeys.Add(PdfName.TI);
            fieldKeys.Add(PdfName.I);
            fieldKeys.Add(PdfName.LOCK);
            fieldKeys.Add(PdfName.SV);
        }

        internal static int? GetFlags(PdfDictionary field) {
            PdfName type = field.GetAsName(PdfName.FT);
            if (!PdfName.BTN.Equals(type))
                return null;
            PdfNumber flags = field.GetAsNumber(PdfName.FF);
            if (flags == null)
                return null;
            return flags.IntValue;
        }

        internal static bool IsCheckButton(PdfDictionary field) {
            int? flags = GetFlags(field);
            return flags == null ||
                   ((flags.Value & PdfFormField.FF_PUSHBUTTON) == 0 && (flags.Value & PdfFormField.FF_RADIO) == 0);
        }

        internal static bool IsRadioButton(PdfDictionary field) {
            int? flags = GetFlags(field);
            return flags != null && (flags.Value & PdfFormField.FF_PUSHBUTTON) == 0 && (flags.Value & PdfFormField.FF_RADIO) != 0;
        }

        internal static bool IsTextField(PdfDictionary field) {
            PdfName type = field.GetAsName(PdfName.FT);
            return PdfName.TX.Equals(type);
        }

        /**
        * Create a page stamp. New content and annotations, including new fields, are allowed.
        * The fields added cannot have parents in another pages. This method modifies the PdfReader instance.<p>
        * The general usage to stamp something in a page is:
        * <p>
        * <pre>
        * PdfImportedPage page = copy.GetImportedPage(reader, 1);
        * PdfCopy.PageStamp ps = copy.CreatePageStamp(page);
        * ps.AddAnnotation(PdfAnnotation.CreateText(copy, new Rectangle(50, 180, 70, 200), "Hello", "No Thanks", true, "Comment"));
        * PdfContentByte under = ps.GetUnderContent();
        * under.AddImage(img);
        * PdfContentByte over = ps.GetOverContent();
        * over.BeginText();
        * over.SetFontAndSize(bf, 18);
        * over.SetTextMatrix(30, 30);
        * over.ShowText("total page " + totalPage);
        * over.EndText();
        * ps.AlterContents();
        * copy.AddPage(page);
        * </pre>
        * @param iPage an imported page
        * @return the <CODE>PageStamp</CODE>
        */
        virtual public PageStamp CreatePageStamp(PdfImportedPage iPage) {
            int pageNum = iPage.PageNumber;
            PdfReader reader = iPage.PdfReaderInstance.Reader;
            if (IsTagged())
                throw new Exception(MessageLocalization.GetComposedMessage("creating.page.stamp.not.allowed.for.tagged.reader"));
            PdfDictionary pageN = reader.GetPageN(pageNum);
            return new PageStamp(reader, pageN, this);
        }

        public class PageStamp {
            
            PdfDictionary pageN;
            PdfCopy.StampContent under;
            PdfCopy.StampContent over;
            PageResources pageResources;
            PdfReader reader;
            PdfCopy cstp;
            
            internal PageStamp(PdfReader reader, PdfDictionary pageN, PdfCopy cstp) {
                this.pageN = pageN;
                this.reader = reader;
                this.cstp = cstp;
            }
            
            virtual public PdfContentByte GetUnderContent(){
                if (under == null) {
                    if (pageResources == null) {
                        pageResources = new PageResources();
                        PdfDictionary resources = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(resources, cstp.namePtr);
                    }
                    under = new PdfCopy.StampContent(cstp, pageResources);
                }
                return under;
            }
            
            virtual public PdfContentByte GetOverContent(){
                if (over == null) {
                    if (pageResources == null) {
                        pageResources = new PageResources();
                        PdfDictionary resources = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(resources, cstp.namePtr);
                    }
                    over = new PdfCopy.StampContent(cstp, pageResources);
                }
                return over;
            }

            virtual public void AlterContents() {
                if (over == null && under == null)
                    return;
                PdfArray ar = null;
                PdfObject content = PdfReader.GetPdfObject(pageN.Get(PdfName.CONTENTS), pageN);
                if (content == null) {
                    ar = new PdfArray();
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                else if (content.IsArray()) {
                    ar = (PdfArray)content;
                }
                else if (content.IsStream()) {
                    ar = new PdfArray();
                    ar.Add(pageN.Get(PdfName.CONTENTS));
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                else {
                    ar = new PdfArray();
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                ByteBuffer out_p = new ByteBuffer();
                if (under != null) {
                    out_p.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, out_p);
                    out_p.Append(under.InternalBuffer);
                    out_p.Append(PdfContents.RESTORESTATE);
                }
                if (over != null)
                    out_p.Append(PdfContents.SAVESTATE);
                PdfStream stream = new PdfStream(out_p.ToByteArray());
                stream.FlateCompress(cstp.CompressionLevel);
                PdfIndirectReference ref1 = cstp.AddToBody(stream).IndirectReference;
                ar.AddFirst(ref1);
                out_p.Reset();
                if (over != null) {
                    out_p.Append(' ');
                    out_p.Append(PdfContents.RESTORESTATE);
                    out_p.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, out_p);
                    out_p.Append(over.InternalBuffer);
                    out_p.Append(PdfContents.RESTORESTATE);
                    stream = new PdfStream(out_p.ToByteArray());
                    stream.FlateCompress(cstp.CompressionLevel);
                    ar.Add(cstp.AddToBody(stream).IndirectReference);
                }
                pageN.Put(PdfName.RESOURCES, pageResources.Resources);
            }

            void ApplyRotation(PdfDictionary pageN, ByteBuffer out_p) {
                if (!cstp.rotateContents)
                    return;
                Rectangle page = reader.GetPageSizeWithRotation(pageN);
                int rotation = page.Rotation;
                switch (rotation) {
                    case 90:
                        out_p.Append(PdfContents.ROTATE90);
                        out_p.Append(page.Top);
                        out_p.Append(' ').Append('0').Append(PdfContents.ROTATEFINAL);
                        break;
                    case 180:
                        out_p.Append(PdfContents.ROTATE180);
                        out_p.Append(page.Right);
                        out_p.Append(' ');
                        out_p.Append(page.Top);
                        out_p.Append(PdfContents.ROTATEFINAL);
                        break;
                    case 270:
                        out_p.Append(PdfContents.ROTATE270);
                        out_p.Append('0').Append(' ');
                        out_p.Append(page.Right);
                        out_p.Append(PdfContents.ROTATEFINAL);
                        break;
                }
            }

            private void AddDocumentField(PdfIndirectReference refi) {
                if (cstp.fieldArray == null)
                    cstp.fieldArray = new PdfArray();
                cstp.fieldArray.Add(refi);
            }

            private void ExpandFields(PdfFormField field, List<PdfAnnotation> allAnnots) {
                allAnnots.Add(field);
                List<PdfFormField> kids = field.Kids;
                if (kids != null) {
                    foreach (PdfFormField f in kids)
                        ExpandFields(f, allAnnots);
                }
            }

            virtual public void AddAnnotation(PdfAnnotation annot) {
                List<PdfAnnotation> allAnnots = new List<PdfAnnotation>();
                if (annot.IsForm()) {
                    PdfFormField field = (PdfFormField)annot;
                    if (field.Parent != null)
                        return;
                    ExpandFields(field, allAnnots);
                    if (cstp.fieldTemplates == null)
                        cstp.fieldTemplates = new HashSet2<PdfTemplate>();
                }
                else
                    allAnnots.Add(annot);
                for (int k = 0; k < allAnnots.Count; ++k) {
                    annot = allAnnots[k];
                    if (annot.IsForm()) {
                        if (!annot.IsUsed()) {
                            Dictionary<PdfTemplate,object> templates = annot.Templates;
                            if (templates != null) {
                                foreach (PdfTemplate tpl in templates.Keys) {
                                    cstp.fieldTemplates.Add(tpl);
                                }
                            }
                        }
                        PdfFormField field = (PdfFormField)annot;
                        if (field.Parent == null)
                            AddDocumentField(field.IndirectReference);
                    }
                    if (annot.IsAnnotation()) {
                        PdfObject pdfobj = PdfReader.GetPdfObject(pageN.Get(PdfName.ANNOTS), pageN);
                        PdfArray annots = null;
                        if (pdfobj == null || !pdfobj.IsArray()) {
                            annots = new PdfArray();
                            pageN.Put(PdfName.ANNOTS, annots);
                        }
                        else 
                            annots = (PdfArray)pdfobj;
                        annots.Add(annot.IndirectReference);
                        if (!annot.IsUsed()) {
                            PdfRectangle rect = (PdfRectangle)annot.Get(PdfName.RECT);
                            if (rect != null && (rect.Left != 0 || rect.Right != 0 || rect.Top != 0 || rect.Bottom != 0)) {
                                int rotation = reader.GetPageRotation(pageN);
                                Rectangle pageSize = reader.GetPageSizeWithRotation(pageN);
                                switch (rotation) {
                                    case 90:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            pageSize.Top - rect.Bottom,
                                            rect.Left,
                                            pageSize.Top - rect.Top,
                                            rect.Right));
                                        break;
                                    case 180:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            pageSize.Right - rect.Left,
                                            pageSize.Top - rect.Bottom,
                                            pageSize.Right - rect.Right,
                                            pageSize.Top - rect.Top));
                                        break;
                                    case 270:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            rect.Bottom,
                                            pageSize.Right - rect.Left,
                                            rect.Top,
                                            pageSize.Right - rect.Right));
                                        break;
                                }
                            }
                        }
                    }
                    if (!annot.IsUsed()) {
                        annot.SetUsed();
                        cstp.AddToBody(annot, annot.IndirectReference);
                    }
                }
            }
        }

        public class StampContent : PdfContentByte {
            PageResources pageResources;
            
            /** Creates a new instance of StampContent */
            internal StampContent(PdfWriter writer, PageResources pageResources) : base(writer) {
                this.pageResources = pageResources;
            }

            /**
            * Gets a duplicate of this <CODE>PdfContentByte</CODE>. All
            * the members are copied by reference but the buffer stays different.
            *
            * @return a copy of this <CODE>PdfContentByte</CODE>
            */
            public override PdfContentByte Duplicate {
                get {
                    return new PdfCopy.StampContent(writer, pageResources);
                }
            }

            internal override PageResources PageResources {
                get {
                    return pageResources;
                }
            }
        }
    }
}
