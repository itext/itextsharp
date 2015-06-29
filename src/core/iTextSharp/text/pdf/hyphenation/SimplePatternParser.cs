using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.util;
using iTextSharp.text.xml.simpleparser;
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

namespace iTextSharp.text.pdf.hyphenation {
    /** Parses the xml hyphenation pattern.
    *
    * @author Paulo Soares
    */
    public class SimplePatternParser : ISimpleXMLDocHandler {
        internal int currElement;
        internal IPatternConsumer consumer;
        internal StringBuilder token;
        internal List<object> exception;
        internal char hyphenChar;
        
        internal const int ELEM_CLASSES = 1;
        internal const int ELEM_EXCEPTIONS = 2;
        internal const int ELEM_PATTERNS = 3;
        internal const int ELEM_HYPHEN = 4;

        /** Creates a new instance of PatternParser2 */
        public SimplePatternParser() {
            token = new StringBuilder();
            hyphenChar = '-';    // default
        }
        
        virtual public void Parse(Stream stream, IPatternConsumer consumer) {
            this.consumer = consumer;
            try {
                SimpleXMLParser.Parse(this, stream);
            }
            finally {
                try{stream.Close();}catch{}
            }
        }
        
        protected static String GetPattern(String word) {
            StringBuilder pat = new StringBuilder();
            int len = word.Length;
            for (int i = 0; i < len; i++) {
                if (!char.IsDigit(word[i])) {
                    pat.Append(word[i]);
                }
            }
            return pat.ToString();
        }

        virtual protected List<object> NormalizeException(List<object> ex) {
            List<object> res = new List<object>();
            for (int i = 0; i < ex.Count; i++) {
                Object item = ex[i];
                if (item is String) {
                    String str = (String)item;
                    StringBuilder buf = new StringBuilder();
                    for (int j = 0; j < str.Length; j++) {
                        char c = str[j];
                        if (c != hyphenChar) {
                            buf.Append(c);
                        } else {
                            res.Add(buf.ToString());
                            buf.Length = 0;
                            char[] h = new char[1];
                            h[0] = hyphenChar;
                            // we use here hyphenChar which is not necessarily
                            // the one to be printed
                            res.Add(new Hyphen(new String(h), null, null));
                        }
                    }
                    if (buf.Length > 0) {
                        res.Add(buf.ToString());
                    }
                } else {
                    res.Add(item);
                }
            }
            return res;
        }

        virtual protected String GetExceptionWord(List<object> ex) {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < ex.Count; i++) {
                Object item = ex[i];
                if (item is String) {
                    res.Append((String)item);
                } else {
                    if (((Hyphen)item).noBreak != null) {
                        res.Append(((Hyphen)item).noBreak);
                    }
                }
            }
            return res.ToString();
        }

        protected static String GetInterletterValues(String pat) {
            StringBuilder il = new StringBuilder();
            String word = pat + "a";    // add dummy letter to serve as sentinel
            int len = word.Length;
            for (int i = 0; i < len; i++) {
                char c = word[i];
                if (char.IsDigit(c)) {
                    il.Append(c);
                    i++;
                } else {
                    il.Append('0');
                }
            }
            return il.ToString();
        }

        virtual public void EndDocument() {
        }
        
        virtual public void EndElement(String tag) {
            if (token.Length > 0) {
                String word = token.ToString();
                switch (currElement) {
                case ELEM_CLASSES:
                    consumer.AddClass(word);
                    break;
                case ELEM_EXCEPTIONS:
                    exception.Add(word);
                    exception = NormalizeException(exception);
                    consumer.AddException(GetExceptionWord(exception), new List<object>(exception));
                    break;
                case ELEM_PATTERNS:
                    consumer.AddPattern(GetPattern(word),
                                        GetInterletterValues(word));
                    break;
                case ELEM_HYPHEN:
                    // nothing to do
                    break;
                }
                if (currElement != ELEM_HYPHEN) {
                    token.Length = 0;
                }
            }
            if (currElement == ELEM_HYPHEN) {
                currElement = ELEM_EXCEPTIONS;
            } else {
                currElement = 0;
            }
        }
        
        virtual public void StartDocument() {
        }
        
        virtual public void StartElement(String tag, IDictionary<string,string> h) {
            if (tag.Equals("hyphen-char")) {
                String hh;
                h.TryGetValue("value", out hh);
                if (hh != null && hh.Length == 1) {
                    hyphenChar = hh[0];
                }
            } else if (tag.Equals("classes")) {
                currElement = ELEM_CLASSES;
            } else if (tag.Equals("patterns")) {
                currElement = ELEM_PATTERNS;
            } else if (tag.Equals("exceptions")) {
                currElement = ELEM_EXCEPTIONS;
                exception = new List<object>();
            } else if (tag.Equals("hyphen")) {
                if (token.Length > 0) {
                    exception.Add(token.ToString());
                }
                string pre;
                h.TryGetValue("pre", out pre);
                string no;
                h.TryGetValue("no", out no);
                string post;
                h.TryGetValue("post", out post);
                exception.Add(new Hyphen(pre, no, post));
                currElement = ELEM_HYPHEN;
            }
            token.Length = 0;
        }
        
        virtual public void Text(String str) {
            StringTokenizer tk = new StringTokenizer(str);
            while (tk.HasMoreTokens()) {
                String word = tk.NextToken();
                // System.out.Println("\"" + word + "\"");
                switch (currElement) {
                case ELEM_CLASSES:
                    consumer.AddClass(word);
                    break;
                case ELEM_EXCEPTIONS:
                    exception.Add(word);
                    exception = NormalizeException(exception);
                    consumer.AddException(GetExceptionWord(exception), new List<object>(exception));
                    exception.Clear();
                    break;
                case ELEM_PATTERNS:
                    consumer.AddPattern(GetPattern(word),
                                        GetInterletterValues(word));
                    break;
                }
            }
        }
    }
}
