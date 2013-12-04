using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using iTextSharp.text.error_messages;
using iTextSharp.text.html;
using iTextSharp.text.xml.simpleparser.handler;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
 *
 * The code to recognize the encoding in this class and in the convenience class IanaEncodings was taken from Apache Xerces published under the following license:
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Part of this code is based on the Quick-and-Dirty XML parser by Steven Brandt.
 * The code for the Quick-and-Dirty parser was published in JavaWorld (java tip 128).
 * Steven Brandt and JavaWorld gave permission to use the code for free.
 * (Bruno Lowagie and Paulo Soares chose to use it under the MPL/LGPL in
 * conformance with the rest of the code).
 * The original code can be found on this url: <A HREF="http://www.javaworld.com/javatips/jw-javatip128_p.html">http://www.javaworld.com/javatips/jw-javatip128_p.html</A>.
 * It was substantially refactored by Bruno Lowagie.
 * 
 * The method 'private static String getEncodingName(byte[] b4)' was found
 * in org.apache.xerces.impl.XMLEntityManager, originaly published by the
 * Apache Software Foundation under the Apache Software License; now being
 * used in iText under the MPL.
 */

namespace iTextSharp.text.xml.simpleparser {
    /**
    * A simple XML and HTML parser.  This parser is, like the SAX parser,
    * an event based parser, but with much less functionality.
    * <p>
    * The parser can:
    * <p>
    * <ul>
    * <li>It recognizes the encoding used
    * <li>It recognizes all the elements' start tags and end tags
    * <li>It lists attributes, where attribute values can be enclosed in single or double quotes
    * <li>It recognizes the <code>&lt;[CDATA[ ... ]]&gt;</code> construct
    * <li>It recognizes the standard entities: &amp;amp;, &amp;lt;, &amp;gt;, &amp;quot;, and &amp;apos;, as well as numeric entities
    * <li>It maps lines ending in <code>\r\n</code> and <code>\r</code> to <code>\n</code> on input, in accordance with the XML Specification, Section 2.11
    * </ul>
    * <p>
    * The code is based on <A HREF="http://www.javaworld.com/javaworld/javatips/javatip128/">
    * http://www.javaworld.com/javaworld/javatips/javatip128/</A> with some extra
    * code from XERCES to recognize the encoding.
    */
    public sealed class SimpleXMLParser {
        /** possible states */
        private const int UNKNOWN = 0;
        private const int TEXT = 1;
        private const int TAG_ENCOUNTERED = 2;
        private const int EXAMIN_TAG = 3;
        private const int TAG_EXAMINED = 4;
        private const int IN_CLOSETAG = 5;
        private const int SINGLE_TAG = 6;
        private const int CDATA = 7;
        private const int COMMENT = 8;
        private const int PI = 9;
        private const int ENTITY = 10;
        private const int QUOTE = 11;
        private const int ATTRIBUTE_KEY = 12;
        private const int ATTRIBUTE_EQUAL = 13;
        private const int ATTRIBUTE_VALUE = 14;
        
        /** the state stack */
        private Stack<int> stack;
        /** The current character. */
        private int character = 0;
        /** The previous character. */
        private int previousCharacter = -1;
        /** the line we are currently reading */
        private int lines = 1;
        /** the column where the current character occurs */
        private int columns = 0;
        /** was the last character equivalent to a newline? */
        private bool eol = false;
        /**
        * A boolean indicating if the next character should be taken into account
        * if it's a space character. When nospace is false, the previous character
        * wasn't whitespace.
        * @since 2.1.5
        */
        private bool nowhite = false;
        /** the current state */
        private int state;
        /** Are we parsing HTML? */
        private bool html;
        /** current text (whatever is encountered between tags) */
        private StringBuilder text = new StringBuilder();
        /** current entity (whatever is encountered between & and ;) */
        private StringBuilder entity = new StringBuilder();
        /** current tagname */
        private String tag = null;
        /** current attributes */
        private Dictionary<string,string> attributes = null;
        /** The handler to which we are going to forward document content */
        private ISimpleXMLDocHandler doc;
        /** The handler to which we are going to forward comments. */
        private ISimpleXMLDocHandlerComment comment;
        /** Keeps track of the number of tags that are open. */
        private int nested = 0;
        /** the quote character that was used to open the quote. */
        private int quoteCharacter = '"';
        /** the attribute key. */
        private String attributekey = null;
        /** the attribute value. */
        private String attributevalue = null;
        
