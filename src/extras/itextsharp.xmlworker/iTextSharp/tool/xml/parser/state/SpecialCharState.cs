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
using System.Text;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.tool.xml.parser;
namespace iTextSharp.tool.xml.parser.state {

    /**
     * @author redlab_b
     *
     */
    public class SpecialCharState : IState {

        private XMLParser parser;

        /**
         * @param parser the XMLParser
         */
        public SpecialCharState(XMLParser parser) {
            this.parser = parser;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.parser.State#process(int)
         */
        virtual public void Process(char character) {
            StringBuilder entity = this.parser.Memory().CurrentEntity();
            if (character == ';') {
    //          if ("nbsp".Equals(entity.ToString())) {
    //              parser.Append(' '); // TODO check yes or no if it's good idea to transform &nbsp into a space ?
    //          } else {
                    char decoded = parser.IsDecodeSpecialChars() ? EntitiesToUnicode.DecodeEntity(entity.ToString()) : (char)0;
                    if (decoded == '\0') {
                        parser.Append('&').Append(entity.ToString()).Append(';');
                        parser.Memory().LastChar = ';';
                    } else {
                        parser.Append(decoded);
                        parser.Memory().LastChar = decoded;
                    }
    //          }
                parser.SelectState().PreviousState();
                this.parser.Memory().CurrentEntity().Length = 0;
             } else if (character != '#' && (character < '0' || character > '9') && (character < 'a' || character > 'z')
                    && (character < 'A' || character > 'Z') || entity.Length >= 7) {
                 parser.Append('&').Append(entity.ToString()).Append(character);
                 parser.SelectState().PreviousState();
                 this.parser.Memory().CurrentEntity().Length = 0;
            } else {
                entity.Append(character);
            }
        }
    }
}
