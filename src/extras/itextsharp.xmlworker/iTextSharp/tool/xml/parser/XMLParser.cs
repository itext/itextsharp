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
using System.IO;
using System.Text;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.tool.xml.parser.io;
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
        private string text = null;
        private TagState tagState;
        private Encoding charset;
        private bool decodeSpecialChars = true;

        /**
         * Constructs a default XMLParser ready for HTML/XHTML processing.
         */
        public XMLParser() : this(true, Encoding.Default) {
        }

        /**
         * Constructs a XMLParser.
         *
         * @param isHtml false if this parser is not going to parse HTML and
         *            whitespace should be submitted as text too.
         * @param charset charset
         */
        public XMLParser(bool isHtml, Encoding charset) {
            this.charset = charset;
            this.controller = new StateController(this, isHtml);
            controller.Unknown();
            memory = new XMLParserMemory(isHtml);
            listeners = new List<IXMLParserListener>();
        }

        /**
         * Construct an XMLParser with the given XMLParserConfig ready for
         * HTML/XHTML processing..
         *
         * @param listener the listener
         * @param charset the Charset
         */
        public XMLParser(IXMLParserListener listener, Encoding charset) : this(true, charset) {
            listeners.Add(listener);
        }

        /**
         * Constructs a new Parser with the default jvm charset.
         * @param b true if HTML is being parsed
         * @param listener the XMLParserListener
         */
        public XMLParser(bool b, IXMLParserListener listener) : this(b, Encoding.Default) {
            listeners.Add(listener);
        }

        /**
         * Constructs a new Parser with HTML parsing set to true and the default jvm charset.
         * @param listener the XMLParserListener
         */
        public XMLParser(IXMLParserListener listener) : this(true, Encoding.Default){
            listeners.Add(listener);
        }

        /**
         * Construct a XMLParser with the given XMLParserConfig.
         *
         * @param isHtml false if this parser is not going to parse HTML and
         *            whitespace should be submitted as text too.
         * @param listener the listener
         * @param charset the Charset to use
         */
        public XMLParser(bool isHtml, IXMLParserListener listener, Encoding charset) : this(isHtml, charset) {
            listeners.Add(listener);
        }

        /**
         * If no <code>ParserListener</code> is added, parsing with the parser seems useless no?
         *
         * @param pl the {@link XMLParserListener}
         * @return the parser
         */
        virtual public XMLParser AddListener(IXMLParserListener pl) {
            listeners.Add(pl);
            return this;
        }

        /**
         * Removes a Listener from the list of listeners.
         * @param pl the {@link XMLParserListener} to remove
         * @return the parser
         */
        virtual public XMLParser RemoveListener(IXMLParserListener pl) {
            listeners.Remove(pl);
            return this;
        }

        /**
         * Parse an Stream.
         * @param in the Stream to parse
         * @throws IOException if IO went wrong
         */
        virtual public void Parse(Stream inp) {
            Parse(new StreamReader(inp));
        }

        /**
         * Parse an Stream.
         * @param inp the Stream to parse
         * @param detectEncoding true if encoding should be detected from the stream
         * @throws IOException if IO went wrong
         */
        virtual public void Parse(Stream inp, bool detectEncoding) {
            if (detectEncoding) {
                Parse(DetectEncoding(inp));
            } else {
                Parse(inp);
            }
        }

	    /**
	     * Parses an InputStream using the given encoding
	     * @param in the stream to read
	     * @param charSet to use for the constructed reader.
	     * @throws IOException if reading fails
	     */
	    virtual public void Parse(Stream inp, Encoding charSet) {
		    this.charset = charSet;
		    StreamReader reader = new StreamReader(inp, charSet);
		    Parse(reader);
	    }

        /**
         * Parse an Reader
         *
         * @param reader the reader
         * @throws IOException if IO went wrong
         */
        virtual public void Parse(TextReader reader) {
            ParseWithReader(reader);
        }

        /**
         * The actual parse method
         * @param r
         * @throws IOException
         */
        private void ParseWithReader(TextReader reader) {
            foreach (IXMLParserListener l in listeners) {
                l.Init();
            }
            TextReader r;
            if (monitor != null) {
                r = new MonitorInputReader(reader, monitor);
            } else {
                r = reader;
            }
            char[] read = new char[1];
            try {
                while (1 == (r.Read(read, 0, 1))) {
                    state.Process(read[0]);
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
        virtual public StreamReader DetectEncoding(Stream inp) {
            byte[] b4 = new byte[4];
            int count = inp.Read(b4, 0, b4.Length);
            if (count != 4)
                throw new IOException("Insufficient length");
            String encoding = XMLUtil.GetEncodingName(b4);
            String decl = null;
            int bytesNumber = 0; 
            if (encoding.Equals("UTF-8")) {
                StringBuilder sb = new StringBuilder();
                int c;
                while (bytesNumber < 1028 && ((c = inp.ReadByte()) != -1)) {
                    if (c == '>')
                        break;
                    sb.Append((char) c);
                    bytesNumber++;
                }
                decl = sb.ToString();
            } else if (encoding.Equals("CP037")) {
                MemoryStream bi = new MemoryStream();
                int c;
                while (bytesNumber < 1028 && ((c = inp.ReadByte()) != -1)) {
                    if (c == 0x6e) // that's '>' in ebcdic
                        break;
                    bi.WriteByte((byte)c);
                    bytesNumber++;
                }
                decl = Encoding.GetEncoding(37).GetString(bi.ToArray());
            }
            if (decl != null) {
                decl = EncodingUtil.GetDeclaredEncoding(decl);
                if (decl != null)
                    encoding = decl;
            }
            if (inp.CanSeek)
                inp.Seek(0, SeekOrigin.Begin);
            return new StreamReader(inp, IanaEncodings.GetEncodingEncoding(encoding));
        }

        /**
         * Set the current state.
         *
         * @param state the current state
         */
        virtual protected internal void SetState(IState state) {
            this.state = state;
        }

        /**
         * @param character the character to append
         * @return the parser
         */
        virtual public XMLParser Append(char character) {
            this.memory.Current().Append(character);
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
        virtual public StateController SelectState() {
            return this.controller;
        }

        /**
         * Triggered when the UnknownState encountered anything before encountering a tag.
         */
        virtual public void UnknownData() {
            foreach (IXMLParserListener l in listeners) {
                l.UnknownText(this.memory.Current().ToString());
            }
        }

        /**
         * Flushes the currently stored data in the buffer.
         */
        virtual public void Flush() {
            this.memory.ResetBuffer();
        }

        /**
         * Returns the current content of the text buffer.
         * @return current buffer content
         */
        virtual public string Current() {
            return this.memory.Current().ToString();
        }

        /**
         * Returns the XMLParserMemory.
         *
         * @return the memory
         */
        virtual public XMLParserMemory Memory() {
            return memory;
        }

        /**
         * Triggered when an opening tag has been encountered.
         */
        virtual public void StartElement() {
            CurrentTagState(TagState.OPEN);
            String tagName = this.memory.GetCurrentTag();
            IDictionary<String, String> attributes = this.memory.GetAttributes();
            if (tagName.StartsWith("?")) {
                Memory().ProcessingInstruction().Length = 0;
            }
            CallText();
            foreach (IXMLParserListener l in listeners) {
                l.StartElement(tagName, attributes, this.memory.GetNameSpace());
            }
            this.memory.FlushNameSpace();
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
        virtual public void EndElement() {
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
        virtual public void Text(string bs) {
            text = bs;
        }

        /**
         * Triggered for comments.
         */
        virtual public void Comment() {
            CallText();
            foreach (IXMLParserListener l in listeners) {
                l.Comment(this.memory.Current().ToString());
            }
        }

        /**
         * @return the current last character of the buffer or ' ' if none.
         */
        virtual public char CurrentLastChar() {
            StringBuilder sb = this.memory.Current();
            if (sb.Length == 0)
                return ' ';
            else
                return sb[sb.Length - 1];
        }

        /**
         * Get the current tag
         * @return the current tag.
         */
        virtual public String CurrentTag() {
            return this.memory.GetCurrentTag();
        }
        /**
         * Get the state of the current tag
         * @return the state of the current tag
         */
        virtual public TagState CurrentTagState() {
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
        virtual public void SetMonitor(IParserMonitor monitor) {
            this.monitor = monitor;
        }

        /**
         * Determines whether special chars like &gt; will be decoded
         * @param decodeSpecialChars true to decode, false to not decode
         */
        virtual public void SetDecodeSpecialChars(bool decodeSpecialChars) {
            this.decodeSpecialChars = decodeSpecialChars;
        }

        virtual public bool IsDecodeSpecialChars() {
            return decodeSpecialChars;
        }
        
        /**
         * @return the current buffer as a String
         */
        virtual public String BufferToString() {
            return this.memory.Current().ToString();
        }

        /**
         * @param bytes the byte array to append
         * @return this instance of the XMLParser
         */
        virtual public XMLParser Append(char[] bytes) {
            this.memory.Current().Append(bytes);
            return this;
        }

        /**
         * @return the size of the buffer
         */
        virtual public int BufferSize() {
            return (null != this.memory.Current())?this.memory.Current().Length:0;
        }

        /**
         * Appends the given string to the buffer.
         * @param string the String to append
         * @return this instance of the XMLParser
         */
        virtual public XMLParser Append(String str) {
            this.memory.Current().Append(str);
            return this;

        }
        /**
         * Returns the current used character set.
         * @return the charset
         */
        virtual public Encoding Charset {
            get {
                return charset;
            }
        }
    }
}
