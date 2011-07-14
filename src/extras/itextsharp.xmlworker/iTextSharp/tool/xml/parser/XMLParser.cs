using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.tool.xml.parser.io;
/*
 * $Id: XMLParser.java 134 2011-05-30 11:27:27Z redlab_b $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2011 1T3XT BVBA
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
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
namespace iTextSharp.tool.xml.parser {

    /**
     * Reads an XML file. Attach a {@link XMLParserListener} for receiving events.
     *
     * @author redlab_b
     *
     */
    public class XMLParser {

        private IState state;
        private StateController controller;
        private IList<IXMLParserListener> listeners;
        private XMLParserMemory memory;
        private IParserMonitor monitor;
        private byte[] text = null;
        private TagState tagState;

        /**
         * Constructs a default XMLParser ready for HTML/XHTML processing.
         */
        public XMLParser() : this(true) {
        }
        /**
         * Constructs a XMLParser.
         * @param isHtml false if this parser is not going to parse HTML and whitespace should be submitted as text too.
         */
        public XMLParser(bool isHtml) {
            this.controller = new StateController(this, isHtml);
            controller.Unknown();
            memory = new XMLParserMemory();
            listeners = new List<IXMLParserListener>();
        }

        /**
         * Construct an HTML XMLParser with the given XMLParserConfig.
         * @param listener the listener
         */
        public XMLParser(IXMLParserListener listener) : this(true) {
            listeners.Add(listener);
        }
        /**
         * Construct an HTML XMLParser with the given XMLParserConfig.
         * @param isHtml false if this parser is not going to parse HTML and whitespace should be submitted as text too.
         * @param listener the listener
         */
        public XMLParser(bool isHtml, IXMLParserListener listener) : this(isHtml) {
            listeners.Add(listener);
        }

        /**
         * If no <code>ParserListener</code> is added, parsing with the parser seems useless no?
         *
         * @param pl the {@link XMLParserListener}
         * @return the parser
         */
        public XMLParser AddListener(IXMLParserListener pl) {
            listeners.Add(pl);
            return this;
        }

        /**
         * Removes a Listener from the list of listeners.
         * @param pl the {@link XMLParserListener} to remove
         * @return the parser
         */
        public XMLParser RemoveListener(IXMLParserListener pl) {
            listeners.Remove(pl);
            return this;
        }

        /**
         * Parse an Stream.
         * @param in the Stream to parse
         * @throws IOException if IO went wrong
         */
        public void Parse(Stream inp) {
            ParseStream(inp);
        }

        /**
         * Parse an Stream.
         * @param inp the Stream to parse
         * @param detectEncoding true if encoding should be detected from the stream
         * @throws IOException if IO went wrong
         */
        public void Parse(Stream inp, bool detectEncoding) {
            if (detectEncoding) {
                Parse(DetectEncoding(inp));
            } else {
                Parse(inp);
            }
        }

        /**
         * Parse an Reader
         * @param reader the reader
         * @throws IOException if IO went wrong
         */
        public void Parse(TextReader reader) {
            foreach (IXMLParserListener l in listeners) {
                l.Init();
            }
            int read = -1;
            TextReader r;
            if (monitor != null) {
                r = new MonitorInputReader(reader, monitor);
            } else {
                r = reader;
            }
            try {
            while (-1 != (read = r.Read())) {
                state.Process(read);
            }
            } finally {
                foreach (IXMLParserListener l in listeners) {
                    l.Close();
                }
                r.Close();
            }
        }
        /**
         * @param r
         * @throws IOException
         */
        private void ParseStream(Stream r) {
            foreach (IXMLParserListener l in listeners) {
                l.Init();
            }
            int read = -1;
            try {
                while (-1 != (read = r.ReadByte())) {
                    state.Process(read);
                }
            } finally {
                foreach (IXMLParserListener l in listeners) {
                    l.Close();
                }
                r.Close();
            }
        }

        /**
         * Detects encoding from a stream.
         *
         * @param inp the stream
         * @return a Reader with the deduced encoding.
         * @throws IOException if IO went wrong
         * @throws UnsupportedEncodingException if unsupported encoding was detected
         */
        public StreamReader DetectEncoding(Stream inp) {
            byte[] b4 = new byte[4];
            int count = inp.Read(b4, 0, b4.Length);
            if (count != 4)
                throw new IOException("Insufficient length");
            String encoding = XMLUtil.GetEncodingName(b4);
            String decl = null;
            if (encoding.Equals("UTF-8")) {
                StringBuilder sb = new StringBuilder();
                int c;
                while ((c = inp.ReadByte()) != -1) {
                    if (c == '>')
                        break;
                    sb.Append((char) c);
                }
                decl = sb.ToString();
            } else if (encoding.Equals("CP037")) {
                MemoryStream bi = new MemoryStream();
                int c;
                while ((c = inp.ReadByte()) != -1) {
                    if (c == 0x6e) // that's '>' in ebcdic
                        break;
                    bi.WriteByte((byte)c);
                }
                decl = Encoding.GetEncoding(37).GetString(bi.ToArray());
            }
            if (decl != null) {
                decl = GetDeclaredEncoding(decl);
                if (decl != null)
                    encoding = decl;
            }
            return new StreamReader(inp, IanaEncodings.GetEncodingEncoding(encoding));
        }

        /**
         * Set the current state.
         *
         * @param state the current state
         */
        protected internal void SetState(IState state) {
            this.state = state;
        }

        /**
         * @param character the int that will be converted to a character.
         * @return the parser
         */
        public XMLParser Append(int character) {
            this.memory.Current().WriteByte((byte)character);
            return this;

        }

        /**
         * @param character the character to append
         * @return the parser
         */
        public XMLParser Append(char character) {
            this.memory.Current().WriteByte((byte)character);
            return this;

        }

    //  /**
    //   * @param str the String to append
    //   * @return the parser
    //   */
    //  public XMLParser Append(String str) {
    //      this.memory.Current().Write(str.GetBytes());
    //      return this;
    //
    //  }

        /**
         * The state controller of the parser
         * @return {@link StateController}
         */
        public StateController SelectState() {
            return this.controller;
        }

        /**
         * Triggered when the UnknownState encountered anything before encountering a tag.
         */
        public void UnknownData() {
            foreach (IXMLParserListener l in listeners) {
                l.UnknownText(this.memory.Current().ToString());
            }
        }

        /**
         * Flushes the currently stored data in the buffer.
         */
        public void Flush() {
            this.memory.ResetBuffer();
        }

        /**
         * Returns the current content of the text buffer.
         * @return current buffer content
         */
        public byte[] Current() {
            return this.memory.Current().ToArray();
        }

        /**
         * Returns the XMLParserMemory.
         *
         * @return the memory
         */
        public XMLParserMemory Memory() {
            return memory;
        }

        /**
         * Triggered when an opening tag has been encountered.
         */
        public void StartElement() {
            CurrentTagState(TagState.OPEN);
            CallText();
            foreach (IXMLParserListener l in listeners) {
                l.StartElement(this.memory.GetCurrentTag(), this.memory.GetAttributes(), this.memory.GetNameSpace());
            }
        }

        /**
         * Call this method to submit the text to listeners.
         */
        private void CallText() {
            if (null != text && text.Length > 0) {
                // LOGGER .Log(text);
                foreach (IXMLParserListener l in listeners) {
                    l.Text(text);
                }
                text = null;
            }
        }

        /**
         * Triggered when a closing tag has been encountered.
         */
        public void EndElement() {
            CurrentTagState(TagState.CLOSE);
            CallText();
            foreach (IXMLParserListener l in listeners) {
                l.EndElement(this.memory.GetCurrentTag(), this.memory.GetNameSpace());
            }
        }

        /**
         * Triggered when content has been encountered.
         *
         * @param bs the content
         */
        public void Text(byte[] bs) {
            text = bs;
        }

        /**
         * Triggered for comments.
         */
        public void Comment() {
            CallText();
            foreach (IXMLParserListener l in listeners) {
                l.Comment(this.memory.Current().ToString());
            }
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
            if (idx1 < 0 && idx2 > 0 || idx2 > 0 && idx2 < idx1) {
                int idx3 = decl.IndexOf('\'', idx2 + 1);
                if (idx3 < 0)
                    return null;
                return decl.Substring(idx2 + 1, idx3 - (idx2 + 1));
            }
            if (idx2 < 0 && idx1 > 0 || idx1 > 0 && idx1 < idx2) {
                int idx3 = decl.IndexOf('"', idx1 + 1);
                if (idx3 < 0)
                    return null;
                return decl.Substring(idx1 + 1, idx3 - (idx1 + 1));
            }
            return null;
        }

        /**
         * @return the current last character of the buffer or ' ' if none.
         */
        public char CurrentLastChar() {
            byte[] current = this.memory.Current().ToArray();
            if (current.Length > 0) {
                return (char)(current.Length -1);
            }
            return ' ';
        }

        /**
         * Get the current tag
         * @return the current tag.
         */
        public String CurrentTag() {
            return this.memory.GetCurrentTag();
        }
        /**
         * Get the state of the current tag
         * @return the state of the current tag
         */
        public TagState CurrentTagState() {
            return this.tagState;
        }

        /**
         *  Set the state of the current tag
         * @param state the state of the current tag
         */
        private void CurrentTagState(TagState state) {
            this.tagState = state;
        }
        /**
         * @param monitor the monitor to set
         */
        public void SetMonitor(IParserMonitor monitor) {
            this.monitor = monitor;
        }
        /**
         * @return the current buffer as a String
         */
        public String BufferToString() {
            return Encoding.ASCII.GetString(this.memory.Current().ToArray());
        }
        /**
         * @param bytes the byte array to append
         * @return this XMLParser
         */
        public XMLParser Append(byte[] bytes) {
            foreach (byte b in bytes) {
                this.memory.Current().WriteByte(b);
            }
            return this;
        }
        /**
         * @return the size of the buffer
         */
        public int BufferSize() {
            return (null != this.memory.Current())?(int)this.memory.Current().Length:0;
        }
    }
}