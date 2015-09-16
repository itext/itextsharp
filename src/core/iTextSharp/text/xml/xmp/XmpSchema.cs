using System;
using System.Text;
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

namespace iTextSharp.text.xml.xmp {

    /**
    * Abstract superclass of the XmpSchemas supported by iText.
    */
    [Obsolete]
    public abstract class XmpSchema : Properties {
        /** the namesspace */
        protected String xmlns;
        
        /** Constructs an XMP schema. 
        * @param xmlns
        */
        public XmpSchema(String xmlns) : base() {
            this.xmlns = xmlns;
        }
        /**
        * The String representation of the contents.
        * @return a String representation.
        */
        public override String ToString() {
            StringBuilder buf = new StringBuilder();
            foreach (object key in Keys) {
                Process(buf, key);
            }
            return buf.ToString();
        }
        /**
        * Processes a property
        * @param buf
        * @param p
        */
        virtual protected void Process(StringBuilder buf, Object p) {
            buf.Append('<');
            buf.Append(p);
            buf.Append('>');
            buf.Append(this[p.ToString()]);
            buf.Append("</");
            buf.Append(p);
            buf.Append('>');
        }

        /**
        * @return Returns the xmlns.
        */
        virtual public String Xmlns {
            get {
                return xmlns;
            }
        }

        /**
        * @param key
        * @param value
        * @return the previous property (null if there wasn't one)
        */
        virtual public void AddProperty(String key, String value) {
            this[key] = value;
        }
        
        public override string this[string key] {
            set {
                base[key] = XMLUtil.EscapeXML(value, false);
            }
        }
        
        virtual public void SetProperty(string key, XmpArray value) {
            base[key] = value.ToString();
        }
        
        /**
        * @see java.util.Properties#setProperty(java.lang.String, java.lang.String)
        * 
        * @param key
        * @param value
        * @return the previous property (null if there wasn't one)
        */
        virtual public void SetProperty(String key, LangAlt value) {
            base[key] = value.ToString();
        }
        
        /**
        * @param content
        * @return
        */
        public static String Escape(String content) {
            return XMLUtil.EscapeXML(content, false);
        }
    }
}
