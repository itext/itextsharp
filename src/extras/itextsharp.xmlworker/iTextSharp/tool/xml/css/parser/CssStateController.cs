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
using System.Text;
using iTextSharp.tool.xml.css.parser.state;

namespace iTextSharp.tool.xml.css.parser {

    /**
     * IState controller for CSS Processing
     * @author redlab_b
     *
     */
    public class CssStateController {

        private IState current;
        private IState previous;
        private IState commentEnd;
        private IState commentStart;
        private IState commentInside;
        private StringBuilder buffer;
        private IState properties;
        private IState unknown;
        private String currentSelector;
        private IState rule;
        private CssUtils utils;
        private ICssFile css;

        /**
         * @param file the CssFile
         *
         */
        public CssStateController(ICssFile file) {
            this.css = file;
            utils = CssUtils.GetInstance();
            buffer = new StringBuilder();
            commentStart = new CommentStart(this);
            commentEnd = new CommentEnd(this);
            commentInside = new CommentInside(this);
            unknown = new Unknown(this);
            properties = new Properties(this);
            rule = new Rule(this);
            current = unknown;
        }

        /**
         * Appends a character to the buffer.
         * @param c the char to append to the buffer.
         */
        virtual public void Append(char c) {
            this.buffer.Append(c);

        }

        /**
         * Sets the state to the previous state
         */
        virtual public void Previous() {
            this.current = previous;
        }

        /**
         * Processes the character, delegates to states.
         *
         * @param c a character that needs to be processed
         */
        virtual public void Process(char c) {
            current.Process(c);
        }

        private void ProcessProps(String props) {
            String[] split = props.Split(';');
            IDictionary<String, String> map = new Dictionary<String, String>();
            foreach (String prop in split) {
                String[] propSplit = prop.Split(':');
                if (propSplit.Length == 2) {
                    map[utils.StripDoubleSpacesTrimAndToLowerCase(propSplit[0])] = utils.StripDoubleSpacesAndTrim(propSplit[1]);
                }
            }
            if (currentSelector.Contains(",")) {
                String[] selectors = currentSelector.Split(',');
                //check for rules like p, {�}
                for (int i = 0; i < selectors.Length; i++) {
                    selectors[i] = utils.StripDoubleSpacesAndTrim(selectors[i]);
                    if (selectors[i].Length == 0)
                        return;
                }
                foreach (String selector in selectors) {
                    //if any separated selector has errors, all others become invalid.
                    //in this case we just clear map, it is the easies way to support this.
                    if (!css.Add(selector, map))
                        map.Clear();
                }
            } else {
                css.Add(utils.StripDoubleSpacesAndTrim(currentSelector), map);
            }
        }

        private void SetState(IState state) {
            this.current = state;
        }

        private void SetPrevious() {
            this.previous = current;
        }

        /**
         *
         */
        virtual public void StateCommentEnd() {
            SetState(commentEnd);
        }

        /**
         *
         */
        virtual public void StateCommentInside() {
            SetState(commentInside);
        }

        /**
         *
         */
        virtual public void StateCommentStart() {
            SetPrevious();
            SetState(commentStart);
        }

        /**
         *
         */
        virtual public void StateProperties() {
            previous = current;
            SetState(properties);
        }

        /**
         *
         */
        virtual public void StateUnknown() {
            SetState(unknown);
        }

        /**
         *
         */
        virtual public void StateRule() {
            SetState(rule);
        }

        /**
         *
         */
        virtual public void StoreSelector() {
            this.currentSelector = buffer.ToString();
            buffer.Length = 0;
        }

        /**
         *
         */
        virtual public void StoreProperties() {
            ProcessProps(buffer.ToString());
            buffer.Length = 0;
        }
    }
}
