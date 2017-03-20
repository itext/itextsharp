/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.html;
namespace iTextSharp.tool.xml.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class TagEncounteredState : IState {

        private XMLParser parser;

        /**
         * @param parser the XMLParser
         */
        public TagEncounteredState(XMLParser parser) {
            this.parser = parser;
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.parser.State#process(int)
         */
        virtual public void Process(char character) {
            String tag = this.parser.BufferToString();
            if (HTMLUtils.IsWhiteSpace(character) || character == '>' || character == '/' || character == ':' || tag.Equals("!--") || tag.Equals("![CDATA") && character == '[' || character == '?') {
                // cope with <? xml and <! DOCTYPE
                if (tag.Length > 0) {
                    if (tag.Equals("!--")) {
                        this.parser.Flush();
                        this.parser.Memory().Comment().Length = 0;
                        parser.SelectState().Comment();
                        // if else structure added to check for the presence of an empty comment without a space <!---->
                        // if this check isn't included it would add the third dash to the stringbuilder of XmlParser and
                        // not to memory().comment() which caused CloseCommentState to keep adding the rest of the document
                        // as a comment because it checked the closing tag on the length, which would be 1 in this case
                        // (the fourth dash)
                        if (character != '-') {
                            this.parser.Append(character);
                        } else {
                            this.parser.Memory().Comment().Append(character);
                        }
                    } else if (tag.Equals("![CDATA") && character == '[') {
                        this.parser.Flush();
                        parser.SelectState().Cdata();
                    } else if (tag.Equals("!DOCTYPE")) {
                        this.parser.Flush();
                        parser.SelectState().Doctype();
                        this.parser.Append(character);
                    } else if (HTMLUtils.IsWhiteSpace(character)) {
                        this.parser.Memory().CurrentTag(this.parser.BufferToString());
                        this.parser.Flush();
                        this.parser.SelectState().TagAttributes();
                    } else if (character == '>') {
                        this.parser.Memory().CurrentTag(tag);
                        this.parser.Flush();
                        this.parser.StartElement();
                        this.parser.SelectState().InTag();
                    } else if (character == '/') {
                        this.parser.Memory().CurrentTag(this.parser.BufferToString());
                        this.parser.Flush();
                        this.parser.SelectState().SelfClosing();
                    } else if (character == ':') {
                        this.parser.Memory().Namespace(tag);
                        this.parser.Flush();
                    }
                } else {
                    if (character == '/') {
				        this.parser.SelectState().ClosingTag();
			        } else if (character == '?') {
                        this.parser.Append(character);
                        this.parser.SelectState().ProcessingInstructions();
                    }
                }
            } else {
                this.parser.Append(character);
            }
        }
    }
}
