using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iTextSharp.text.xml.simpleparser;

/*
 * $Id:  $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, Balder Van Camp, et al.
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
namespace iTextSharp.text.xml {

    /**
     * This class converts XML into plain text stripping all tags.
     */
    public class XmlToTxt : ISimpleXMLDocHandler {

        /**
         * Buffer that stores all content that is encountered.
         */
        protected internal StringBuilder buf;

        /**
         * Static method that parses an XML Stream.
         * @param is    the XML input that needs to be parsed
         * @return  a String obtained by removing all tags from the XML
         */
        public static String Parse(Stream isp) {
            XmlToTxt handler = new XmlToTxt();
            SimpleXMLParser.Parse(handler, null, new StreamReader(isp), true);
            return handler.ToString();
        }
        
        /**
         * Creates an instance of XML to TXT.
         */
        protected XmlToTxt() {
            buf = new StringBuilder();
        }
        
        /**
         * @return  the String after parsing.
         */
        public override String ToString() {
            return buf.ToString();
        }
        
        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#startElement(java.lang.String, java.util.Map)
         */
        virtual public void StartElement(String tag, IDictionary<String, String> h) {
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#endElement(java.lang.String)
         */
        virtual public void EndElement(String tag) {
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#startDocument()
         */
        virtual public void StartDocument() {
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#endDocument()
         */
        virtual public void EndDocument() {
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#text(java.lang.String)
         */
        virtual public void Text(String str) {
            buf.Append(str);
        }
    }
}