        private  INewLineHandler newLineHandler;

        /**
        * Creates a Simple XML parser object.
        * Call Go(BufferedReader) immediately after creation.
        */
        private SimpleXMLParser(ISimpleXMLDocHandler doc, ISimpleXMLDocHandlerComment comment, bool html) {
            this.doc = doc;
            this.comment = comment;
            this.html = html;
            if (html) {
                this.newLineHandler = new HTMLNewLineHandler();
            } else {
                this.newLineHandler = new NeverNewLineHandler();
            }
            stack = new Stack<int>();
            state = html ? TEXT : UNKNOWN;
        }
        
        /**
        * Does the actual parsing. Perform this immediately
        * after creating the parser object.
        */
        private void Go(TextReader reader) {
            doc.StartDocument();
            while (true) {
                // read a new character
                if (previousCharacter == -1) {
                    character = reader.Read();
                }
                // or re-examin the previous character
                else {
                    character = previousCharacter;
                    previousCharacter = -1;
                }
                
                // the end of the file was reached
                if (character == -1) {
                    if (html) {
                        if (html && state == TEXT)
                            Flush();
                        doc.EndDocument();
                    } else {
                        ThrowException(MessageLocalization.GetComposedMessage("missing.end.tag"));
                    }
                    return;
                }
                
                // dealing with  \n and \r
                if (character == '\n' && eol) {
                    eol = false;
                    continue;
                } else if (eol) {
                    eol = false;
                } else if (character == '\n') {
                    lines++;
                    columns = 0;
                } else if (character == '\r') {
                    eol = true;
                    character = '\n';
                    lines++;
                    columns = 0;
                } else {
                    columns++;
                }
                
                switch (state) {
                // we are in an unknown state before there's actual content
                case UNKNOWN:
                    if (character == '<') {
                        SaveState(TEXT);
                        state = TAG_ENCOUNTERED;
                    }
                    break;
                // we can encounter any content
                case TEXT:
                    if (character == '<') {
                        Flush();
                        SaveState(state);
                        state = TAG_ENCOUNTERED;
                    } else if (character == '&') {
                        SaveState(state);
                        entity.Length = 0;
                        state = ENTITY;
                        nowhite = true;
                    } else if (character == ' ') {
                        if (html && nowhite) {
                            text.Append(' ');
                            nowhite = false;
                        } else {
                            if (nowhite){
                                text.Append((char)character);
                            }
                            nowhite = false;
                        }
                    } else if (Char.IsWhiteSpace((char)character)) {
                        if (html) {
                            // totally ignore other whitespace
                        } else {
                            if (nowhite){
                                text.Append((char)character);
                            }
                            nowhite = false;
                        }
                    } else {
                        text.Append((char)character);
                        nowhite = true;
                    }
                    break;
                // we have just seen a < and are wondering what we are looking at
                // <foo>, </foo>, <!-- ... --->, etc.
                case TAG_ENCOUNTERED:
                    InitTag();
                    if (character == '/') {
                        state = IN_CLOSETAG;
                    } else if (character == '?') {
                        RestoreState();
                        state = PI;
                    } else {
                        text.Append((char)character);
                        state = EXAMIN_TAG;
                    }
                    break;
                // we are processing something like this <foo ... >.
                // It could still be a <!-- ... --> or something.
                case EXAMIN_TAG:
                    if (character == '>') {
                        DoTag();
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    } else if (character == '/') {
                        state = SINGLE_TAG;
                    } else if (character == '-' && text.ToString().Equals("!-")) {
                        Flush();
                        state = COMMENT;
                    } else if (character == '[' && text.ToString().Equals("![CDATA")) {
                        Flush();
                        state = CDATA;
                    } else if (character == 'E' && text.ToString().Equals("!DOCTYP")) {
                        Flush();
                        state = PI;
                    } else if (char.IsWhiteSpace((char)character)) {
                        DoTag();
                        state = TAG_EXAMINED;
                    } else {
                        text.Append((char)character);
                    }
                    break;
                // we know the name of the tag now.
                case TAG_EXAMINED:
                    if (character == '>') {
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    } else if (character == '/') {
                        state = SINGLE_TAG;
                    } else if (char.IsWhiteSpace((char)character)) {
                        // empty
                    } else {
                        text.Append((char)character);
                        state = ATTRIBUTE_KEY;
                    }
                    break;
                    
                    // we are processing a closing tag: e.g. </foo>
                case IN_CLOSETAG:
                    if (character == '>') {
                        DoTag();
                        ProcessTag(false);
                        if (!html && nested==0) return;
                        state = RestoreState();
                    } else {
                        if (!char.IsWhiteSpace((char)character))
                            text.Append((char)character);
                    }
                    break;
                    
                // we have just seen something like this: <foo a="b"/
                // and are looking for the final >.
                case SINGLE_TAG:
                    if (character != '>')
                        ThrowException(MessageLocalization.GetComposedMessage("expected.gt.for.tag.lt.1.gt", tag));
                    DoTag();
                    ProcessTag(true);
                    ProcessTag(false);
                    InitTag();
                    if (!html && nested==0) {
                        doc.EndDocument();
                        return;
                    }
                    state = RestoreState();
                    break;
                    
                // we are processing CDATA
                case CDATA:
                    if (character == '>'
                    && text.ToString().EndsWith("]]")) {
                        text.Length = text.Length - 2;
                        Flush();
                        state = RestoreState();
                    } else
                        text.Append((char)character);
                    break;
                    
                // we are processing a comment.  We are inside
                // the <!-- .... --> looking for the -->.
                case COMMENT:
                    if (character == '>'
                    && text.ToString().EndsWith("--")) {
                        text.Length = text.Length - 2;
                        Flush();
                        state = RestoreState();
                    } else
                        text.Append((char)character);
                    break;
                    
                // We are inside one of these <? ... ?> or one of these <!DOCTYPE ... >
                case PI:
                    if (character == '>') {
                        state = RestoreState();
                        if (state == TEXT) state = UNKNOWN;
                    }
                    break;
                    
                // we are processing an entity, e.g. &lt;, &#187;, etc.
                case ENTITY:
                    if (character == ';') {
                        state = RestoreState();
                        String cent = entity.ToString();
                        entity.Length = 0;
                        char ce = EntitiesToUnicode.DecodeEntity(cent);
                        if (ce == '\0')
                            text.Append('&').Append(cent).Append(';');
                        else
                            text.Append(ce);
                    } else if ((character != '#' && (character < '0' || character > '9') && (character < 'a' || character > 'z')
                        && (character < 'A' || character > 'Z')) || entity.Length >= 7) {
                        state = RestoreState();
                        previousCharacter = character;
                        text.Append('&').Append(entity.ToString());
                        entity.Length = 0;
                    }
                    else {
                        entity.Append((char)character);
                    }
                    break;
                // We are processing the quoted right-hand side of an element's attribute.
                case QUOTE:
                    if (html && quoteCharacter == ' ' && character == '>') {
                        Flush();
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    }
                    else if (html && quoteCharacter == ' ' && char.IsWhiteSpace((char)character)) {
                        Flush();
                        state = TAG_EXAMINED;
                    }
                    else if (html && quoteCharacter == ' ') {
                        text.Append((char)character);
                    }
                    else if (character == quoteCharacter) {
                        Flush();
                        state = TAG_EXAMINED;
                    } else if (" \r\n\u0009".IndexOf((char)character)>=0) {
                        text.Append(' ');
                    } else if (character == '&') {
                        SaveState(state);
                        state = ENTITY;
                        entity.Length = 0;
                    } else {
                        text.Append((char)character);
                    }
                    break;
                    
                case ATTRIBUTE_KEY:
                    if (char.IsWhiteSpace((char)character)) {
                        Flush();
                        state = ATTRIBUTE_EQUAL;
                    } else if (character == '=') {
                        Flush();
                        state = ATTRIBUTE_VALUE;
                    } else if (html && character == '>') {
                        text.Length = 0;
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    } else {
                        text.Append((char)character);
                    }
                    break;
                    
                case ATTRIBUTE_EQUAL:
                    if (character == '=') {
                        state = ATTRIBUTE_VALUE;
                    } else if (char.IsWhiteSpace((char)character)) {
                        // empty
                    } else if (html && character == '>') {
                        text.Length = 0;
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    } else if (html && character == '/') {
                        Flush();
                        state = SINGLE_TAG;
                    } else if (html) {
                        Flush();
                        text.Append((char)character);
                        state = ATTRIBUTE_KEY;
                    } else {
                        ThrowException(MessageLocalization.GetComposedMessage("error.in.attribute.processing"));
                    }
                    break;
                    
                case ATTRIBUTE_VALUE:
                    if (character == '"' || character == '\'') {
                        quoteCharacter = character;
                        state = QUOTE;
                    } else if (char.IsWhiteSpace((char)character)) {
                        // empty
                    } else if (html && character == '>') {
                        Flush();
                        ProcessTag(true);
                        InitTag();
                        state = RestoreState();
                    } else if (html) {
                        text.Append((char)character);
                        quoteCharacter = ' ';
                        state = QUOTE;
                    } else {
                        ThrowException(MessageLocalization.GetComposedMessage("error.in.attribute.processing"));
                    }
                    break;
                }
            }
        }

