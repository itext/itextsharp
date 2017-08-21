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
using System.Collections.Generic;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
namespace iTextSharp.tool.xml.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class InsideTagHTMLState : IState {

        private XMLParser parser;
        private IList<String> noSanitize = new List<String>(1);
        private IList<String> ignoreLastChars = new List<String>(9);
        /**
         * @param parser the XMLParser
         */
        public InsideTagHTMLState(XMLParser parser) {
            this.parser = parser;
            noSanitize.Add(HTML.Tag.PRE);
            ignoreLastChars.Add(HTML.Tag.P);
            ignoreLastChars.Add(HTML.Tag.DIV);
            ignoreLastChars.Add(HTML.Tag.H1);
            ignoreLastChars.Add(HTML.Tag.H2);
            ignoreLastChars.Add(HTML.Tag.H3);
            ignoreLastChars.Add(HTML.Tag.H4);
            ignoreLastChars.Add(HTML.Tag.H5);
            ignoreLastChars.Add(HTML.Tag.H6);
            ignoreLastChars.Add(HTML.Tag.TD);
            ignoreLastChars.Add(HTML.Tag.TH);
            ignoreLastChars.Add(HTML.Tag.UL);
            ignoreLastChars.Add(HTML.Tag.OL);
            ignoreLastChars.Add(HTML.Tag.LI);
            ignoreLastChars.Add(HTML.Tag.DD);
            ignoreLastChars.Add(HTML.Tag.DT);
            ignoreLastChars.Add(HTML.Tag.HR);
            ignoreLastChars.Add(HTML.Tag.BR);
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.parser.State#process(int)
         */
        virtual public void Process(char character) {
            if (character == '<') {
                if (this.parser.BufferSize() > 0) {
                    this.parser.Text(this.parser.Current());
                }
                this.parser.Flush();
                this.parser.SelectState().TagEncountered();
            } else if (character == '&') {
                this.parser.SelectState().SpecialChar();
            } else  {
                if (this.parser.CurrentTag() == HTML.Tag.STYLE && character == '*' && this.parser.Memory().LastChar == '/' && parser.Memory().Current().Length > 0)
                {
                    this.parser.SelectState().StarComment();
                    this.parser.Memory()
                        .Current()
                        .Remove(this.parser.Memory().Current().Length-1, 1);
                    if (this.parser.BufferSize() > 0)
                    {
                        this.parser.Memory().StoredString = this.parser.Current();
                    }
                    this.parser.Flush();
                }
                else
                {
                    String tag = this.parser.CurrentTag();
                    TagState state = this.parser.CurrentTagState();
                    if (noSanitize.Contains(tag) && TagState.OPEN == state)
                    {
                        this.parser.Append(character);
                    }
                    else
                    {
                        if (this.parser.Memory().WhitespaceTag().Length != 0)
                        {
                            if (ignoreLastChars.Contains(this.parser.Memory().WhitespaceTag().ToLower()))
                            {
                                parser.Memory().LastChar = ' ';
                            }
                            this.parser.Memory().WhitespaceTag("");
                        }
                        bool whitespace = HTMLUtils.IsWhiteSpace(parser.Memory().LastChar);
                        bool noWhiteSpace = !HTMLUtils.IsWhiteSpace(character);
                        if (!whitespace || (whitespace && noWhiteSpace))
                        {
                            if (noWhiteSpace)
                            {
                                this.parser.Append(character);
                            }
                            else
                            {
                                this.parser.Append(' ');
                            }
                        }
                        parser.Memory().LastChar = character;
                    }    
                }
                
            }
        }
    }
}
