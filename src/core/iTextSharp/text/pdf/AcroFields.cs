using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.util;
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.security;
using iTextSharp.text.xml;
using iTextSharp.text.io;

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
    /** Query and change fields in existing documents either by method
    * calls or by FDF merging.
    * @author Paulo Soares
    */
    public class AcroFields {

        internal PdfReader reader;
        internal PdfWriter writer;
        internal IDictionary<String, Item> fields;
        private int topFirst;
        private Dictionary<String, int[]> sigNames;
        private bool append;
        public const int DA_FONT = 0;
        public const int DA_SIZE = 1;
        public const int DA_COLOR = 2;
        private Dictionary<int, BaseFont> extensionFonts = new Dictionary<int,BaseFont>();
        private XfaForm xfa;
        /**
        * A field type invalid or not found.
        */    
        public const int FIELD_TYPE_NONE = 0;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_PUSHBUTTON = 1;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_CHECKBOX = 2;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_RADIOBUTTON = 3;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_TEXT = 4;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_LIST = 5;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_COMBO = 6;
        /**
        * A field type.
        */    
        public const int FIELD_TYPE_SIGNATURE = 7;
        
        private bool lastWasString;
        
        /** Holds value of property generateAppearances. */
        private bool generateAppearances = true;
        
        private Dictionary<String, BaseFont> localFonts = new Dictionary<String, BaseFont>();
        
        private float extraMarginLeft;
        private float extraMarginTop;
        private List<BaseFont> substitutionFonts;

        internal AcroFields(PdfReader reader, PdfWriter writer) {
            this.reader = reader;
            this.writer = writer;
            xfa = new XfaForm(reader);
            if (writer is PdfStamperImp) {
                append = ((PdfStamperImp)writer).append;
            }
            Fill();
        }

        internal void Fill() {
            fields = new LinkedDictionary<string, Item>();
            PdfDictionary top = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (top == null)
                return;
            PdfBoolean needappearances = top.GetAsBoolean(PdfName.NEEDAPPEARANCES);
            if(needappearances == null || !needappearances.BooleanValue)
                GenerateAppearances = true;
            else
                GenerateAppearances = false;
            PdfArray arrfds = (PdfArray)PdfReader.GetPdfObjectRelease(top.Get(PdfName.FIELDS));
            if (arrfds == null || arrfds.Size == 0)
                return;
            for (int k = 1; k <= reader.NumberOfPages; ++k) {
                PdfDictionary page = reader.GetPageNRelease(k);
                PdfArray annots = (PdfArray)PdfReader.GetPdfObjectRelease(page.Get(PdfName.ANNOTS), page);
                if (annots == null)
                    continue;
                for (int j = 0; j < annots.Size; ++j) {
                    PdfDictionary annot = annots.GetAsDict(j);
                    if (annot == null) {
                        PdfReader.ReleaseLastXrefPartial(annots.GetAsIndirectObject(j));
                        continue;
                    }
                    if (!PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE))) {
                        PdfReader.ReleaseLastXrefPartial(annots.GetAsIndirectObject(j));
                        continue;
                    }
                    PdfDictionary widget = annot;
                    PdfDictionary dic = new PdfDictionary();
                    dic.Merge(annot);
                    String name = "";
                    PdfDictionary value = null;
                    PdfObject lastV = null;
                    while (annot != null) {
                        dic.MergeDifferent(annot);
                        PdfString t = annot.GetAsString(PdfName.T);
                        if (t != null)
                            name = t.ToUnicodeString() + "." + name;
                        if (lastV == null && annot.Get(PdfName.V) != null)
                            lastV = PdfReader.GetPdfObjectRelease(annot.Get(PdfName.V));
                        if (value == null &&  t != null) {
                            value = annot;
                            if (annot.Get(PdfName.V) == null && lastV  != null)
                                value.Put(PdfName.V, lastV);
                        }
                        annot = annot.GetAsDict(PdfName.PARENT);
                    }
                    if (name.Length > 0)
                        name = name.Substring(0, name.Length - 1);
                    Item item;
                    if (!fields.TryGetValue(name, out item)) {
                        item = new Item();
                        fields[name] = item;
                    }
                    if (value == null)
                        item.AddValue(widget);
                    else
                        item.AddValue(value);
                    item.AddWidget(widget);
                    item.AddWidgetRef(annots.GetAsIndirectObject(j)); // must be a reference
                    if (top != null)
                        dic.MergeDifferent(top);
                    item.AddMerged(dic);
                    item.AddPage(k);
                    item.AddTabOrder(j);
                }
            }
            // some tools produce invisible signatures without an entry in the page annotation array
            // look for a single level annotation
            PdfNumber sigFlags = top.GetAsNumber(PdfName.SIGFLAGS);
            if (sigFlags == null || (sigFlags.IntValue & 1) != 1)
                return;
            for (int j = 0; j < arrfds.Size; ++j) {
                PdfDictionary annot = arrfds.GetAsDict(j);
                if (annot == null) {
                    PdfReader.ReleaseLastXrefPartial(arrfds.GetAsIndirectObject(j));
                    continue;
                }
                if (!PdfName.WIDGET.Equals(annot.GetAsName(PdfName.SUBTYPE))) {
                    PdfReader.ReleaseLastXrefPartial(arrfds.GetAsIndirectObject(j));
                    continue;
                }
                PdfArray kids = (PdfArray)PdfReader.GetPdfObjectRelease(annot.Get(PdfName.KIDS));
                if (kids != null)
                    continue;
                PdfDictionary dic = new PdfDictionary();
                dic.Merge(annot);
                PdfString t = annot.GetAsString(PdfName.T);
                if (t == null)
                    continue;
                String name = t.ToUnicodeString();
                if (fields.ContainsKey(name))
                    continue;
                Item item = new Item();
                fields[name] = item;
                item.AddValue(dic);
                item.AddWidget(dic);
                item.AddWidgetRef(arrfds.GetAsIndirectObject(j)); // must be a reference
                item.AddMerged(dic);
                item.AddPage(-1);
                item.AddTabOrder(-1);
            }
        }

        /** Gets the list of appearance names. Use it to get the names allowed
         * with radio and checkbox fields. If the /Opt key exists the values will
         * also be included. The name 'Off' may also be valid
         * even if not returned in the list.
         *  
         * For Comboboxes it will return an array of display values. To extract the
         * export values of a Combobox, please refer to {@link AcroFields#getListOptionExport(String)}
         *
         * @param fieldName the fully qualified field name
         * @return the list of names or <CODE>null</CODE> if the field does not exist
         */
        virtual public String[] GetAppearanceStates(String fieldName) {
            if (!fields.ContainsKey(fieldName))
                return null;
            Item fd = fields[fieldName];
            HashSet2<string> names = new HashSet2<string>();
            PdfDictionary vals = fd.GetValue(0);
            PdfString stringOpt = vals.GetAsString( PdfName.OPT );

            // should not happen according to specs
            if (stringOpt != null) {
                names.Add(stringOpt.ToUnicodeString());
            }
            else {
                PdfArray arrayOpt = vals.GetAsArray(PdfName.OPT);
                if (arrayOpt != null) {
                    for (int k = 0; k < arrayOpt.Size; ++k) {
                        PdfObject pdfObject = arrayOpt.GetDirectObject(k);
                        PdfString valStr = null;

                        switch (pdfObject.Type)
                        {
                            case PdfObject.ARRAY:
                                valStr = ((PdfArray)pdfObject).GetAsString(1);
                                break;
                            case PdfObject.STRING:
                                valStr = (PdfString)pdfObject;
                                break;
                        }
                        if (valStr != null)
                            names.Add(valStr.ToUnicodeString());
                    }
                }
            }
            for (int k = 0; k < fd.Size; ++k) {
                PdfDictionary dic = fd.GetWidget( k );
                dic = dic.GetAsDict(PdfName.AP);
                if (dic == null)
                    continue;
                dic = dic.GetAsDict(PdfName.N);
                if (dic == null)
                    continue;
            foreach (PdfName pname in dic.Keys) {
                    String name = PdfName.DecodeName(pname.ToString());
                    names.Add(name);
                }
            }
            string[] outs = new string[names.Count];
            names.CopyTo(outs, 0);
            return outs;
        }
        
        private String[] GetListOption(String fieldName, int idx) {
            Item fd = GetFieldItem(fieldName);
            if (fd == null)
                return null;
            PdfArray ar = fd.GetMerged(0).GetAsArray(PdfName.OPT);
            if (ar == null)
                return null;
            String[] ret = new String[ar.Size];
            for (int k = 0; k < ar.Size; ++k) {
                PdfObject obj = ar.GetDirectObject( k );
                try {
                    if (obj.IsArray()) {
                        obj = ((PdfArray)obj).GetDirectObject(idx);
                    }
                    if (obj.IsString())
                        ret[k] = ((PdfString)obj).ToUnicodeString();
                    else
                        ret[k] = obj.ToString();
                }
                catch {
                    ret[k] = "";
                }
            }
            return ret;
        }
    
        /**
        * Gets the list of export option values from fields of type list or combo.
        * If the field doesn't exist or the field type is not list or combo it will return
        * <CODE>null</CODE>.
        * @param fieldName the field name
        * @return the list of export option values from fields of type list or combo
        */    
        virtual public String[] GetListOptionExport(String fieldName) {
            return GetListOption(fieldName, 0);
        }
        
        /**
        * Gets the list of display option values from fields of type list or combo.
        * If the field doesn't exist or the field type is not list or combo it will return
        * <CODE>null</CODE>.
        * @param fieldName the field name
        * @return the list of export option values from fields of type list or combo
        */    
        virtual public String[] GetListOptionDisplay(String fieldName) {
            return GetListOption(fieldName, 1);
        }
        
        /**
        * Sets the option list for fields of type list or combo. One of <CODE>exportValues</CODE>
        * or <CODE>displayValues</CODE> may be <CODE>null</CODE> but not both. This method will only
        * set the list but will not set the value or appearance. For that, calling <CODE>setField()</CODE>
        * is required.
        * <p>
        * An example:
        * <p>
        * <PRE>
        * PdfReader pdf = new PdfReader("input.pdf");
        * PdfStamper stp = new PdfStamper(pdf, new FileOutputStream("output.pdf"));
        * AcroFields af = stp.GetAcroFields();
        * af.SetListOption("ComboBox", new String[]{"a", "b", "c"}, new String[]{"first", "second", "third"});
        * af.SetField("ComboBox", "b");
        * stp.Close();
        * </PRE>
        * @param fieldName the field name
        * @param exportValues the export values
        * @param displayValues the display values
        * @return <CODE>true</CODE> if the operation succeeded, <CODE>false</CODE> otherwise
        */    
        virtual public bool SetListOption(String fieldName, String[] exportValues, String[] displayValues) {
            if (exportValues == null && displayValues == null)
                return false;
            if (exportValues != null && displayValues != null && exportValues.Length != displayValues.Length)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.export.and.the.display.array.must.have.the.same.size"));
            int ftype = GetFieldType(fieldName);
            if (ftype != FIELD_TYPE_COMBO && ftype != FIELD_TYPE_LIST)
                return false;
            Item fd = fields[fieldName];
            String[] sing = null;
            if (exportValues == null && displayValues != null)
                sing = displayValues;
            else if (exportValues != null && displayValues == null)
                sing = exportValues;
            PdfArray opt = new PdfArray();
            if (sing != null) {
                for (int k = 0; k < sing.Length; ++k)
                    opt.Add(new PdfString(sing[k], PdfObject.TEXT_UNICODE));
            }
            else {
                for (int k = 0; k < exportValues.Length; ++k) {
                    PdfArray a = new PdfArray();
                    a.Add(new PdfString(exportValues[k], PdfObject.TEXT_UNICODE));
                    a.Add(new PdfString(displayValues[k], PdfObject.TEXT_UNICODE));
                    opt.Add(a);
                }
            }
            fd.WriteToAll( PdfName.OPT, opt, Item.WRITE_VALUE | Item.WRITE_MERGED );
            return true;
        }
        
        /**
        * Gets the field type. The type can be one of: <CODE>FIELD_TYPE_PUSHBUTTON</CODE>,
        * <CODE>FIELD_TYPE_CHECKBOX</CODE>, <CODE>FIELD_TYPE_RADIOBUTTON</CODE>,
        * <CODE>FIELD_TYPE_TEXT</CODE>, <CODE>FIELD_TYPE_LIST</CODE>,
        * <CODE>FIELD_TYPE_COMBO</CODE> or <CODE>FIELD_TYPE_SIGNATURE</CODE>.
        * <p>
        * If the field does not exist or is invalid it returns
        * <CODE>FIELD_TYPE_NONE</CODE>.
        * @param fieldName the field name
        * @return the field type
        */    
        virtual public int GetFieldType(String fieldName) {
            Item fd = GetFieldItem(fieldName);
            if (fd == null)
                return FIELD_TYPE_NONE;
            PdfDictionary merged = fd.GetMerged( 0 );
            PdfName type = merged.GetAsName(PdfName.FT);
            if (type == null)
                return FIELD_TYPE_NONE;
            int ff = 0;
            PdfNumber ffo = merged.GetAsNumber(PdfName.FF);
            if (ffo != null) {
                ff = ffo.IntValue;
            }
            if (PdfName.BTN.Equals(type)) {
                if ((ff & PdfFormField.FF_PUSHBUTTON) != 0)
                    return FIELD_TYPE_PUSHBUTTON;
                if ((ff & PdfFormField.FF_RADIO) != 0)
                    return FIELD_TYPE_RADIOBUTTON;
                else
                    return FIELD_TYPE_CHECKBOX;
            }
            else if (PdfName.TX.Equals(type)) {
                return FIELD_TYPE_TEXT;
            }
            else if (PdfName.CH.Equals(type)) {
                if ((ff & PdfFormField.FF_COMBO) != 0)
                    return FIELD_TYPE_COMBO;
                else
                    return FIELD_TYPE_LIST;
            }
            else if (PdfName.SIG.Equals(type)) {
                return FIELD_TYPE_SIGNATURE;
            }
            return FIELD_TYPE_NONE;
        }
        
        /**
        * Export the fields as a FDF.
        * @param writer the FDF writer
        */    
        virtual public void ExportAsFdf(FdfWriter writer) {
            foreach (KeyValuePair<string,Item> entry in fields) {
                Item item = entry.Value;
                string name = entry.Key;
                PdfObject v = item.GetMerged(0).Get(PdfName.V);
                if (v == null)
                    continue;
                string value = GetField(name);
                if (lastWasString)
                    writer.SetFieldAsString(name, value);
                else
                    writer.SetFieldAsName(name, value);
            }
        }
        
        /**
        * Renames a field. Only the last part of the name can be renamed. For example,
        * if the original field is "ab.cd.ef" only the "ef" part can be renamed.
        * @param oldName the old field name
        * @param newName the new field name
        * @return <CODE>true</CODE> if the renaming was successful, <CODE>false</CODE>
        * otherwise
        */    
        virtual public bool RenameField(String oldName, String newName) {
            int idx1 = oldName.LastIndexOf('.') + 1;
            int idx2 = newName.LastIndexOf('.') + 1;
            if (idx1 != idx2)
                return false;
            if (!oldName.Substring(0, idx1).Equals(newName.Substring(0, idx2)))
                return false;
            if (fields.ContainsKey(newName))
                return false;
            if (!fields.ContainsKey(oldName))
                return false;
            Item item = fields[oldName];
            newName = newName.Substring(idx2);
            PdfString ss = new PdfString(newName, PdfObject.TEXT_UNICODE);
            item.WriteToAll( PdfName.T, ss, Item.WRITE_VALUE | Item.WRITE_MERGED);
            item.MarkUsed( this, Item.WRITE_VALUE );
            fields.Remove(oldName);
            fields[newName] = item;
            return true;
        }
        
        public static Object[] SplitDAelements(String da) {
            PRTokeniser tk = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(PdfEncodings.ConvertToBytes(da, null))));
            List<String> stack = new List<string>();
            Object[] ret = new Object[3];
            while (tk.NextToken()) {
                if (tk.TokenType == PRTokeniser.TokType.COMMENT)
                    continue;
                if (tk.TokenType == PRTokeniser.TokType.OTHER) {
                    String oper = tk.StringValue;
                    if (oper.Equals("Tf")) {
                        if (stack.Count >= 2) {
                            ret[DA_FONT] = stack[stack.Count - 2];
                            ret[DA_SIZE] = float.Parse(stack[stack.Count - 1], System.Globalization.NumberFormatInfo.InvariantInfo);
                        }
                    }
                    else if (oper.Equals("g")) {
                        if (stack.Count >= 1) {
                            float gray = float.Parse(stack[stack.Count - 1], System.Globalization.NumberFormatInfo.InvariantInfo);
                            if (gray != 0)
                                ret[DA_COLOR] = new GrayColor(gray);
                        }
                    }
                    else if (oper.Equals("rg")) {
                        if (stack.Count >= 3) {
                            float red = float.Parse(stack[stack.Count - 3], System.Globalization.NumberFormatInfo.InvariantInfo);
                            float green = float.Parse(stack[stack.Count - 2], System.Globalization.NumberFormatInfo.InvariantInfo);
                            float blue = float.Parse(stack[stack.Count - 1], System.Globalization.NumberFormatInfo.InvariantInfo);
                            ret[DA_COLOR] = new BaseColor(red, green, blue);
                        }
                    }
                    else if (oper.Equals("k")) {
                        if (stack.Count >= 4) {
                            float cyan = float.Parse(stack[stack.Count - 4], System.Globalization.NumberFormatInfo.InvariantInfo);
                            float magenta = float.Parse(stack[stack.Count - 3], System.Globalization.NumberFormatInfo.InvariantInfo);
                            float yellow = float.Parse(stack[stack.Count - 2], System.Globalization.NumberFormatInfo.InvariantInfo);
                            float black = float.Parse(stack[stack.Count - 1], System.Globalization.NumberFormatInfo.InvariantInfo);
                            ret[DA_COLOR] = new CMYKColor(cyan, magenta, yellow, black);
                        }
                    }
                    stack.Clear();
                }
                else
                    stack.Add(tk.StringValue);
            }
            return ret;
        }
        
        virtual public void DecodeGenericDictionary(PdfDictionary merged, BaseField tx) {
            int flags = 0;
            // the text size and color
            PdfString da = merged.GetAsString(PdfName.DA);
            if (da != null) {
                bool fontFallBack = false;
                Object[] dab = SplitDAelements(da.ToUnicodeString());
                if (dab[DA_SIZE] != null)
                    tx.FontSize = (float)dab[DA_SIZE];
                if (dab[DA_COLOR] != null)
                    tx.TextColor = (BaseColor)dab[DA_COLOR];
                if (dab[DA_FONT] != null) {
                    PdfDictionary dr = merged.GetAsDict(PdfName.DR);
                    if (dr != null) {
                        PdfDictionary font = dr.GetAsDict(PdfName.FONT);
                        if (font != null) {
                            PdfObject po = font.Get(new PdfName((String) dab[DA_FONT]));
                            if (po != null && po.Type == PdfObject.INDIRECT) {
                                PRIndirectReference por = (PRIndirectReference) po;
                                BaseFont bp = new DocumentFont((PRIndirectReference) po, dr.GetAsDict(PdfName.ENCODING));
                                tx.Font = bp;
                                int porkey = por.Number;
                                BaseFont porf;
                                extensionFonts.TryGetValue(porkey, out porf);
                                if (porf == null) {
                                    if (!extensionFonts.ContainsKey(porkey)) {
                                        PdfDictionary fo = (PdfDictionary) PdfReader.GetPdfObject(po);
                                        PdfDictionary fd = fo.GetAsDict(PdfName.FONTDESCRIPTOR);
                                        if (fd != null) {
                                            PRStream prs = (PRStream) PdfReader.GetPdfObject(fd.Get(PdfName.FONTFILE2));
                                            if (prs == null)
                                                prs = (PRStream) PdfReader.GetPdfObject(fd.Get(PdfName.FONTFILE3));
                                            if (prs == null) {
                                                extensionFonts[porkey] = null;
                                            }
                                            else {
                                                try {
                                                    porf = BaseFont.CreateFont("font.ttf", BaseFont.IDENTITY_H, true,
                                                        false, PdfReader.GetStreamBytes(prs), null);
                                                }
                                                catch {
                                                }
                                                extensionFonts[porkey] = porf;
                                            }
                                        }
                                    }
                                }
                                if (tx is TextField)
                                    ((TextField) tx).ExtensionFont = porf;
                            }
                            else {
                                fontFallBack = true;
                            }
                        }
                        else {
                            fontFallBack = true;
                        }
                    }
                    else {
                        fontFallBack = true;
                    }
                }
                if (fontFallBack) {
                        BaseFont bf;
                        if (!localFonts.TryGetValue((string)dab[DA_FONT], out bf))
                        {
                            String[] fn;
                            stdFieldFontNames.TryGetValue((string)dab[DA_FONT], out fn);
                            if (fn != null)
                            {
                                try
                                {
                                    String enc = "winansi";
                                    if (fn.Length > 1)
                                        enc = fn[1];
                                    bf = BaseFont.CreateFont(fn[0], enc, false);
                                    tx.Font = bf;
                                }
                                catch
                                {
                                    // empty
                                }
                            }
                        }
                        else
                            tx.Font = bf;
                }
            }
            //rotation, border and backgound color
            PdfDictionary mk = merged.GetAsDict(PdfName.MK);
            if (mk != null) {
                PdfArray ar = mk.GetAsArray(PdfName.BC);
                BaseColor border = GetMKColor(ar);
                tx.BorderColor = border;
                if (border != null)
                    tx.BorderWidth = 1;
                ar = mk.GetAsArray(PdfName.BG);
                tx.BackgroundColor = GetMKColor(ar);
                PdfNumber rotation = mk.GetAsNumber(PdfName.R);
                if (rotation != null)
                    tx.Rotation = rotation.IntValue;
            }
            //flags
            PdfNumber nfl = merged.GetAsNumber(PdfName.F);
            flags = 0;
            tx.Visibility = BaseField.VISIBLE_BUT_DOES_NOT_PRINT;
            if (nfl != null) {
                flags = nfl.IntValue;
                if ((flags & PdfFormField.FLAGS_PRINT) != 0 && (flags & PdfFormField.FLAGS_HIDDEN) != 0)
                    tx.Visibility = BaseField.HIDDEN;
                else if ((flags & PdfFormField.FLAGS_PRINT) != 0 && (flags & PdfFormField.FLAGS_NOVIEW) != 0)
                    tx.Visibility = BaseField.HIDDEN_BUT_PRINTABLE;
                else if ((flags & PdfFormField.FLAGS_PRINT) != 0)
                    tx.Visibility = BaseField.VISIBLE;
            }
            //multiline
            nfl = merged.GetAsNumber(PdfName.FF);
            flags = 0;
            if (nfl != null)
                flags = nfl.IntValue;
            tx.Options = flags;
            if ((flags & PdfFormField.FF_COMB) != 0) {
                PdfNumber maxLen = merged.GetAsNumber(PdfName.MAXLEN);
                int len = 0;
                if (maxLen != null)
                    len = maxLen.IntValue;
                tx.MaxCharacterLength = len;
            }
            //alignment
            nfl = merged.GetAsNumber(PdfName.Q);
            if (nfl != null) {
                if (nfl.IntValue == PdfFormField.Q_CENTER)
                    tx.Alignment = Element.ALIGN_CENTER;
                else if (nfl.IntValue == PdfFormField.Q_RIGHT)
                    tx.Alignment = Element.ALIGN_RIGHT;
            }
            //border styles
            PdfDictionary bs = merged.GetAsDict(PdfName.BS);
            if (bs != null) {
                PdfNumber w = bs.GetAsNumber(PdfName.W);
                if (w != null)
                    tx.BorderWidth = w.FloatValue;
                PdfName s = bs.GetAsName(PdfName.S);
                if (PdfName.D.Equals(s))
                    tx.BorderStyle = PdfBorderDictionary.STYLE_DASHED;
                else if (PdfName.B.Equals(s))
                    tx.BorderStyle = PdfBorderDictionary.STYLE_BEVELED;
                else if (PdfName.I.Equals(s))
                    tx.BorderStyle = PdfBorderDictionary.STYLE_INSET;
                else if (PdfName.U.Equals(s))
                    tx.BorderStyle = PdfBorderDictionary.STYLE_UNDERLINE;
            }
            else {
                PdfArray bd = merged.GetAsArray(PdfName.BORDER);
                if (bd != null) {
                    if (bd.Size >= 3)
                        tx.BorderWidth = bd.GetAsNumber(2).FloatValue;
                    if (bd.Size >= 4)
                        tx.BorderStyle = PdfBorderDictionary.STYLE_DASHED;
                }
            }
        }

        internal PdfAppearance GetAppearance(PdfDictionary merged, String[] values, String fieldName) {
            PdfName fieldType = merged.GetAsName(PdfName.FT);

            if (PdfName.BTN.Equals(fieldType)) {
                PdfNumber fieldFlags = merged.GetAsNumber(PdfName.FF);
                bool isRadio = fieldFlags != null && (fieldFlags.IntValue & PdfFormField.FF_RADIO) != 0;
                RadioCheckField field = new RadioCheckField(writer, null, null, null);
                DecodeGenericDictionary(merged, field);
                //rect
                PdfArray rect = merged.GetAsArray(PdfName.RECT);
                Rectangle box = PdfReader.GetNormalizedRectangle(rect);
                if (field.Rotation == 90 || field.Rotation == 270)
                    box = box.Rotate();
                field.Box = box;
                if (!isRadio) {
                    field.CheckType = RadioCheckField.TYPE_CROSS;    
                }
                return field.GetAppearance(isRadio, !(merged.GetAsName(PdfName.AS).Equals(PdfName.Off_)));
            }

            topFirst = 0;
            String text = (values.Length > 0) ? values[0] : null;
            TextField tx = null;
            if (fieldCache == null || !fieldCache.ContainsKey(fieldName)) {
                tx = new TextField(writer, null, null);
                tx.SetExtraMargin(extraMarginLeft, extraMarginTop);
                tx.BorderWidth = 0;
                tx.SubstitutionFonts = substitutionFonts;
                DecodeGenericDictionary(merged, tx);
                //rect
                PdfArray rect = merged.GetAsArray(PdfName.RECT);
                Rectangle box = PdfReader.GetNormalizedRectangle(rect);
                if (tx.Rotation == 90 || tx.Rotation == 270)
                    box = box.Rotate();
                tx.Box = box;
                if (fieldCache != null)
                    fieldCache[fieldName] = tx;
            }
            else {
                tx = fieldCache[fieldName];
                tx.Writer = writer;
            }

            if (PdfName.TX.Equals(fieldType)) {
                if (values.Length > 0 && values[0] != null) {
                    tx.Text = values[0];
                }
                return tx.GetAppearance();
            }
            if (!PdfName.CH.Equals(fieldType))
                throw new DocumentException(MessageLocalization.GetComposedMessage("an.appearance.was.requested.without.a.variable.text.field"));
            PdfArray opt = merged.GetAsArray(PdfName.OPT);
            int flags = 0;
            PdfNumber nfl = merged.GetAsNumber(PdfName.FF);
            if (nfl != null)
                flags = nfl.IntValue;
            if ((flags & PdfFormField.FF_COMBO) != 0 && opt == null) {
                tx.Text = text;
                return tx.GetAppearance();
            }
            if (opt != null) {
                String[] choices = new String[opt.Size];
                String[] choicesExp = new String[opt.Size];
                for (int k = 0; k < opt.Size; ++k) {
                    PdfObject obj = opt.GetPdfObject(k);
                    if (obj.IsString()) {
                            choices[k] = choicesExp[k] = ((PdfString)obj).ToUnicodeString();
                    }
                    else {
                        PdfArray a = (PdfArray) obj;
                        choicesExp[k] = a.GetAsString(0).ToUnicodeString();
                        choices[k] = a.GetAsString(1).ToUnicodeString();
                    }
                }
                if ((flags & PdfFormField.FF_COMBO) != 0) {
                    for (int k = 0; k < choices.Length; ++k) {
                        if (text.Equals(choicesExp[k])) {
                            text = choices[k];
                            break;
                        }
                    }
                    tx.Text = text;
                    return tx.GetAppearance();
                }
                List<int> indexes = new List<int>();
                for (int k = 0; k < choicesExp.Length; ++k) {
                    for (int j = 0; j < values.Length; ++j) {
                        String val = values[j];
                        if (val != null && val.Equals(choicesExp[k])) {
                            indexes.Add(k);
                            break;
                        }
                    }
                }
                tx.Choices = choices;
                tx.ChoiceExports = choicesExp;
                tx.ChoiceSelections = indexes;
            }
            PdfAppearance app = tx.GetListAppearance();
            topFirst = tx.TopFirst;
            return app;
        }
        
        internal PdfAppearance GetAppearance(PdfDictionary merged, String text, String fieldName) {
            String[] valueArr = new String[]{text};
            return GetAppearance( merged, valueArr, fieldName );
        }

        internal BaseColor GetMKColor(PdfArray ar) {
            if (ar == null)
                return null;
            switch (ar.Size) {
                case 1:
                    return new GrayColor(ar.GetAsNumber(0).FloatValue);
                case 3:
                    return new BaseColor(ExtendedColor.Normalize(ar.GetAsNumber(0).FloatValue), ExtendedColor.Normalize(ar.GetAsNumber(1).FloatValue), ExtendedColor.Normalize(ar.GetAsNumber(2).FloatValue));
                case 4:
                    return new CMYKColor(ar.GetAsNumber(0).FloatValue, ar.GetAsNumber(1).FloatValue, ar.GetAsNumber(2).FloatValue, ar.GetAsNumber(3).FloatValue);
                default:
                    return null;
            }
        }
        
        /**
         * Retrieve the rich value for the given field
         * @param name
         * @return The rich value if present, or null.
         * @since 5.0.6
         */
        virtual public String GetFieldRichValue(String name) {
            if (xfa.XfaPresent) {
                return null;
            }

            Item item;
            fields.TryGetValue(name, out item);
            if (item == null) { 
                return null;
            }

            PdfDictionary merged = item.GetMerged(0);
            PdfString rich = merged.GetAsString(PdfName.RV);
            
            String markup = null;
            if (rich != null) {
                markup = rich.ToString();
            }

            return markup;
        }

        /** Gets the field value.
        * @param name the fully qualified field name
        * @return the field value
        */    
        virtual public String GetField(String name) {
            if (xfa.XfaPresent) {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                    return null;
                name = XfaForm.Xml2Som.GetShortName(name);
                return XfaForm.GetNodeText(xfa.FindDatasetsNode(name));
            }
            if (!fields.ContainsKey(name))
                return null;
            Item item = fields[name];
            lastWasString = false;
            PdfDictionary mergedDict = item.GetMerged( 0 );

            // Jose A. Rodriguez posted a fix to the mailing list (May 11, 2009)
            // explaining that the value can also be a stream value
            // the fix was made against an old iText version. Bruno adapted it.
            PdfObject v = PdfReader.GetPdfObject(mergedDict.Get(PdfName.V));
            if (v == null)
                return "";
            if (v is PRStream) {
                byte[] valBytes = PdfReader.GetStreamBytes((PRStream)v);
                return PdfEncodings.ConvertToString(valBytes, BaseFont.WINANSI);
            }
            
            PdfName type = mergedDict.GetAsName(PdfName.FT);
            if (PdfName.BTN.Equals(type)) {
                PdfNumber ff = mergedDict.GetAsNumber(PdfName.FF);
            int flags = 0;
                if (ff != null)
                    flags = ff.IntValue;
                if ((flags & PdfFormField.FF_PUSHBUTTON) != 0)
                    return "";
                String value = "";
                if (v is PdfName)
                    value = PdfName.DecodeName(v.ToString());
                else if (v is PdfString)
                    value = ((PdfString)v).ToUnicodeString();
                PdfArray opts = item.GetValue(0).GetAsArray(PdfName.OPT);
                if (opts != null) {
                    int idx = 0;
                    try {
                        idx = int.Parse(value);
                        PdfString ps = opts.GetAsString(idx);
                        value = ps.ToUnicodeString();
                        lastWasString = true;
                    }
                    catch {
                    }
                }
                return value;
            }
            if (v is PdfString) {
                lastWasString = true;
                return ((PdfString)v).ToUnicodeString();
            } else if (v is PdfName) {
                return PdfName.DecodeName(v.ToString());
            } else
                return "";
        }

        /**
        * Gets the field values of a Choice field.
        * @param name the fully qualified field name
        * @return the field value
        * @since 2.1.3
        */    
        virtual public String[] GetListSelection(String name) {
            String[] ret;
            String s = GetField(name);
            if (s == null) {
                ret = new String[]{};
            }
            else {
                ret = new String[]{ s };
            }
            if (!fields.ContainsKey(name))
                return null;
            Item item = fields[name];
            PdfArray values = item.GetMerged(0).GetAsArray(PdfName.I);
            if (values == null)
                return ret;
            ret = new String[values.Size];
            String[] options = GetListOptionExport(name);
            int idx = 0;
            foreach (PdfNumber n in values.ArrayList) {
                ret[idx++] = options[n.IntValue];
            }
            return ret;
        }

        /**
        * Sets a field property. Valid property names are:
        * <p>
        * <ul>
        * <li>textfont - sets the text font. The value for this entry is a <CODE>BaseFont</CODE>.<br>
        * <li>textcolor - sets the text color. The value for this entry is a <CODE>java.awt.Color</CODE>.<br>
        * <li>textsize - sets the text size. The value for this entry is a <CODE>Float</CODE>.
        * <li>bgcolor - sets the background color. The value for this entry is a <CODE>java.awt.Color</CODE>.
        *     If <code>null</code> removes the background.<br>
        * <li>bordercolor - sets the border color. The value for this entry is a <CODE>java.awt.Color</CODE>.
        *     If <code>null</code> removes the border.<br>
        * </ul>
        * @param field the field name
        * @param name the property name
        * @param value the property value
        * @param inst an array of <CODE>int</CODE> indexing into <CODE>AcroField.Item.merged</CODE> elements to process.
        * Set to <CODE>null</CODE> to process all
        * @return <CODE>true</CODE> if the property exists, <CODE>false</CODE> otherwise
        */    
        virtual public bool SetFieldProperty(String field, String name, Object value, int[] inst) {
            if (writer == null)
                throw new Exception(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            if (!fields.ContainsKey(field))
                return false;
            Item item = fields[field];
            InstHit hit = new InstHit(inst);
            PdfDictionary merged;
            PdfString da;
            if (Util.EqualsIgnoreCase(name, "textfont")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        merged = item.GetMerged( k );
                        da = merged.GetAsString(PdfName.DA);
                        PdfDictionary dr = merged.GetAsDict(PdfName.DR);
                        if (da != null)
                        {
                            if (dr == null)
                            {
                                dr = new PdfDictionary();
                                merged.Put(PdfName.DR, dr);
                            }
                            Object[] dao = SplitDAelements(da.ToUnicodeString());
                            PdfAppearance cb = new PdfAppearance();
                            if (dao[DA_FONT] != null) {
                                BaseFont bf = (BaseFont)value;
                                PdfName psn;
                                if (!PdfAppearance.stdFieldFontNames.TryGetValue(bf.PostscriptFontName, out psn)) {
                                    psn = new PdfName(bf.PostscriptFontName);
                                }
                                PdfDictionary fonts = dr.GetAsDict(PdfName.FONT);
                                if (fonts == null) {
                                    fonts = new PdfDictionary();
                                    dr.Put(PdfName.FONT, fonts);
                                }
                                PdfIndirectReference fref = (PdfIndirectReference)fonts.Get(psn);
                                PdfDictionary top = reader.Catalog.GetAsDict(PdfName.ACROFORM);
                                MarkUsed(top);
                                dr = top.GetAsDict(PdfName.DR);
                                if (dr == null) {
                                    dr = new PdfDictionary();
                                    top.Put(PdfName.DR, dr);
                                }
                                MarkUsed(dr);
                                PdfDictionary fontsTop = dr.GetAsDict(PdfName.FONT);
                                if (fontsTop == null) {
                                    fontsTop = new PdfDictionary();
                                    dr.Put(PdfName.FONT, fontsTop);
                                }
                                MarkUsed(fontsTop);
                                PdfIndirectReference frefTop = (PdfIndirectReference)fontsTop.Get(psn);
                                if (frefTop != null) {
                                    if (fref == null)
                                        fonts.Put(psn, frefTop);
                                }
                                else if (fref == null) {
                                    FontDetails fd;
                                    if (bf.FontType == BaseFont.FONT_TYPE_DOCUMENT) {
                                        fd = new FontDetails(null, ((DocumentFont)bf).IndirectReference, bf);
                                    }
                                    else {
                                        bf.Subset = false;
                                        fd = writer.AddSimple(bf);
                                        localFonts[psn.ToString().Substring(1)] = bf;
                                    }
                                    fontsTop.Put(psn, fd.IndirectReference);
                                    fonts.Put(psn, fd.IndirectReference);
                                }
                                ByteBuffer buf = cb.InternalBuffer;
                                buf.Append(psn.GetBytes()).Append(' ').Append((float)dao[DA_SIZE]).Append(" Tf ");
                                if (dao[DA_COLOR] != null)
                                    cb.SetColorFill((BaseColor)dao[DA_COLOR]);
                                PdfString s = new PdfString(cb.ToString());
                                item.GetMerged(k).Put(PdfName.DA, s);
                                item.GetWidget(k).Put(PdfName.DA, s);
                                MarkUsed(item.GetWidget(k));
                            }
                        }
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "textcolor")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        merged = item.GetMerged( k );
                        da = merged.GetAsString(PdfName.DA);
                        if (da != null) {
                            Object[] dao = SplitDAelements(da.ToUnicodeString());
                            PdfAppearance cb = new PdfAppearance();
                            if (dao[DA_FONT] != null) {
                                ByteBuffer buf = cb.InternalBuffer;
                                buf.Append(new PdfName((String)dao[DA_FONT]).GetBytes()).Append(' ').Append((float)dao[DA_SIZE]).Append(" Tf ");
                                cb.SetColorFill((BaseColor)value);
                                PdfString s = new PdfString(cb.ToString());
                                item.GetMerged(k).Put(PdfName.DA, s);
                                item.GetWidget(k).Put(PdfName.DA, s);
                                MarkUsed(item.GetWidget(k));
                            }
                        }
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "textsize")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        merged = item.GetMerged( k );
                        da = merged.GetAsString(PdfName.DA);
                        if (da != null) {
                            Object[] dao = SplitDAelements(da.ToUnicodeString());
                            PdfAppearance cb = new PdfAppearance();
                            if (dao[DA_FONT] != null) {
                                ByteBuffer buf = cb.InternalBuffer;
                                buf.Append(new PdfName((String)dao[DA_FONT]).GetBytes()).Append(' ').Append((float)value).Append(" Tf ");
                                if (dao[DA_COLOR] != null)
                                    cb.SetColorFill((BaseColor)dao[DA_COLOR]);
                                PdfString s = new PdfString(cb.ToString());
                                item.GetMerged(k).Put(PdfName.DA, s);
                                item.GetWidget(k).Put(PdfName.DA, s);
                                MarkUsed(item.GetWidget(k));
                            }
                        }
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "bgcolor") || Util.EqualsIgnoreCase(name, "bordercolor")) {
                PdfName dname = (Util.EqualsIgnoreCase(name, "bgcolor") ? PdfName.BG : PdfName.BC);
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        merged = item.GetMerged( k );
                        PdfDictionary mk = merged.GetAsDict(PdfName.MK);
                        if (mk == null) {
                            if (value == null)
                                return true;
                            mk = new PdfDictionary();
                            item.GetMerged(k).Put(PdfName.MK, mk);
                            item.GetWidget(k).Put(PdfName.MK, mk);
                            MarkUsed(item.GetWidget(k));
                        } else {
                            MarkUsed( mk );
                        }
                        if (value == null)
                            mk.Remove(dname);
                        else
                            mk.Put(dname, PdfFormField.GetMKColor((BaseColor)value));
                    }
                }
            }
            else
                return false;
            return true;
        }

        /**
        * Sets a field property. Valid property names are:
        * <p>
        * <ul>
        * <li>flags - a set of flags specifying various characteristics of the field�s widget annotation.
        * The value of this entry replaces that of the F entry in the form�s corresponding annotation dictionary.<br>
        * <li>setflags - a set of flags to be set (turned on) in the F entry of the form�s corresponding
        * widget annotation dictionary. Bits equal to 1 cause the corresponding bits in F to be set to 1.<br>
        * <li>clrflags - a set of flags to be cleared (turned off) in the F entry of the form�s corresponding
        * widget annotation dictionary. Bits equal to 1 cause the corresponding
        * bits in F to be set to 0.<br>
        * <li>fflags - a set of flags specifying various characteristics of the field. The value
        * of this entry replaces that of the Ff entry in the form�s corresponding field dictionary.<br>
        * <li>setfflags - a set of flags to be set (turned on) in the Ff entry of the form�s corresponding
        * field dictionary. Bits equal to 1 cause the corresponding bits in Ff to be set to 1.<br>
        * <li>clrfflags - a set of flags to be cleared (turned off) in the Ff entry of the form�s corresponding
        * field dictionary. Bits equal to 1 cause the corresponding bits in Ff
        * to be set to 0.<br>
        * </ul>
        * @param field the field name
        * @param name the property name
        * @param value the property value
        * @param inst an array of <CODE>int</CODE> indexing into <CODE>AcroField.Item.merged</CODE> elements to process.
        * Set to <CODE>null</CODE> to process all
        * @return <CODE>true</CODE> if the property exists, <CODE>false</CODE> otherwise
        */    
        virtual public bool SetFieldProperty(String field, String name, int value, int[] inst) {
            if (writer == null)
                throw new Exception(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            if (!fields.ContainsKey(field))
                return false;
            Item item = fields[field];
            InstHit hit = new InstHit(inst);
            if (Util.EqualsIgnoreCase(name, "flags")) {
                PdfNumber num = new PdfNumber(value);
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        item.GetMerged(k).Put(PdfName.F, num);
                        item.GetWidget(k).Put(PdfName.F, num);
                        MarkUsed(item.GetWidget(k));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "setflags")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        PdfNumber num = item.GetWidget(k).GetAsNumber(PdfName.F);
                        int val = 0;
                        if (num != null)
                            val = num.IntValue;
                        num = new PdfNumber(val | value);
                        item.GetMerged(k).Put(PdfName.F, num);
                        item.GetWidget(k).Put(PdfName.F, num);
                        MarkUsed(item.GetWidget(k));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "clrflags")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        PdfDictionary widget = item.GetWidget( k );
                        PdfNumber num = widget.GetAsNumber(PdfName.F);
                        int val = 0;
                        if (num != null)
                            val = num.IntValue;
                        num = new PdfNumber(val & (~value));
                        item.GetMerged(k).Put(PdfName.F, num);
                        widget.Put(PdfName.F, num);
                        MarkUsed(widget);
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "fflags")) {
                PdfNumber num = new PdfNumber(value);
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        item.GetMerged(k).Put(PdfName.FF, num);
                        item.GetValue(k).Put(PdfName.FF, num);
                        MarkUsed(item.GetValue(k));
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "setfflags")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        PdfDictionary valDict = item.GetValue( k );
                        PdfNumber num = valDict.GetAsNumber( PdfName.FF );
                        int val = 0;
                        if (num != null)
                            val = num.IntValue;
                        num = new PdfNumber(val | value);
                        item.GetMerged(k).Put(PdfName.FF, num);
                        valDict.Put(PdfName.FF, num);
                        MarkUsed(valDict);
                    }
                }
            }
            else if (Util.EqualsIgnoreCase(name, "clrfflags")) {
                for (int k = 0; k < item.Size; ++k) {
                    if (hit.IsHit(k)) {
                        PdfDictionary valDict = item.GetValue( k );
                        PdfNumber num = valDict.GetAsNumber(PdfName.FF);
                        int val = 0;
                        if (num != null)
                            val = num.IntValue;
                        num = new PdfNumber(val & (~value));
                        item.GetMerged(k).Put(PdfName.FF, num);
                        valDict.Put(PdfName.FF, num);
                        MarkUsed(valDict);
                    }
                }
            }
            else
                return false;
            return true;
        }
        
        /**
        * Merges an XML data structure into this form.
        * @param n the top node of the data structure
        * @throws java.io.IOException on error
        * @throws com.lowagie.text.DocumentException o error
        */
        virtual public void MergeXfaData(XmlNode n) {
            XfaForm.Xml2SomDatasets data = new XfaForm.Xml2SomDatasets(n);
            foreach (String name in data.Order) {
                String text = XfaForm.GetNodeText((XmlNode)data.Name2Node[name]);
                SetField(name, text);
            }
        }

        /** Sets the fields by FDF merging.
        * @param fdf the FDF form
        * @throws IOException on error
        * @throws DocumentException on error
        */    
        virtual public void SetFields(FdfReader fdf) {
            Dictionary<String, PdfDictionary> fd = fdf.Fields;
            foreach (string f in fd.Keys) {
                String v = fdf.GetFieldValue(f);
                if (v != null)
                    SetField(f, v);
            }
        }
        
        /** Sets the fields by XFDF merging.
        * @param xfdf the XFDF form
        * @throws IOException on error
        * @throws DocumentException on error
        */
        
        virtual public void SetFields(XfdfReader xfdf) {
            Dictionary<String, string> fd = xfdf.Fields;
            foreach (string f in fd.Keys) {
                String v = xfdf.GetFieldValue(f);
                if (v != null)
                    SetField(f, v);
                List<String> l = xfdf.GetListValues(f);
                if (l != null) {
                    string[] ar = l.ToArray();
                    SetListSelection(v, ar);
                }
            }
        }

        /**
        * Regenerates the field appearance.
        * This is usefull when you change a field property, but not its value,
        * for instance form.SetFieldProperty("f", "bgcolor", BaseColor.BLUE, null);
        * This won't have any effect, unless you use RegenerateField("f") after changing
        * the property.
        * 
        * @param name the fully qualified field name or the partial name in the case of XFA forms
        * @throws IOException on error
        * @throws DocumentException on error
        * @return <CODE>true</CODE> if the field was found and changed,
        * <CODE>false</CODE> otherwise
        */    
        virtual public bool RegenerateField(String name) {
    	    String value = GetField(name);
            return SetField(name, value, value);
        }

        /** Sets the field value.
        * @param name the fully qualified field name or the partial name in the case of XFA forms
        * @param value the field value
        * @throws IOException on error
        * @throws DocumentException on error
        * @return <CODE>true</CODE> if the field was found and changed,
        * <CODE>false</CODE> otherwise
        */    
        virtual public bool SetField(String name, String value) {
            return SetField(name, value, null);
        }

        /** Sets the field value.
        * @param name the fully qualified field name or the partial name in the case of XFA forms
        * @param value the field value
        * @param saveAppearance save the current appearance of the field or not
        * @throws IOException on error
        * @throws DocumentException on error
        * @return <CODE>true</CODE> if the field was found and changed,
        * <CODE>false</CODE> otherwise
        */
        virtual public bool SetField(String name, String value, bool saveAppearance)
        {
            return SetField(name, value, null, saveAppearance);
        }
        
        /**
         * Sets the rich value for the given field.  See <a href="http://www.adobe.com/content/dam/Adobe/en/devnet/pdf/pdfs/PDF32000_2008.pdf">PDF Reference</a> chapter 
         * 12.7.3.4 (Rich Text) and 12.7.4.3 (Text Fields) for further details. Note that iText doesn't create an appearance for Rich Text fields.
         * So you either need to use XML Worker to create an appearance (/N entry in the /AP dictionary), or you need to use setGenerateAppearances(false) to tell the viewer
         * that iText didn't create any appearances.
         * @param name  Field name
         * @param richValue html markup 
         * @return success/failure (will fail if the field isn't found, isn't a text field, or doesn't support rich text)
         * @throws DocumentException
         * @throws IOException 
         * @since 5.0.6
         */
        virtual public bool SetFieldRichValue(String name, String richValue) {
            if (writer == null) {
                // can't set field values: fail
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            }

            AcroFields.Item item = GetFieldItem(name);
            if (item == null) {
                // can't find the field: fail.
                return false;
            }
            
            if (GetFieldType(name) != FIELD_TYPE_TEXT) {
                // field isn't a text field: fail
                return false;
            }
            
            PdfDictionary merged = item.GetMerged(0);
            PdfNumber ffNum = merged.GetAsNumber(PdfName.FF);
            int flagVal = 0;
            if (ffNum != null) {
                flagVal = ffNum.IntValue;
            }
            if ((flagVal & PdfFormField.FF_RICHTEXT) == 0) {
                // text field doesn't support rich text: fail
                return false;
            }
            
            PdfString richString = new PdfString(richValue);
            item.WriteToAll(PdfName.RV, richString, Item.WRITE_MERGED | Item.WRITE_VALUE);
            
            Stream isp = new MemoryStream(Encoding.Default.GetBytes(richValue));
            PdfString valueString = new PdfString(XmlToTxt.Parse(isp));
            item.WriteToAll(PdfName.V, valueString, Item.WRITE_MERGED | Item.WRITE_VALUE);
            return true;
        }

        /** Sets the field value and the display string. The display string
        * is used to build the appearance in the cases where the value
        * is modified by Acrobat with JavaScript and the algorithm is
        * known.
        * @param name the fully qualified field name or the partial name in the case of XFA forms
        * @param value the field value
        * @param display the string that is used for the appearance. If <CODE>null</CODE>
        * the <CODE>value</CODE> parameter will be used
        * @return <CODE>true</CODE> if the field was found and changed,
        * <CODE>false</CODE> otherwise
        * @throws IOException on error
        * @throws DocumentException on error
        */
        public virtual bool SetField(String name, String value, String display) {
            return SetField(name, value, false);
        }

        /** Sets the field value and the display string. The display string
        * is used to build the appearance in the cases where the value
        * is modified by Acrobat with JavaScript and the algorithm is
        * known.
        * @param name the fully qualified field name or the partial name in the case of XFA forms
        * @param value the field value
        * @param display the string that is used for the appearance. If <CODE>null</CODE>
        * the <CODE>value</CODE> parameter will be used
        * @param saveAppearance save the current appearance of the field or not
        * @return <CODE>true</CODE> if the field was found and changed,
        * <CODE>false</CODE> otherwise
        * @throws IOException on error
        * @throws DocumentException on error
        */
        virtual public bool SetField(String name, String value, String display, bool saveAppearance) {
            if (writer == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.acrofields.instance.is.read.only"));
            if (xfa.XfaPresent) {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                    return false;
                String shortName = XfaForm.Xml2Som.GetShortName(name);
                XmlNode xn = xfa.FindDatasetsNode(shortName);
                if (xn == null) {
                    xn = xfa.DatasetsSom.InsertNode(xfa.DatasetsNode, shortName);
                }
                xfa.SetNodeText(xn, value);
            }
            if (!fields.ContainsKey(name))
                return false;
            Item item = fields[name];
            PdfDictionary merged = item.GetMerged( 0 );
            PdfName type = merged.GetAsName(PdfName.FT);
            if (PdfName.TX.Equals(type)) {
                PdfNumber maxLen = merged.GetAsNumber(PdfName.MAXLEN);
                int len = 0;
                if (maxLen != null)
                    len = maxLen.IntValue;
                if (len > 0)
                    value = value.Substring(0, Math.Min(len, value.Length));
            }
            if (display == null)
                display = value;
            if (PdfName.TX.Equals(type) || PdfName.CH.Equals(type)) {
                PdfString v = new PdfString(value, PdfObject.TEXT_UNICODE);
                for (int idx = 0; idx < item.Size; ++idx) {
                    PdfDictionary valueDic = item.GetValue(idx);
                    valueDic.Put(PdfName.V, v);
                    valueDic.Remove(PdfName.I);
                    MarkUsed(valueDic);                
                    merged = item.GetMerged(idx);
                    merged.Remove(PdfName.I);
                    merged.Put(PdfName.V, v);
                    PdfDictionary widget = item.GetWidget(idx);
                    if (generateAppearances) {
                        PdfAppearance app = GetAppearance(merged, display, name);
                        if (PdfName.CH.Equals(type)) {
                            PdfNumber n = new PdfNumber(topFirst);
                            widget.Put(PdfName.TI, n);
                            merged.Put(PdfName.TI, n);
                        }
                        PdfDictionary appDic = widget.GetAsDict(PdfName.AP);
                        if (appDic == null) {
                            appDic = new PdfDictionary();
                            widget.Put(PdfName.AP, appDic);
                            merged.Put(PdfName.AP, appDic);
                        }
                        appDic.Put(PdfName.N, app.IndirectReference);
                        writer.ReleaseTemplate(app);
                    }
                    else {
                        widget.Remove(PdfName.AP);
                        merged.Remove(PdfName.AP);
                    }
                    MarkUsed(widget);
                }
                return true;
            }
            else if (PdfName.BTN.Equals(type)) {
                PdfNumber ff = item.GetMerged(0).GetAsNumber(PdfName.FF);
                int flags = 0;
                if (ff != null)
                    flags = ff.IntValue;
                if ((flags & PdfFormField.FF_PUSHBUTTON) != 0) {
                    //we'll assume that the value is an image in base64
                    Image img;
                    try {
                        img = Image.GetInstance(Convert.FromBase64String(value));
                    }
                    catch {
                        return false;
                    }
                    PushbuttonField pb = GetNewPushbuttonFromField(name);
                    pb.Image = img;
                    ReplacePushbuttonField(name, pb.Field);
                    return true;
                }
                PdfName v = new PdfName(value);
                List<String> lopt = new List<string>();
                PdfArray opts = item.GetValue(0).GetAsArray(PdfName.OPT);
                if (opts != null) {
                    for (int k = 0; k < opts.Size; ++k) {
                        PdfString valStr = opts.GetAsString(k);
                        if (valStr != null)
                            lopt.Add(valStr.ToUnicodeString());
                        else
                            lopt.Add(null);
                    }
                }
                int vidx = lopt.IndexOf(value);
                PdfName vt;
                if (vidx >= 0)
                    vt = new PdfName(vidx.ToString());
                else
                    vt = v;
                for (int idx = 0; idx < item.Size; ++idx) {
                    merged = item.GetMerged(idx);
                    PdfDictionary widget = item.GetWidget(idx);
                    PdfDictionary valDict = item.GetValue(idx);
                    MarkUsed(item.GetValue(idx));
                    valDict.Put(PdfName.V, vt);
                    merged.Put(PdfName.V, vt);
                    MarkUsed(widget);
                    PdfDictionary appDic = widget.GetAsDict(PdfName.AP);
                    if (appDic == null)
                        return false;
                    PdfDictionary normal = appDic.GetAsDict(PdfName.N);
                    if (IsInAP(normal, vt) || normal == null) {
                        merged.Put(PdfName.AS, vt);
                        widget.Put(PdfName.AS, vt);
                    }
                    else {
                        merged.Put(PdfName.AS, PdfName.Off_);
                        widget.Put(PdfName.AS, PdfName.Off_);
                    }
                    if (generateAppearances && !saveAppearance) {
                        PdfAppearance app = GetAppearance(merged, display, name);
                        if (normal != null)
                            normal.Put(merged.GetAsName(PdfName.AS), app.IndirectReference);
                        else
                            appDic.Put(PdfName.N, app.IndirectReference);
                        writer.ReleaseTemplate(app);
                    }
                }
                return true;
            }
            return false;
        }
        
        /**
        * Sets different values in a list selection.
        * No appearance is generated yet; nor does the code check if multiple select is allowed.
        * 
        * @param    name    the name of the field
        * @param    value   an array with values that need to be selected
        * @return   true only if the field value was changed
        * @since 2.1.4
        */
        virtual public bool SetListSelection(String name, String[] value) {
            Item item = GetFieldItem(name);
            if (item == null)
                return false;
            PdfDictionary merged = item.GetMerged(0);
            PdfName type = merged.GetAsName(PdfName.FT);
            if (!PdfName.CH.Equals(type)) {
                return false;
            }
            String[] options = GetListOptionExport(name);
            PdfArray array = new PdfArray();
            foreach (string element in value) {
                for (int j = 0; j < options.Length; j++) {
                    if (options[j].Equals(element)) {
                        array.Add(new PdfNumber(j));
                    }
                }
            }
            item.WriteToAll(PdfName.I, array, Item.WRITE_MERGED | Item.WRITE_VALUE);
        
            PdfArray vals = new PdfArray();
            for (int i = 0; i < value.Length; ++i) {
                vals.Add( new PdfString( value[i] ) );
            }
            item.WriteToAll(PdfName.V, vals, Item.WRITE_MERGED | Item.WRITE_VALUE);
            
            PdfAppearance app = GetAppearance( merged, value, name ); 
            
            PdfDictionary apDic = new PdfDictionary();
            apDic.Put( PdfName.N, app.IndirectReference);
            item.WriteToAll(PdfName.AP, apDic, Item.WRITE_MERGED | Item.WRITE_WIDGET);
            
            writer.ReleaseTemplate( app );
            
            item.MarkUsed( this, Item.WRITE_VALUE | Item.WRITE_WIDGET );
            return true;
        }

        internal bool IsInAP(PdfDictionary nDic, PdfName check) {
            return nDic != null && nDic.Get(check) != null;
        }
        
        /** Gets all the fields. The fields are keyed by the fully qualified field name and
        * the value is an instance of <CODE>AcroFields.Item</CODE>.
        * @return all the fields
        */    
        virtual public IDictionary<String, Item> Fields {
            get {
                return fields;
            }
        }
        
        /**
        * Gets the field structure.
        * @param name the name of the field
        * @return the field structure or <CODE>null</CODE> if the field
        * does not exist
        */    
        virtual public Item GetFieldItem(String name) {
            if (xfa.XfaPresent) {
                name = xfa.FindFieldName(name, this);
                if (name == null)
                    return null;
            }
            if (!fields.ContainsKey(name))
                return null;
            return fields[name];
        }

        /**
        * Gets the long XFA translated name.
        * @param name the name of the field
        * @return the long field name
        */    
        virtual public String GetTranslatedFieldName(String name) {
            if (xfa.XfaPresent) {
                String namex = xfa.FindFieldName(name, this);
                if (namex != null)
                    name = namex;
            }
            return name;
        }
        
        /**
        * Gets the field box positions in the document. The return is an array of <CODE>float</CODE>
        * multiple of 5. For each of this groups the values are: [page, llx, lly, urx,
        * ury]. The coordinates have the page rotation in consideration.
        * @param name the field name
        * @return the positions or <CODE>null</CODE> if field does not exist
        */    
        virtual public IList<FieldPosition> GetFieldPositions(String name) {
            Item item = GetFieldItem(name);
            if (item == null)
                return null;
            List<FieldPosition> ret = new List<FieldPosition>();
            for (int k = 0; k < item.Size; ++k) {
                try {
                    PdfDictionary wd = item.GetWidget(k);
                    PdfArray rect = wd.GetAsArray(PdfName.RECT);
                    if (rect == null)
                        continue;
                    Rectangle r = PdfReader.GetNormalizedRectangle(rect);
                    int page = item.GetPage(k);
                    int rotation = reader.GetPageRotation(page);
                    FieldPosition fp = new FieldPosition();
                    fp.page = page;
                    if (rotation != 0) {
                        Rectangle pageSize = reader.GetPageSize(page);
                        switch (rotation) {
                            case 270:
                                r = new Rectangle(
                                    pageSize.Top - r.Bottom,
                                    r.Left,
                                    pageSize.Top - r.Top,
                                    r.Right);
                                break;
                            case 180:
                                r = new Rectangle(
                                    pageSize.Right - r.Left,
                                    pageSize.Top - r.Bottom,
                                    pageSize.Right - r.Right,
                                    pageSize.Top - r.Top);
                                break;
                            case 90:
                                r = new Rectangle(
                                    r.Bottom,
                                    pageSize.Right - r.Left,
                                    r.Top,
                                    pageSize.Right - r.Right);
                                break;
                        }
                        r.Normalize();
                    }
                    fp.position = r;
                    ret.Add(fp);
                }
                catch {
                    // empty on purpose
                }
            }
            return ret;
        }
        
        private int RemoveRefFromArray(PdfArray array, PdfObject refo) {
            if (refo == null || !refo.IsIndirect())
                return array.Size;
            PdfIndirectReference refi = (PdfIndirectReference)refo;
            for (int j = 0; j < array.Size; ++j) {
                PdfObject obj = array.GetPdfObject(j);
                if (!obj.IsIndirect())
                    continue;
                if (((PdfIndirectReference)obj).Number == refi.Number)
                    array.Remove(j--);
            }
            return array.Size;
        }
        
        /**
        * Removes all the fields from <CODE>page</CODE>.
        * @param page the page to remove the fields from
        * @return <CODE>true</CODE> if any field was removed, <CODE>false otherwise</CODE>
        */    
        virtual public bool RemoveFieldsFromPage(int page) {
            if (page < 1)
                return false;
            String[] names = new String[fields.Count];
            fields.Keys.CopyTo(names, 0);
            bool found = false;
            for (int k = 0; k < names.Length; ++k) {
                bool fr = RemoveField(names[k], page);
                found = (found || fr);
            }
            return found;
        }
        
        /**
        * Removes a field from the document. If page equals -1 all the fields with this
        * <CODE>name</CODE> are removed from the document otherwise only the fields in
        * that particular page are removed.
        * @param name the field name
        * @param page the page to remove the field from or -1 to remove it from all the pages
        * @return <CODE>true</CODE> if the field exists, <CODE>false otherwise</CODE>
        */    
        virtual public bool RemoveField(String name, int page) {
            Item item = GetFieldItem(name);
            if (item == null)
                return false;
            PdfDictionary acroForm = (PdfDictionary)PdfReader.GetPdfObject(reader.Catalog.Get(PdfName.ACROFORM), reader.Catalog);
            
            if (acroForm == null)
                return false;
            PdfArray arrayf = acroForm.GetAsArray(PdfName.FIELDS);
            if (arrayf == null)
                return false;
            for (int k = 0; k < item.Size; ++k) {
                int pageV = item.GetPage(k);
                if (page != -1 && page != pageV)
                    continue;
                PdfIndirectReference refi = item.GetWidgetRef(k);
                PdfDictionary wd = item.GetWidget( k );
                PdfDictionary pageDic = reader.GetPageN(pageV);
                PdfArray annots = pageDic.GetAsArray(PdfName.ANNOTS);
                if (annots != null) {
                    if (RemoveRefFromArray(annots, refi) == 0) {
                        pageDic.Remove(PdfName.ANNOTS);
                        MarkUsed(pageDic);
                    }
                    else
                        MarkUsed(annots);
                }
                PdfReader.KillIndirect(refi);
                PdfIndirectReference kid = refi;
                while ((refi = wd.GetAsIndirectObject(PdfName.PARENT)) != null) {
                    wd = wd.GetAsDict( PdfName.PARENT );
                    PdfArray kids = wd.GetAsArray(PdfName.KIDS);
                    if (RemoveRefFromArray(kids, kid) != 0)
                        break;
                    kid = refi;
                    PdfReader.KillIndirect(refi);
                }
                if (refi == null) {
                    RemoveRefFromArray(arrayf, kid);
                    MarkUsed(arrayf);
                }
                if (page != -1) {
                    item.Remove( k );
                    --k;
                }
            }
            if (page == -1 || item.Size == 0)
                fields.Remove(name);
            return true;
        }
        
        /**
        * Removes a field from the document.
        * @param name the field name
        * @return <CODE>true</CODE> if the field exists, <CODE>false otherwise</CODE>
        */    
        virtual public bool RemoveField(String name) {
            return RemoveField(name, -1);
        }
        
        /** Sets the option to generate appearances. Not generating apperances
        * will speed-up form filling but the results can be
        * unexpected in Acrobat. Don't use it unless your environment is well
        * controlled. The default is <CODE>true</CODE>.
        * @param generateAppearances the option to generate appearances
        */
        virtual public bool GenerateAppearances {
            set {
                generateAppearances = value;
                PdfDictionary top = reader.Catalog.GetAsDict(PdfName.ACROFORM);
                if (generateAppearances)
                    top.Remove(PdfName.NEEDAPPEARANCES);
                else
                    top.Put(PdfName.NEEDAPPEARANCES, PdfBoolean.PDFTRUE);
            }
            get {
                return generateAppearances;
            }
        }
        
        /** The field representations for retrieval and modification. */    
        public class Item {
            
            /**
            * <CODE>writeToAll</CODE> constant.
            * 
            *  @since 2.1.5
            */
            public const int WRITE_MERGED = 1;
            
            /**
            * <CODE>writeToAll</CODE> and <CODE>markUsed</CODE> constant.
            * 
            *  @since 2.1.5
            */
            public const int WRITE_WIDGET = 2;
            
            /**
            * <CODE>writeToAll</CODE> and <CODE>markUsed</CODE> constant.
            * 
            *  @since 2.1.5
            */
            public const int WRITE_VALUE = 4;

            /**
            * This function writes the given key/value pair to all the instances
            * of merged, widget, and/or value, depending on the <code>writeFlags</code> setting
            *
            * @since 2.1.5
            *
            * @param key        you'll never guess what this is for.
            * @param value      if value is null, the key will be removed
            * @param writeFlags ORed together WRITE_* flags
            */
            virtual public void WriteToAll(PdfName key, PdfObject value, int writeFlags) {
                int i;
                PdfDictionary curDict = null;
                if ((writeFlags & WRITE_MERGED) != 0) {
                    for (i = 0; i < merged.Count; ++i) {
                        curDict = GetMerged(i);
                        curDict.Put(key, value);
                    }
                }
                if ((writeFlags & WRITE_WIDGET) != 0) {
                    for (i = 0; i < widgets.Count; ++i) {
                        curDict = GetWidget(i);
                        curDict.Put(key, value);
                    }
                }
                if ((writeFlags & WRITE_VALUE) != 0) {
                    for (i = 0; i < values.Count; ++i) {
                        curDict = GetValue(i);
                        curDict.Put(key, value);
                    }
                }
            }

            /**
            * Mark all the item dictionaries used matching the given flags
            * 
            * @since 2.1.5
            * @param writeFlags WRITE_MERGED is ignored
            */
            virtual public void MarkUsed( AcroFields parentFields, int writeFlags ) {
                if ((writeFlags & WRITE_VALUE) != 0) {
                    for (int i = 0; i < Size; ++i) {
                        parentFields.MarkUsed( GetValue( i ) );
                    }
                }
                if ((writeFlags & WRITE_WIDGET) != 0) {
                    for (int i = 0; i < Size; ++i) {
                        parentFields.MarkUsed(GetWidget(i));
                    }
                }
            }

            /**
            * An array of <CODE>PdfDictionary</CODE> where the value tag /V
            * is present.
            * 
            */
            protected internal List<PdfDictionary> values = new List<PdfDictionary>();
            
            /**
            * An array of <CODE>PdfDictionary</CODE> with the widgets.
            * 
            */
            protected internal List<PdfDictionary> widgets = new List<PdfDictionary>();
            
            /**
            * An array of <CODE>PdfDictionary</CODE> with the widget references.
            * 
            */
            protected internal List<PdfIndirectReference> widget_refs = new List<PdfIndirectReference>();
            
            /**
            * An array of <CODE>PdfDictionary</CODE> with all the field
            * and widget tags merged.
            * 
            */
            protected internal List<PdfDictionary> merged = new List<PdfDictionary>();
            
            /**
            * An array of <CODE>Integer</CODE> with the page numbers where
            * the widgets are displayed.
            * 
            */
            protected internal List<int> page = new List<int>();
            /**
            * An array of <CODE>Integer</CODE> with the tab order of the field in the page.
            * 
            */
            protected internal List<int> tabOrder = new List<int>();

            /**
            * Preferred method of determining the number of instances
            * of a given field.
            * 
            * @since 2.1.5
            * @return number of instances
            */
            virtual public int Size {
                get {
                    return values.Count;
                }
            }

            /**
            * Remove the given instance from this item.  It is possible to
            * remove all instances using this function.
            * 
            * @since 2.1.5
            * @param killIdx
            */
            internal void Remove(int killIdx) {
                values.RemoveAt(killIdx);
                widgets.RemoveAt(killIdx);
                widget_refs.RemoveAt(killIdx);
                merged.RemoveAt(killIdx);
                page.RemoveAt(killIdx);
                tabOrder.RemoveAt(killIdx);
            }

            /**
            * Retrieve the value dictionary of the given instance
            * 
            * @since 2.1.5
            * @param idx instance index
            * @return dictionary storing this instance's value.  It may be shared across instances.
            */
            virtual public PdfDictionary GetValue(int idx) {
                return values[idx];
            }

            /**
            * Add a value dict to this Item
            * 
            * @since 2.1.5
            * @param value new value dictionary
            */
            internal void AddValue(PdfDictionary value) {
                values.Add(value);
            }

            /**
            * Retrieve the widget dictionary of the given instance
            * 
            * @since 2.1.5
            * @param idx instance index
            * @return The dictionary found in the appropriate page's Annot array.
            */
            virtual public PdfDictionary GetWidget(int idx) {
                return widgets[idx];
            }

            /**
            * Add a widget dict to this Item
            * 
            * @since 2.1.5
            * @param widget
            */
            internal void AddWidget(PdfDictionary widget) {
                widgets.Add(widget);
            }

            /**
            * Retrieve the reference to the given instance
            * 
            * @since 2.1.5
            * @param idx instance index
            * @return reference to the given field instance
            */
            virtual public PdfIndirectReference GetWidgetRef(int idx) {
                return widget_refs[idx];
            }

            /**
            * Add a widget ref to this Item
            * 
            * @since 2.1.5
            * @param widgRef
            */
            internal void AddWidgetRef(PdfIndirectReference widgRef) {
                widget_refs.Add(widgRef);
            }

            /**
            * Retrieve the merged dictionary for the given instance.  The merged
            * dictionary contains all the keys present in parent fields, though they
            * may have been overwritten (or modified?) by children.
            * Example: a merged radio field dict will contain /V
            * 
            * @since 2.1.5
            * @param idx  instance index
            * @return the merged dictionary for the given instance
            */
            virtual public PdfDictionary GetMerged(int idx) {
                return merged[idx];
            }

            /**
            * Adds a merged dictionary to this Item.
            * 
            * @since 2.1.5
            * @param mergeDict
            */
            internal void AddMerged(PdfDictionary mergeDict) {
                merged.Add(mergeDict);
            }

            /**
            * Retrieve the page number of the given instance
            * 
            * @since 2.1.5
            * @param idx
            * @return remember, pages are "1-indexed", not "0-indexed" like field instances.
            */
            virtual public int GetPage(int idx) {
                return page[idx];
            }

            /**
            * Adds a page to the current Item.
            * 
            * @since 2.1.5
            * @param pg
            */
            internal void AddPage(int pg) {
                page.Add(pg);
            }

            /**
            * forces a page value into the Item.
            * 
            * @since 2.1.5
            * @param idx
            */
            internal void ForcePage(int idx, int pg) {
                page[idx] = pg;
            }

            /**
            * Gets the tabOrder.
            * 
            * @since 2.1.5
            * @param idx
            * @return tab index of the given field instance
            */
            virtual public int GetTabOrder(int idx) {
                return tabOrder[idx];
            }

            /**
            * Adds a tab order value to this Item.
            * 
            * @since 2.1.5
            * @param order
            */
            internal void AddTabOrder(int order) {
                tabOrder.Add(order);
            }
        }
        
        private class InstHit {
            IntHashtable hits;
            public InstHit(int[] inst) {
                if (inst == null)
                    return;
                hits = new IntHashtable();
                for (int k = 0; k < inst.Length; ++k)
                    hits[inst[k]] = 1;
            }
            
            virtual public bool IsHit(int n) {
                if (hits == null)
                    return true;
                return hits.ContainsKey(n);
            }
        }

        /**
         * Clears a signed field.
         * @param name the field name
         * @return true if the field was signed, false if the field was not signed or not found
         * @since 5.0.5
         */
        virtual public bool ClearSignatureField(String name) {
            sigNames = null;
            FindSignatureNames();
            if (!sigNames.ContainsKey(name))
                return false;
            Item sig = fields[name];
            sig.MarkUsed(this, Item.WRITE_VALUE | Item.WRITE_WIDGET);
            int n = sig.Size;
            for (int k = 0; k < n; ++k) {
                ClearSigDic(sig.GetMerged(k));
                ClearSigDic(sig.GetWidget(k));
                ClearSigDic(sig.GetValue(k));
            }
            return true;
        }

        private static void ClearSigDic(PdfDictionary dic) {
            dic.Remove(PdfName.AP);
            dic.Remove(PdfName.AS);
            dic.Remove(PdfName.V);
            dic.Remove(PdfName.DV);
            dic.Remove(PdfName.SV);
            dic.Remove(PdfName.FF);
            dic.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
        }

        private List<string> orderedSignatureNames;
        
        private void FindSignatureNames() {
            if (sigNames != null)
                return;
            sigNames = new Dictionary<string,int[]>();
            orderedSignatureNames = new List<string>();
            List<object[]> sorter = new List<object[]>();
            foreach (KeyValuePair<string,Item> entry in fields) {
                Item item = entry.Value;
                PdfDictionary merged = item.GetMerged(0);
                if (!PdfName.SIG.Equals(merged.Get(PdfName.FT)))
                    continue;
                PdfDictionary v = merged.GetAsDict(PdfName.V);
                if (v == null)
                    continue;
                PdfString contents = v.GetAsString(PdfName.CONTENTS);
                if (contents == null)
                    continue;
                PdfArray ro = v.GetAsArray(PdfName.BYTERANGE);
                if (ro == null)
                    continue;
                int rangeSize = ro.Size;
                if (rangeSize < 2)
                    continue;
                int length = ro.GetAsNumber(rangeSize - 1).IntValue + ro.GetAsNumber(rangeSize - 2).IntValue;
                sorter.Add(new Object[]{entry.Key, new int[]{length, 0}});
            }
            sorter.Sort(new AcroFields.ISorterComparator());
            if (sorter.Count > 0) {
                if (((int[])sorter[sorter.Count - 1][1])[0] == reader.FileLength)
                    totalRevisions = sorter.Count;
                else
                    totalRevisions = sorter.Count + 1;
                for (int k = 0; k < sorter.Count; ++k) {
                    Object[] objs = sorter[k];
                    String name = (String)objs[0];
                    int[] p = (int[])objs[1];
                    p[1] = k + 1;
                    sigNames[name] = p;
                    orderedSignatureNames.Add(name);
                }
            }
        }

        /**
        * Gets the field names that have signatures and are signed.
        * @return the field names that have signatures and are signed
        */    
        virtual public List<string> GetSignatureNames() {
            FindSignatureNames();
            return new List<string>(orderedSignatureNames);
        }
        
        /**
        * Gets the field names that have blank signatures.
        * @return the field names that have blank signatures
        */    
        virtual public List<string> GetBlankSignatureNames() {
            FindSignatureNames();
            List<String> sigs = new List<String>();
            foreach (KeyValuePair<string,Item> entry in fields) {
                Item item = entry.Value;
                PdfDictionary merged = item.GetMerged(0);
                if (!PdfName.SIG.Equals(merged.GetAsName(PdfName.FT)))
                    continue;
                if (sigNames.ContainsKey(entry.Key))
                    continue;
                sigs.Add(entry.Key);
            }
            return sigs;
        }
        
        /**
        * Gets the signature dictionary, the one keyed by /V.
        * @param name the field name
        * @return the signature dictionary keyed by /V or <CODE>null</CODE> if the field is not
        * a signature
        */    
        virtual public PdfDictionary GetSignatureDictionary(String name) {
            FindSignatureNames();
            name = GetTranslatedFieldName(name);
            if (!sigNames.ContainsKey(name))
                return null;
            Item item = fields[name];
            PdfDictionary merged = item.GetMerged(0);
            return merged.GetAsDict(PdfName.V);
        }


        /**
         * Gets a reference to the normal appearance of a field.
         *
         * @param name the field name
         * @return a reference to the /N entry of the /AP dictionary or <CODE>null</CODE> if the field is not found
         */
        virtual public PdfIndirectReference GetNormalAppearance(String name) {
            GetSignatureNames();
            name = GetTranslatedFieldName(name);
            Item item = fields[name];
            if (item == null)
        	    return null;
            PdfDictionary merged = item.GetMerged(0);
            PdfDictionary ap = merged.GetAsDict(PdfName.AP);
            if (ap == null)
        	    return null;
            PdfIndirectReference refa = ap.GetAsIndirectObject(PdfName.N);
            if (refa == null)
        	    return null;
            return refa;
        }


        /**
        * Checks is the signature covers the entire document or just part of it.
        * @param name the signature field name
        * @return <CODE>true</CODE> if the signature covers the entire document,
        * <CODE>false</CODE> otherwise
        */    
        virtual public bool SignatureCoversWholeDocument(String name) {
            FindSignatureNames();
            name = GetTranslatedFieldName(name);
            if (!sigNames.ContainsKey(name))
                return false;
            return sigNames[name][0] == reader.FileLength;
        }
        
        /**
        * Verifies a signature. An example usage is:
        * <p>
        * <pre>
        * KeyStore kall = PdfPKCS7.LoadCacertsKeyStore();
        * PdfReader reader = new PdfReader("my_signed_doc.pdf");
        * AcroFields af = reader.GetAcroFields();
        * ArrayList names = af.GetSignatureNames();
        * for (int k = 0; k &lt; names.Size(); ++k) {
        *    String name = (String)names.Get(k);
        *    System.out.Println("Signature name: " + name);
        *    System.out.Println("Signature covers whole document: " + af.SignatureCoversWholeDocument(name));
        *    PdfPKCS7 pk = af.VerifySignature(name);
        *    Calendar cal = pk.GetSignDate();
        *    Certificate pkc[] = pk.GetCertificates();
        *    System.out.Println("Subject: " + PdfPKCS7.GetSubjectFields(pk.GetSigningCertificate()));
        *    System.out.Println("Document modified: " + !pk.Verify());
        *    Object fails[] = PdfPKCS7.VerifyCertificates(pkc, kall, null, cal);
        *    if (fails == null)
        *        System.out.Println("Certificates verified against the KeyStore");
        *    else
        *        System.out.Println("Certificate failed: " + fails[1]);
        * }
        * </pre>
        * @param name the signature field name
        * @return a <CODE>PdfPKCS7</CODE> class to continue the verification
        */    
        virtual public PdfPKCS7 VerifySignature(String name) {
            PdfDictionary v = GetSignatureDictionary(name);
            if (v == null)
                return null;
            PdfName sub = v.GetAsName(PdfName.SUBFILTER);
            PdfString contents = v.GetAsString(PdfName.CONTENTS);
            PdfPKCS7 pk = null;
            if (sub.Equals(PdfName.ADBE_X509_RSA_SHA1)) {
                PdfString cert = v.GetAsString(PdfName.CERT);
                if (cert == null)
                    cert = v.GetAsArray(PdfName.CERT).GetAsString(0);
                pk = new PdfPKCS7(contents.GetOriginalBytes(), cert.GetBytes());
            }
            else
                pk = new PdfPKCS7(contents.GetOriginalBytes(), sub);
            UpdateByteRange(pk, v);
            PdfString str = v.GetAsString(PdfName.M);
            if (str != null)
                pk.SignDate = PdfDate.Decode(str.ToString());
            PdfObject obj = PdfReader.GetPdfObject(v.Get(PdfName.NAME));
            if (obj != null) {
              if (obj.IsString())
                pk.SignName = ((PdfString)obj).ToUnicodeString();
              else if(obj.IsName())
                pk.SignName = PdfName.DecodeName(obj.ToString());
            }
            str = v.GetAsString(PdfName.REASON);
            if (str != null)
                pk.Reason = str.ToUnicodeString();
            str = v.GetAsString(PdfName.LOCATION);
            if (str != null)
                pk.Location = str.ToUnicodeString();
            return pk;
        }
        
        private void UpdateByteRange(PdfPKCS7 pkcs7, PdfDictionary v) {
            PdfArray b = v.GetAsArray(PdfName.BYTERANGE);
            RandomAccessFileOrArray rf = reader.SafeFile;
            Stream rg = null;
            try {
                rg = new RASInputStream(new RandomAccessSourceFactory().CreateRanged(rf.CreateSourceView(), b.AsLongArray()));
                byte[] buf = new byte[8192];
                int rd;
                while ((rd = rg.Read(buf, 0, buf.Length)) > 0) {
                    pkcs7.Update(buf, 0, rd);
                }
            } finally {
                if (rg != null) rg.Close();
            }
        }

        /**
        * Gets the total number of revisions this document has.
        * @return the total number of revisions
        */
        virtual public int TotalRevisions {
            get {
                FindSignatureNames();
                return this.totalRevisions;
            }
        }
        
        /**
        * Gets this <CODE>field</CODE> revision.
        * @param field the signature field name
        * @return the revision or zero if it's not a signature field
        */    
        virtual public int GetRevision(String field) {
            FindSignatureNames();
            field = GetTranslatedFieldName(field);
            if (!sigNames.ContainsKey(field))
                return 0;
            return sigNames[field][1];
        }
        
        /**
        * Extracts a revision from the document.
        * @param field the signature field name
        * @return an <CODE>Stream</CODE> covering the revision. Returns <CODE>null</CODE> if
        * it's not a signature field
        * @throws IOException on error
        */    
        virtual public Stream ExtractRevision(String field) {
            FindSignatureNames();
            field = GetTranslatedFieldName(field);
            if (!sigNames.ContainsKey(field))
                return null;
            int length = sigNames[field][0];
            RandomAccessFileOrArray raf = reader.SafeFile;
            raf.ReOpen();
            raf.Seek(0);
            return new RASInputStream(new WindowRandomAccessSource(raf.CreateSourceView(), 0, length));
        }

        /**
        * Sets a cache for field appearances. Parsing the existing PDF to
        * create a new TextField is time expensive. For those tasks that repeatedly
        * fill the same PDF with different field values the use of the cache has dramatic
        * speed advantages. An example usage:
        * <p>
        * <pre>
        * String pdfFile = ...;// the pdf file used as template
        * ArrayList xfdfFiles = ...;// the xfdf file names
        * ArrayList pdfOutFiles = ...;// the output file names, one for each element in xpdfFiles
        * Hashtable cache = new Hashtable();// the appearances cache
        * PdfReader originalReader = new PdfReader(pdfFile);
        * for (int k = 0; k &lt; xfdfFiles.Size(); ++k) {
        *    PdfReader reader = new PdfReader(originalReader);
        *    XfdfReader xfdf = new XfdfReader((String)xfdfFiles.Get(k));
        *    PdfStamper stp = new PdfStamper(reader, new FileOutputStream((String)pdfOutFiles.Get(k)));
        *    AcroFields af = stp.GetAcroFields();
        *    af.SetFieldCache(cache);
        *    af.SetFields(xfdf);
        *    stp.Close();
        * }
        * </pre>
        * @param fieldCache an HasMap that will carry the cached appearances
        */
        virtual public IDictionary<String, TextField> FieldCache {
            set {
                fieldCache = value;
            }
            get {
                return fieldCache;
            }
        }
        
        private void MarkUsed(PdfObject obj) {
            if (!append)
                return;
            ((PdfStamperImp)writer).MarkUsed(obj);
        }

        /**
        * Sets extra margins in text fields to better mimic the Acrobat layout.
        * @param extraMarginLeft the extra marging left
        * @param extraMarginTop the extra margin top
        */    
        virtual public void SetExtraMargin(float extraMarginLeft, float extraMarginTop) {
            this.extraMarginLeft = extraMarginLeft;
            this.extraMarginTop = extraMarginTop;
        }

        /**
        * Adds a substitution font to the list. The fonts in this list will be used if the original
        * font doesn't contain the needed glyphs.
        * @param font the font
        */
        virtual public void AddSubstitutionFont(BaseFont font) {
            if (substitutionFonts == null)
                substitutionFonts = new List<BaseFont>();
            substitutionFonts.Add(font);
        }

        private static Dictionary<string,string[]> stdFieldFontNames = new Dictionary<string,string[]>();
        
        /**
        * Holds value of property fieldCache.
        */
        private IDictionary<String, TextField> fieldCache;

        private int totalRevisions;

        static AcroFields() {
            stdFieldFontNames["CoBO"] = new String[]{"Courier-BoldOblique"};
            stdFieldFontNames["CoBo"] = new String[]{"Courier-Bold"};
            stdFieldFontNames["CoOb"] = new String[]{"Courier-Oblique"};
            stdFieldFontNames["Cour"] = new String[]{"Courier"};
            stdFieldFontNames["HeBO"] = new String[]{"Helvetica-BoldOblique"};
            stdFieldFontNames["HeBo"] = new String[]{"Helvetica-Bold"};
            stdFieldFontNames["HeOb"] = new String[]{"Helvetica-Oblique"};
            stdFieldFontNames["Helv"] = new String[]{"Helvetica"};
            stdFieldFontNames["Symb"] = new String[]{"Symbol"};
            stdFieldFontNames["TiBI"] = new String[]{"Times-BoldItalic"};
            stdFieldFontNames["TiBo"] = new String[]{"Times-Bold"};
            stdFieldFontNames["TiIt"] = new String[]{"Times-Italic"};
            stdFieldFontNames["TiRo"] = new String[]{"Times-Roman"};
            stdFieldFontNames["ZaDb"] = new String[]{"ZapfDingbats"};
            stdFieldFontNames["HySm"] = new String[]{"HYSMyeongJo-Medium", "UniKS-UCS2-H"};
            stdFieldFontNames["HyGo"] = new String[]{"HYGoThic-Medium", "UniKS-UCS2-H"};
            stdFieldFontNames["KaGo"] = new String[]{"HeiseiKakuGo-W5", "UniKS-UCS2-H"};
            stdFieldFontNames["KaMi"] = new String[]{"HeiseiMin-W3", "UniJIS-UCS2-H"};
            stdFieldFontNames["MHei"] = new String[]{"MHei-Medium", "UniCNS-UCS2-H"};
            stdFieldFontNames["MSun"] = new String[]{"MSung-Light", "UniCNS-UCS2-H"};
            stdFieldFontNames["STSo"] = new String[]{"STSong-Light", "UniGB-UCS2-H"};
        }

        private class ISorterComparator : IComparer<Object[]> {        
            virtual public int Compare(Object[] o1, Object[] o2) {
                int n1 = ((int[])o1[1])[0];
                int n2 = ((int[])o2[1])[0];
                return n1 - n2;
            }        
        }

        /**
        * Sets a list of substitution fonts. The list is composed of <CODE>BaseFont</CODE> and can also be <CODE>null</CODE>. The fonts in this list will be used if the original
        * font doesn't contain the needed glyphs.
        * @param substitutionFonts the list
        */
        virtual public List<BaseFont> SubstitutionFonts {
            set {
                substitutionFonts = value;
            }
            get {
                return substitutionFonts;
            }
        }

        /**
        * Gets the XFA form processor.
        * @return the XFA form processor
        */
        virtual public XfaForm Xfa {
            get {
                return xfa;
            }
        }

        /**
        * Removes the XFA stream from the document.
        */
        virtual public void RemoveXfa() {
            PdfDictionary root = reader.Catalog;
            PdfDictionary acroform = root.GetAsDict(PdfName.ACROFORM);
            acroform.Remove(PdfName.XFA);
            xfa = new XfaForm(reader);
        }

        private static readonly PdfName[] buttonRemove = {PdfName.MK, PdfName.F , PdfName.FF , PdfName.Q , PdfName.BS , PdfName.BORDER};
        
        /**
        * Creates a new pushbutton from an existing field. If there are several pushbuttons with the same name
        * only the first one is used. This pushbutton can be changed and be used to replace 
        * an existing one, with the same name or other name, as long is it is in the same document. To replace an existing pushbutton
        * call {@link #replacePushbuttonField(String,PdfFormField)}.
        * @param field the field name that should be a pushbutton
        * @return a new pushbutton or <CODE>null</CODE> if the field is not a pushbutton
        */
        virtual public PushbuttonField GetNewPushbuttonFromField(String field) {
            return GetNewPushbuttonFromField(field, 0);
        }

        /**
        * Creates a new pushbutton from an existing field. This pushbutton can be changed and be used to replace 
        * an existing one, with the same name or other name, as long is it is in the same document. To replace an existing pushbutton
        * call {@link #replacePushbuttonField(String,PdfFormField,int)}.
        * @param field the field name that should be a pushbutton
        * @param order the field order in fields with same name
        * @return a new pushbutton or <CODE>null</CODE> if the field is not a pushbutton
        */
        virtual public PushbuttonField GetNewPushbuttonFromField(String field, int order) {
            if (GetFieldType(field) != FIELD_TYPE_PUSHBUTTON)
                return null;
            Item item = GetFieldItem(field);
            if (order >= item.Size)
                return null;
            IList<FieldPosition> pos = GetFieldPositions(field);
            Rectangle box = pos[order].position;
            PushbuttonField newButton = new PushbuttonField(writer, box, null);
            PdfDictionary dic = item.GetMerged(order);
            DecodeGenericDictionary(dic, newButton);
            PdfDictionary mk = dic.GetAsDict(PdfName.MK);
            if (mk != null) {
                PdfString text = mk.GetAsString(PdfName.CA);
                if (text != null)
                    newButton.Text = text.ToUnicodeString();
                PdfNumber tp = mk.GetAsNumber(PdfName.TP);
                if (tp != null)
                    newButton.Layout = tp.IntValue + 1;
                PdfDictionary ifit = mk.GetAsDict(PdfName.IF);
                if (ifit != null) {
                    PdfName sw = ifit.GetAsName(PdfName.SW);
                    if (sw != null) {
                        int scale = PushbuttonField.SCALE_ICON_ALWAYS;
                        if (sw.Equals(PdfName.B))
                            scale = PushbuttonField.SCALE_ICON_IS_TOO_BIG;
                        else if (sw.Equals(PdfName.S))
                            scale = PushbuttonField.SCALE_ICON_IS_TOO_SMALL;
                        else if (sw.Equals(PdfName.N))
                            scale = PushbuttonField.SCALE_ICON_NEVER;
                        newButton.ScaleIcon = scale;
                    }
                    sw = ifit.GetAsName(PdfName.S);
                    if (sw != null) {
                        if (sw.Equals(PdfName.A))
                            newButton.ProportionalIcon = false;
                    }
                    PdfArray aj = ifit.GetAsArray(PdfName.A);
                    if (aj != null && aj.Size == 2) {
                        float left = aj.GetAsNumber(0).FloatValue;
                        float bottom = aj.GetAsNumber(1).FloatValue;
                        newButton.IconHorizontalAdjustment = left;
                        newButton.IconVerticalAdjustment = bottom;
                    }
                    PdfBoolean fb = ifit.GetAsBoolean(PdfName.FB);
                    if (fb != null && fb.BooleanValue)
                        newButton.IconFitToBounds = true;
                }
                PdfObject i = mk.Get(PdfName.I);
                if (i != null && i.IsIndirect())
                    newButton.IconReference = (PRIndirectReference)i;
            }
            return newButton;
        }
        
        /**
        * Replaces the first field with a new pushbutton. The pushbutton can be created with
        * {@link #getNewPushbuttonFromField(String)} from the same document or it can be a
        * generic PdfFormField of the type pushbutton.
        * @param field the field name
        * @param button the <CODE>PdfFormField</CODE> representing the pushbutton
        * @return <CODE>true</CODE> if the field was replaced, <CODE>false</CODE> if the field
        * was not a pushbutton
        */
        virtual public bool ReplacePushbuttonField(String field, PdfFormField button) {
            return ReplacePushbuttonField(field, button, 0);
        }
        
        /**
        * Replaces the designated field with a new pushbutton. The pushbutton can be created with
        * {@link #getNewPushbuttonFromField(String,int)} from the same document or it can be a
        * generic PdfFormField of the type pushbutton.
        * @param field the field name
        * @param button the <CODE>PdfFormField</CODE> representing the pushbutton
        * @param order the field order in fields with same name
        * @return <CODE>true</CODE> if the field was replaced, <CODE>false</CODE> if the field
        * was not a pushbutton
        */
        virtual public bool ReplacePushbuttonField(String field, PdfFormField button, int order) {
            if (GetFieldType(field) != FIELD_TYPE_PUSHBUTTON)
                return false;
            Item item = GetFieldItem(field);
            if (order >= item.Size)
                return false;
            PdfDictionary merged = item.GetMerged(order);
            PdfDictionary values = item.GetValue(order);
            PdfDictionary widgets = item.GetWidget(order);
            for (int k = 0; k < buttonRemove.Length; ++k) {
                merged.Remove(buttonRemove[k]);
                values.Remove(buttonRemove[k]);
                widgets.Remove(buttonRemove[k]);
            }
            foreach (PdfName key in button.Keys) {
                if (key.Equals(PdfName.T))
                    continue;
                if (key.Equals(PdfName.FF))
                    values.Put(key, button.Get(key));
                else
                    widgets.Put(key, button.Get(key));
                merged.Put(key, button.Get(key));
                MarkUsed(values);
                MarkUsed(widgets);
            }
            return true;
        }

        /**
         * Checks whether a name exists as a signature field or not. It checks both signed fields and blank signatures.
         * @param name String
         * @return boolean does the signature field exist
         * @since 5.5.1
         */
        public virtual bool DoesSignatureFieldExist(String name) {
            return GetBlankSignatureNames().Contains(name) || GetSignatureNames().Contains(name);
        }

        /**
         * A class representing a field position
         * @since 5.0.2
         */
        public class FieldPosition {
            public int page;
            public Rectangle position;
        }
    }
}