        /**
        * Gets a state from the stack
        * @return the previous state
        */
        private int RestoreState() {
            if (stack.Count != 0)
                return stack.Pop();
            else
                return UNKNOWN;
        }
        /**
        * Adds a state to the stack.
        * @param   s   a state to add to the stack
        */
        private void SaveState(int s) {
            stack.Push(s);
        }
        /**
        * Flushes the text that is currently in the buffer.
        * The text can be ignored, added to the document
        * as content or as comment,... depending on the current state.
        */
        private void Flush() {
            switch (state){
            case TEXT:
            case CDATA:
                if (text.Length > 0) {
                    doc.Text(text.ToString());
                }
                break;
            case COMMENT:
                if (comment != null) {
                    comment.Comment(text.ToString());
                }
                break;
            case ATTRIBUTE_KEY:
                attributekey = text.ToString();
                if (html)
                    attributekey = attributekey.ToLower(CultureInfo.InvariantCulture);
                break;
            case QUOTE:
            case ATTRIBUTE_VALUE:
                attributevalue = text.ToString();
                attributes[attributekey] = attributevalue;
                break;
            default:
                // do nothing
                break;
            }
            text.Length = 0;
        }
        /**
        * Initialized the tag name and attributes.
        */
        private void InitTag() {
            tag = null;
            attributes = new Dictionary<string,string>();
        }
        /** Sets the name of the tag. */
        private void DoTag() {
            if (tag == null)
                tag = text.ToString();
            if (html)
                tag = tag.ToLower(CultureInfo.InvariantCulture);
            text.Length = 0;
        }
        /**
        * processes the tag.
        * @param start if true we are dealing with a tag that has just been opened; if false we are closing a tag.
        */
        private void ProcessTag(bool start) {
            if (start) {
                nested++;
                doc.StartElement(tag,attributes);
            }
            else {
                // White spaces following new lines need to be ignored in HTML
                if (newLineHandler.IsNewLineTag(tag)) {
                    nowhite = false;
                }
                nested--;
                doc.EndElement(tag);
            }
        }
        /** Throws an exception */
        private void ThrowException(String s) {
            throw new IOException(MessageLocalization.GetComposedMessage("1.near.line.2.column.3", s, lines, columns));
        }
        
