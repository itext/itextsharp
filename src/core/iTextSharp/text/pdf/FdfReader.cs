using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
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
    /** Reads an FDF form and makes the fields available
    * @author Paulo Soares
    */
    public class FdfReader : PdfReader {
        
        internal Dictionary<String, PdfDictionary> fields;
        internal String fileSpec;
        internal PdfName encoding;
        
        /** Reads an FDF form.
        * @param filename the file name of the form
        * @throws IOException on error
        */    
        public FdfReader(String filename) : base(filename) {
        }
        
        /** Reads an FDF form.
        * @param pdfIn the byte array with the form
        * @throws IOException on error
        */    
        public FdfReader(byte[] pdfIn) : base(pdfIn) {
        }
        
        /** Reads an FDF form.
        * @param url the URL of the document
        * @throws IOException on error
        */    
        public FdfReader(Uri url) : base(url) {
        }
        
        /** Reads an FDF form.
        * @param is the <CODE>InputStream</CODE> containing the document. The stream is read to the
        * end but is not closed
        * @throws IOException on error
        */    
        public FdfReader(Stream isp) : base(isp) {
        }
        
        protected static ICounter COUNTER = CounterFactory.GetCounter(typeof(FdfReader));
	    protected override ICounter GetCounter() {
		    return COUNTER;
    	}

        protected internal override void ReadPdf() {
            fields = new Dictionary<string,PdfDictionary>();
            tokens.CheckFdfHeader();
            RebuildXref();
            ReadDocObj();
            ReadFields();
        }
        
        protected virtual void KidNode(PdfDictionary merged, String name) {
            PdfArray kids = merged.GetAsArray(PdfName.KIDS);
            if (kids == null || kids.Size == 0) {
                if (name.Length > 0)
                    name = name.Substring(1);
                fields[name] = merged;
            }
            else {
                merged.Remove(PdfName.KIDS);
                for (int k = 0; k < kids.Size; ++k) {
                    PdfDictionary dic = new PdfDictionary();
                    dic.Merge(merged);
                    PdfDictionary newDic = kids.GetAsDict(k);
                    PdfString t = newDic.GetAsString(PdfName.T);
                    String newName = name;
                    if (t != null)
                        newName += "." + t.ToUnicodeString();
                    dic.Merge(newDic);
                    dic.Remove(PdfName.T);
                    KidNode(dic, newName);
                }
            }
        }
        
        protected virtual void ReadFields() {
            catalog = trailer.GetAsDict(PdfName.ROOT);
            PdfDictionary fdf = catalog.GetAsDict(PdfName.FDF);
            if (fdf == null)
                return;
            PdfString fs = fdf.GetAsString(PdfName.F);
            if (fs != null)
                fileSpec = fs.ToUnicodeString();
            PdfArray fld = fdf.GetAsArray(PdfName.FIELDS);
            if (fld == null)
                return;
            encoding = fdf.GetAsName(PdfName.ENCODING);
            PdfDictionary merged = new PdfDictionary();
            merged.Put(PdfName.KIDS, fld);
            KidNode(merged, "");
        }

        /** Gets all the fields. The map is keyed by the fully qualified
        * field name and the value is a merged <CODE>PdfDictionary</CODE>
        * with the field content.
        * @return all the fields
        */    
        virtual public Dictionary<String, PdfDictionary> Fields {
            get {
                return fields;
            }
        }
        
        /** Gets the field dictionary.
        * @param name the fully qualified field name
        * @return the field dictionary
        */    
        virtual public PdfDictionary GetField(String name) {
            PdfDictionary dic;
            fields.TryGetValue(name, out dic);
            return dic;;
        }
        
        /**
        * Gets a byte[] containing a file that is embedded in the FDF.
        * @param name the fully qualified field name
        * @return the bytes of the file
        * @throws IOException 
        * @since 5.0.1 
        */
        virtual public byte[] GetAttachedFile(String name) {
            PdfDictionary field = GetField(name);
            if (field != null) {
                PdfIndirectReference ir = (PRIndirectReference)field.Get(PdfName.V);
                PdfDictionary filespec = (PdfDictionary)GetPdfObject(ir.Number);
                PdfDictionary ef = filespec.GetAsDict(PdfName.EF);
                ir = (PRIndirectReference)ef.Get(PdfName.F);
                PRStream stream = (PRStream)GetPdfObject(ir.Number);
                return GetStreamBytes(stream);
            }
            return new byte[0];
        }
        
        /** Gets the field value or <CODE>null</CODE> if the field does not
        * exist or has no value defined.
        * @param name the fully qualified field name
        * @return the field value or <CODE>null</CODE>
        */    
        virtual public String GetFieldValue(String name) {
            PdfDictionary field = GetField(name);
            if (field == null)
                return null;
            PdfObject v = GetPdfObject(field.Get(PdfName.V));
            if (v == null)
                return null;
            if (v.IsName())
                return PdfName.DecodeName(((PdfName)v).ToString());
            else if (v.IsString()) {
                PdfString vs = (PdfString)v;
                if (encoding == null || vs.Encoding != null)
                    return vs.ToUnicodeString();
                byte[] b = vs.GetBytes();
                if (b.Length >= 2 && b[0] == (byte)254 && b[1] == (byte)255)
                    return vs.ToUnicodeString();
                try {
                    if (encoding.Equals(PdfName.SHIFT_JIS))
                        return Encoding.GetEncoding(932).GetString(b);
                    else if (encoding.Equals(PdfName.UHC))
                        return Encoding.GetEncoding(949).GetString(b);
                    else if (encoding.Equals(PdfName.GBK))
                        return Encoding.GetEncoding(936).GetString(b);
                    else if (encoding.Equals(PdfName.BIGFIVE))
                        return Encoding.GetEncoding(950).GetString(b);
                    else if (encoding.Equals(PdfName.UTF_8))
                        return Encoding.UTF8.GetString(b);
                }
                catch  {
                }
                return vs.ToUnicodeString();
            }
            return null;
        }
        
        /** Gets the PDF file specification contained in the FDF.
        * @return the PDF file specification contained in the FDF
        */    
        virtual public String FileSpec {
            get {
                return fileSpec;
            }
        }
    }
}
