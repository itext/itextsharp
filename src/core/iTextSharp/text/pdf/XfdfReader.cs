using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.text.error_messages;
/*
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
    * Reads a XFDF.
    * @author Leonard Rosenthol (leonardr@pdfsages.com)
    */
    public class XfdfReader : ISimpleXMLDocHandler {
        // stuff used during parsing to handle state
        private bool foundRoot = false;
        private readonly Stackr fieldNames = new Stackr();
        private readonly Stackr fieldValues = new Stackr();

        // storage for the field list and their values
        internal Dictionary<string,string>  fields;
        /**
        * Storage for field values if there's more than one value for a field.
        * @since    2.1.4
        */
        protected Dictionary<string,List<string>> listFields;        
        // storage for the path to referenced PDF, if any
        internal String fileSpec;
        
    /** Reads an XFDF form.
        * @param filename the file name of the form
        * @throws IOException on error
        */    
        public XfdfReader(String filename) {
            FileStream fin = null;
            try {
                fin = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SimpleXMLParser.Parse(this, fin);
            }
            finally {
                try{if (fin != null) fin.Close();}catch{}
            }
        }
        
        /** Reads an XFDF form.
        * @param xfdfIn the byte array with the form
        * @throws IOException on error
        */    
        public XfdfReader(byte[] xfdfIn) : this(new MemoryStream(xfdfIn)) {
        }
        
        /**
        * Reads an XFDF form.
        * @param is an InputStream to read the form
        * @throws IOException on error
        * @since 5.0.1
        */    
        public XfdfReader(Stream isp) {
            SimpleXMLParser.Parse(this, isp);
    }
        
    /** Gets all the fields. The map is keyed by the fully qualified
        * field name and the value is a merged <CODE>PdfDictionary</CODE>
        * with the field content.
        * @return all the fields
        */    
        virtual public Dictionary<string,string> Fields {
            get {
                return fields;
            }
        }
        
        /** Gets the field value.
        * @param name the fully qualified field name
        * @return the field's value
        */    
        virtual public String GetField(String name) {
            if (fields.ContainsKey(name))
                return fields[name];
            else
                return null;
        }
        
        /** Gets the field value or <CODE>null</CODE> if the field does not
        * exist or has no value defined.
        * @param name the fully qualified field name
        * @return the field value or <CODE>null</CODE>
        */    
        virtual public String GetFieldValue(String name) {
            return GetField(name);
        }
        
        /**
        * Gets the field values for a list or <CODE>null</CODE> if the field does not
        * exist or has no value defined.
        * @param name the fully qualified field name
        * @return the field values or <CODE>null</CODE>
        * @since   2.1.4
        */    
        virtual public List<string> GetListValues(String name) {
            if (listFields.ContainsKey(name))
                return listFields[name];
            else
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

        /**
        * Called when a start tag is found.
        * @param tag the tag name
        * @param h the tag's attributes
        */    
        virtual public void StartElement(String tag, IDictionary<string,string> h) {
            if ( !foundRoot ) {
                if (!tag.Equals("xfdf"))
                    throw new Exception(MessageLocalization.GetComposedMessage("root.element.is.not.xfdf.1", tag));
                else 
                    foundRoot = true;
            }

            if ( tag.Equals("xfdf") ){
                
            } else if ( tag.Equals("f") ) {
                h.TryGetValue("href", out fileSpec);
            } else if ( tag.Equals("fields") ) {
                fields = new Dictionary<string,string>();     // init it!
                listFields = new Dictionary<string,List<string>>();
            } else if ( tag.Equals("field") ) {
                String  fName;
                h.TryGetValue("name", out fName);
                fieldNames.Push( fName );
            } else if ( tag.Equals("value") ) {
                fieldValues.Push("");
            }
        }
        /**
        * Called when an end tag is found.
        * @param tag the tag name
        */    
        virtual public void EndElement(String tag) {
            if ( tag.Equals("value") ) {
                String  fName = "";
                for (int k = 0; k < fieldNames.Count; ++k) {
                    fName += "." + fieldNames[k];
                }
                if (fName.StartsWith("."))
                    fName = fName.Substring(1);
                String  fVal = fieldValues.Pop();
                String old;
                fields.TryGetValue(fName, out old);
                fields[fName] = fVal;
                if (old != null) {
                    List<string> l;
                    listFields.TryGetValue(fName, out l);
                    if (l == null) {
                        l = new List<string>();
                        l.Add(old);
                    }
                    l.Add(fVal);
                    listFields[fName] = l;
                }
            }
            else if (tag.Equals("field") ) {
                if (fieldNames.Count != 0)
                    fieldNames.Pop();
            }
        }
        
        /**
        * Called when the document starts to be parsed.
        */    
        virtual public void StartDocument()
        {
            fileSpec = "";  // and this too...
        }
        /**
        * Called after the document is parsed.
        */    
        virtual public void EndDocument()
        {
            
        }
        /**
        * Called when a text element is found.
        * @param str the text element, probably a fragment.
        */    
        virtual public void Text(String str)
        {
            if (fieldNames.Count == 0 || fieldValues.Count == 0)
                return;
            
            String val = fieldValues.Pop();
            val += str;
            fieldValues.Push(val);
        }

        internal class Stackr : List<string> {
            internal void Push(string obj) {
                Add(obj);
            }

            internal string Pop() {
                if (Count == 0)
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("the.stack.is.empty"));
                string obj = this[Count - 1];
                RemoveAt(Count - 1);
                return obj;
            }
        }
    }
}