        /**
        * Parses the XML document firing the events to the handler.
        * @param doc the document handler
        * @param r the document. The encoding is already resolved. The reader is not closed
        * @throws IOException on error
        */
        public static void Parse(ISimpleXMLDocHandler doc, ISimpleXMLDocHandlerComment comment, TextReader r, bool html) {
            SimpleXMLParser parser = new SimpleXMLParser(doc, comment, html);
            parser.Go(r);
        }
        
        /**
        * Parses the XML document firing the events to the handler.
        * @param doc the document handler
        * @param in the document. The encoding is deduced from the stream. The stream is not closed
        * @throws IOException on error
        */    
        public static void Parse(ISimpleXMLDocHandler doc, Stream inp) {
            byte[] b4 = new byte[4];
            int count = inp.Read(b4, 0, b4.Length);
            if (count != 4)
                throw new IOException(MessageLocalization.GetComposedMessage("insufficient.length"));
            String encoding = XMLUtil.GetEncodingName(b4);
            String decl = null;
            if (encoding.Equals("UTF-8")) {
                StringBuilder sb = new StringBuilder();
                int c;
                while ((c = inp.ReadByte()) != -1) {
                    if (c == '>')
                        break;
                    sb.Append((char)c);
                }
                decl = sb.ToString();
            }
            else if (encoding.Equals("CP037")) {
                MemoryStream bi = new MemoryStream();
                int c;
                while ((c = inp.ReadByte()) != -1) {
                    if (c == 0x6e) // that's '>' in ebcdic
                        break;
                    bi.WriteByte((byte)c);
                }
                decl = Encoding.GetEncoding(37).GetString(bi.ToArray());//cp037 ebcdic
            }
            if (decl != null) {
                decl = GetDeclaredEncoding(decl);
                if (decl != null)
                    encoding = decl;
            }
            Parse(doc, new StreamReader(inp, IanaEncodings.GetEncodingEncoding(encoding)));
        }
        
