using System;
using System.util;
using System.IO;
using System.Collections.Generic;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
    /** Writes an FDF form.
    * @author Paulo Soares
    */
    public class FdfWriter {
        private static readonly byte[] HEADER_FDF = DocWriter.GetISOBytes("%FDF-1.2\n%\u00e2\u00e3\u00cf\u00d3\n");
        Dictionary<String, Object> fields = new Dictionary<string,object>();

        /** The PDF file associated with the FDF. */
        private String file;
        
        /** Creates a new FdfWriter. */    
        public FdfWriter() {
        }

        /** Writes the content to a stream.
        * @param os the stream
        * @throws DocumentException on error
        * @throws IOException on error
        */    
        public void WriteTo(Stream os) {
            Wrt wrt = new Wrt(os, this);
            wrt.WriteTo();
        }
        
        internal bool SetField(String field, PdfObject value) {
            Dictionary<String, Object> map = fields;
            StringTokenizer tk = new StringTokenizer(field, ".");
            if (!tk.HasMoreTokens())
                return false;
            while (true) {
                String s = tk.NextToken();
                Object obj = map[s];
                if (tk.HasMoreTokens()) {
                    if (obj == null) {
                        obj = new Dictionary<String, Object>();
                        map[s] = obj;
                        map = (Hashtable)obj;
                        continue;
                    }
                    else if (obj is Dictionary<String, Object>)
                        map = (Dictionary<String, Object>)obj;
                    else
                        return false;
                }
                else {
                    if (!(obj is Dictionary<String, Object>)) {
                        map[s] = value;
                        return true;
                    }
                    else
                        return false;
                }
            }
        }
        
        internal void IterateFields(Dictionary<String, Object> values, Dictionary<String, Object> map, String name) {
            foreach (KeyValuePair<String, Object> entry in map) {
                String s = entry.Key;
                Object obj = entry.Value;
                if (obj is Dictionary<String, Object>)
                    IterateFields(values, (Dictionary<String, Object>)obj, name + "." + s);
                else
                    values[(name + "." + s).Substring(1)] = obj;
            }
        }
        
        /** Removes the field value.
        * @param field the field name
        * @return <CODE>true</CODE> if the field was found and removed,
        * <CODE>false</CODE> otherwise
        */    
        public bool RemoveField(String field) {
            Dictionary<String, Object> map = fields;
            StringTokenizer tk = new StringTokenizer(field, ".");
            if (!tk.HasMoreTokens())
                return false;
            List<object> hist = new List<object>();
            while (true) {
                String s = tk.NextToken();
                Object obj = map[s];
                if (obj == null)
                    return false;
                hist.Add(map);
                hist.Add(s);
                if (tk.HasMoreTokens()) {
                    if (obj is Dictionary<String, Object>)
                        map = (Dictionary<String, Object>)obj;
                    else
                        return false;
                }
                else {
                    if (obj is Dictionary<String, Object>)
                        return false;
                    else
                        break;
                }
            }
            for (int k = hist.Count - 2; k >= 0; k -= 2) {
                map = (Dictionary<String, Object>)hist[k];
                String s = (String)hist[k + 1];
                map.Remove(s);
                if (map.Count > 0)
                    break;
            }
            return true;
        }
        
        /** Gets all the fields. The map is keyed by the fully qualified
        * field name and the values are <CODE>PdfObject</CODE>.
        * @return a map with all the fields
        */    
        public Dictionary<String, Object> GetFields() {
            Dictionary<String, Object> values = new Dictionary<String, Object>();
            IterateFields(values, fields, "");
            return values;
        }
        
        /** Gets the field value.
        * @param field the field name
        * @return the field value or <CODE>null</CODE> if not found
        */    
        public String GetField(String field) {
            Dictionary<String, Object> map = fields;
            StringTokenizer tk = new StringTokenizer(field, ".");
            if (!tk.HasMoreTokens())
                return null;
            while (true) {
                String s = tk.NextToken();
                Object obj;
                map.TryGetValue(s, out obj);
                if (obj == null)
                    return null;
                if (tk.HasMoreTokens()) {
                    if (obj is Dictionary<String, Object>)
                        map = (Dictionary<String, Object>)obj;
                    else
                        return null;
                }
                else {
                    if (obj is Dictionary<String, Object>)
                        return null;
                    else {
                        if (((PdfObject)obj).IsString())
                            return ((PdfString)obj).ToUnicodeString();
                        else
                            return PdfName.DecodeName(obj.ToString());
                    }
                }
            }
        }
        
        /** Sets the field value as a name.
        * @param field the fully qualified field name
        * @param value the value
        * @return <CODE>true</CODE> if the value was inserted,
        * <CODE>false</CODE> if the name is incompatible with
        * an existing field
        */    
        public bool SetFieldAsName(String field, String value) {
            return SetField(field, new PdfName(value));
        }
        
        /** Sets the field value as a string.
        * @param field the fully qualified field name
        * @param value the value
        * @return <CODE>true</CODE> if the value was inserted,
        * <CODE>false</CODE> if the name is incompatible with
        * an existing field
        */    
        public bool SetFieldAsString(String field, String value) {
            return SetField(field, new PdfString(value, PdfObject.TEXT_UNICODE));
        }
        
        /**
         * Sets the field value as a <CODE>PDFAction</CODE>. 
         * For example, this method allows setting a form submit button action using {@link PdfAction#createSubmitForm(String, Object[], int)}.
         * This method creates an <CODE>A</CODE> entry for the specified field in the underlying FDF file.
         * Method contributed by Philippe Laflamme (plaflamme)
         * @param field the fully qualified field name
         * @param action the field's action
         * @return <CODE>true</CODE> if the value was inserted,
         * <CODE>false</CODE> if the name is incompatible with
         * an existing field
         * @since	2.1.5
         */
        public bool SetFieldAsAction(String field, PdfAction action) {
            return SetField(field, action);
        }
    
        /** Sets all the fields from this <CODE>FdfReader</CODE>
        * @param fdf the <CODE>FdfReader</CODE>
        */    
        public void SetFields(FdfReader fdf) {
            Dictionary<String, PdfDictionary> map = fdf.Fields;
            foreach (KeyValuePair<string,PdfDictionary> entry in map) {
                String key = entry.Key;
                PdfDictionary dic = entry.Value;
                PdfObject v = dic.Get(PdfName.V);
                if (v != null) {
                    SetField(key, v);
                }
                v = dic.Get(PdfName.A); // (plaflamme)
                if (v != null) {
            	    SetField(key, v);
                }
            }
        }
        
        /** Sets all the fields from this <CODE>PdfReader</CODE>
        * @param pdf the <CODE>PdfReader</CODE>
        */    
        public void SetFields(PdfReader pdf) {
            SetFields(pdf.AcroFields);
        }
        
        /** Sets all the fields from this <CODE>AcroFields</CODE>
        * @param acro the <CODE>AcroFields</CODE>
        */    
        public void SetFields(AcroFields af) {
            foreach (DictionaryEntry entry in af.Fields) {
                String fn = (String)entry.Key;
                AcroFields.Item item = (AcroFields.Item)entry.Value;
                PdfDictionary dic = item.GetMerged(0);
                PdfObject v = PdfReader.GetPdfObjectRelease(dic.Get(PdfName.V));
                if (v == null)
                    continue;
                PdfObject ft = PdfReader.GetPdfObjectRelease(dic.Get(PdfName.FT));
                if (ft == null || PdfName.SIG.Equals(ft))
                    continue;
                SetField(fn, v);
            }
        }
        
        /** Gets the PDF file name associated with the FDF.
        * @return the PDF file name associated with the FDF
        */
        public String File {
            get {
                return this.file;
            }
            set {
                file = value;
            }
        }
                
        internal class Wrt : PdfWriter {
            private FdfWriter fdf;
           
            internal Wrt(Stream os, FdfWriter fdf) : base(new PdfDocument(), os) {
                this.fdf = fdf;
                this.os.Write(HEADER_FDF, 0, HEADER_FDF.Length);
                body = new PdfBody(this);
            }
            
            internal void WriteTo() {
                PdfDictionary dic = new PdfDictionary();
                dic.Put(PdfName.FIELDS, Calculate(fdf.fields));
                if (fdf.file != null)
                    dic.Put(PdfName.F, new PdfString(fdf.file, PdfObject.TEXT_UNICODE));
                PdfDictionary fd = new PdfDictionary();
                fd.Put(PdfName.FDF, dic);
                PdfIndirectReference refi = AddToBody(fd).IndirectReference;
                byte[] b = GetISOBytes("trailer\n");
                os.Write(b, 0, b.Length);
                PdfDictionary trailer = new PdfDictionary();
                trailer.Put(PdfName.ROOT, refi);
                trailer.ToPdf(null, os);
                b = GetISOBytes("\n%%EOF\n");
                os.Write(b, 0, b.Length);
                os.Close();
            }
            
            
            internal PdfArray Calculate(Dictionary<String, Object> map) {
                PdfArray ar = new PdfArray();
                foreach (KeyValuePair<String, Object> entry in map) {
                    String key = entry.Key;
                    Object v = entry.Value;
                    PdfDictionary dic = new PdfDictionary();
                    dic.Put(PdfName.T, new PdfString(key, PdfObject.TEXT_UNICODE));
                    if (v is Dictionary<String, Object>) {
                        dic.Put(PdfName.KIDS, Calculate((Dictionary<String, Object>)v));
                    }
                    else if (v is PdfAction) {	// (plaflamme)
                        dic.Put(PdfName.A, (PdfAction)v);
                    }
                    else {
                        dic.Put(PdfName.V, (PdfObject)v);
                    }
                    ar.Add(dic);
                }
                return ar;
            }
        }
    }
}
