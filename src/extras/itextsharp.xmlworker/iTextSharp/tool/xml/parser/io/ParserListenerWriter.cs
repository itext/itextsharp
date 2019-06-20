/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.tool.xml.parser;
namespace iTextSharp.tool.xml.parser.io {

    /**
     * Debugging util.
     * @author redlab_b
     *
     */
    public class ParserListenerWriter : IXMLParserListener {
        /**
         *
         */
        private IAppender writer;
        private bool formatted;

        /**
         * @param writer the appender
         * @param formatted true if output should be formatted
         */
        public ParserListenerWriter(IAppender writer, bool formatted) {
            this.writer = writer;
            this.formatted = formatted;
        }
        /**
         * Construct a new ParserListenerWriter with the given appender and default formatted to true;
         * @param writer the appender
         */
        public ParserListenerWriter(IAppender writer) : this(writer, true) {
        }

        virtual public void UnknownText(String str) {
        }

        virtual public void Text(string text) {
            writer.Append(text);
        }

        virtual public void StartElement(String currentTag, IDictionary<String, String> attributes, String ns) {
            String myns = (ns.Length > 0)?ns+":":ns;
            if ( attributes.Count >0) {
                writer.Append("<").Append(myns ).Append(currentTag).Append(" ");
                foreach (KeyValuePair<String,String> e in attributes) {
                    writer.Append(e.Key).Append("=\"").Append(e.Value).Append("\" ");
                }
                writer.Append('>');
            } else {
                writer.Append('<').Append(myns).Append(currentTag).Append('>');
            }
        }

        virtual public void EndElement(String curentTag, String ns) {
            String myns = (ns.Length > 0)?ns+":":ns;
            writer.Append("</").Append(myns).Append(curentTag).Append('>');
            if (formatted) {
                writer.Append(Environment.NewLine);
            }
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.parser.ParserListener#comment(java.lang.String)
         */
        virtual public void Comment(String comment) {
            writer.Append("<!--").Append(comment).Append("-->");
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.parser.XMLParserListener#init()
         */
        virtual public void Init() {
        }
        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.parser.XMLParserListener#close()
         */
        virtual public void Close() {
        }
    }
}