        private static String GetDeclaredEncoding(String decl) {
            if (decl == null)
                return null;
            int idx = decl.IndexOf("encoding");
            if (idx < 0)
                return null;
            int idx1 = decl.IndexOf('"', idx);
            int idx2 = decl.IndexOf('\'', idx);
            if (idx1 == idx2)
                return null;
            if ((idx1 < 0 && idx2 > 0) || (idx2 > 0 && idx2 < idx1)) {
                int idx3 = decl.IndexOf('\'', idx2 + 1);
                if (idx3 < 0)
                    return null;
                return decl.Substring(idx2 + 1, idx3 - (idx2 + 1));
            }
            if ((idx2 < 0 && idx1 > 0) || (idx1 > 0 && idx1 < idx2)) {
                int idx3 = decl.IndexOf('"', idx1 + 1);
                if (idx3 < 0)
                    return null;
                return decl.Substring(idx1 + 1, idx3 - (idx1 + 1));
            }
            return null;
        }
        
        public static void Parse(ISimpleXMLDocHandler doc, TextReader r) {
            Parse(doc, null, r, false);
        }
        
        /**
        * Escapes a string with the appropriated XML codes.
        * @param s the string to be escaped
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>
        * @return the escaped string
        */    
        public static String EscapeXML(String s, bool onlyASCII) {
            return XMLUtil.EscapeXML(s, onlyASCII);
        }
    }
}
