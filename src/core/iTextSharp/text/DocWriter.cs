using System;
using System.IO;
using System.Collections;
using System.util;
using iTextSharp.text.pdf;
/*
 * $Id$
 * 
 *
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

namespace iTextSharp.text {
    /// <summary>
    /// An abstract Writer class for documents.
    /// </summary>
    /// <remarks>
    /// DocWriter is the abstract class of several writers such
    /// as PdfWriter and HtmlWriter.
    /// A DocWriter can be added as a DocListener
    /// to a certain Document by getting an instance (see method
    /// GetInstance() in the specific writer-classes).
    /// Every Element added to the original Document
    /// will be written to the stream of the listening
    /// DocWriter.
    /// </remarks>
    /// <seealso cref="T:iTextSharp.text.Document"/>
    /// <seealso cref="T:iTextSharp.text.IDocListener"/>
    public abstract class DocWriter : IDocListener {

        /// <summary> This is some byte that is often used. </summary>
        public const byte NEWLINE = (byte)'\n';

        /// <summary> This is some byte that is often used. </summary>
        public const byte TAB = (byte)'\t';

        /// <summary> This is some byte that is often used. </summary>
        public const byte LT = (byte)'<';

        /// <summary> This is some byte that is often used. </summary>
        public const byte SPACE = (byte)' ';

        /// <summary> This is some byte that is often used. </summary>
        public const byte EQUALS = (byte)'=';

        /// <summary> This is some byte that is often used. </summary>
        public const byte QUOTE = (byte)'\"';

        /// <summary> This is some byte that is often used. </summary>
        public const byte GT = (byte)'>';

        /// <summary> This is some byte that is often used. </summary>
        public const byte FORWARD = (byte)'/';

        // membervariables

        /// <summary> The pageSize. </summary>
        protected Rectangle pageSize;

        /// <summary> This is the document that has to be written. </summary>
        protected Document document;

        /// <summary> The stream of this writer. </summary>
        protected OutputStreamCounter os;

        /// <summary> Is the writer open for writing? </summary>
        protected bool open = false;

        /// <summary> Do we have to pause all writing actions? </summary>
        protected bool pause = false;

        /** Closes the stream on document close */
        protected bool closeStream = true;

        // constructor
    
        protected DocWriter() {
        }
        /// <summary>
        /// Constructs a DocWriter.
        /// </summary>
        /// <param name="document">The Document that has to be written</param>
        /// <param name="os">The Stream the writer has to write to.</param>
        protected DocWriter(Document document, Stream os)  
        {
            this.document = document;
            this.os = new OutputStreamCounter(os);
        }

        // implementation of the DocListener methods

        /// <summary>
        /// Signals that an Element was added to the Document.
        /// </summary>
        /// <remarks>
        /// This method should be overriden in the specific DocWriter classes
        /// derived from this abstract class.
        /// </remarks>
        /// <param name="element"></param>
        /// <returns>false</returns>
        public virtual bool Add(IElement element) {
            return false;
        }

        /// <summary>
        /// Signals that the Document was opened.
        /// </summary>
        public virtual void Open() {
            open = true;
        }

        /// <summary>
        /// Sets the pagesize.
        /// </summary>
        /// <param name="pageSize">the new pagesize</param>
        /// <returns>a boolean</returns>
        public virtual bool SetPageSize(Rectangle pageSize) {
            this.pageSize = pageSize;
            return true;
        }

        /// <summary>
        /// Sets the margins.
        /// </summary>
        /// <remarks>
        /// This does nothing. Has to be overridden if needed.
        /// </remarks>
        /// <param name="marginLeft">the margin on the left</param>
        /// <param name="marginRight">the margin on the right</param>
        /// <param name="marginTop">the margin on the top</param>
        /// <param name="marginBottom">the margin on the bottom</param>
        /// <returns></returns>
        public virtual bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom) {
            return false;
        }

        /// <summary>
        /// Signals that an new page has to be started.
        /// </summary>
        /// <remarks>
        /// This does nothing. Has to be overridden if needed.
        /// </remarks>
        /// <returns>true if the page was added, false if not.</returns>
        public virtual bool NewPage() {
            if (!open) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets the page number to 0.
        /// </summary>
        /// <remarks>
        /// This method should be overriden in the specific DocWriter classes
        /// derived from this abstract class if they actually support the use of
        /// pagenumbers.
        /// </remarks>
        public virtual void ResetPageCount() {
        }

        /// <summary>
        /// Sets the page number.
        /// </summary>
        /// <remarks>
        /// This method should be overriden in the specific DocWriter classes
        /// derived from this abstract class if they actually support the use of
        /// pagenumbers.
        /// </remarks>
        public virtual int PageCount {
            set {}
        }

        /// <summary>
        /// Signals that the Document was closed and that no other
        /// Elements will be added.
        /// </summary>
        public virtual void Close() {
            open = false;
            os.Flush();
            if (closeStream)
                os.Close();
        }

        // methods

        /// <summary>
        /// Converts a string into a Byte array
        /// according to the ISO-8859-1 codepage.
        /// </summary>
        /// <param name="text">the text to be converted</param>
        /// <returns>the conversion result</returns>
        public static byte[] GetISOBytes(string text) {
            if (text == null)
                return null;
            int len = text.Length;
            byte[] b = new byte[len];
            for (int k = 0; k < len; ++k)
                b[k] = (byte)text[k];
            return b;
        }

        /// <summary>
        /// Let the writer know that all writing has to be paused.
        /// </summary>
        public virtual void Pause() {
            pause = true;
        }

        /**
        * Checks if writing is paused.
        *
        * @return       <CODE>true</CODE> if writing temporarely has to be paused, <CODE>false</CODE> otherwise.
        */
        
        virtual public bool IsPaused() {
            return pause;
        }

        /// <summary>
        /// Let the writer know that writing may be resumed.
        /// </summary>
        public virtual void Resume() {
            pause = false;
        }

        /// <summary>
        /// Flushes the Stream.
        /// </summary>
        public virtual void Flush() {
            os.Flush();
        }

        /// <summary>
        /// Writes a string to the stream.
        /// </summary>
        /// <param name="str">the string to write</param>
        virtual protected void Write(string str) {
            byte[] tmp = GetISOBytes(str);
            os.Write(tmp, 0, tmp.Length);
        }

        /// <summary>
        /// Writes a number of tabs.
        /// </summary>
        /// <param name="indent">the number of tabs to add</param>
        virtual protected void AddTabs(int indent) {
            os.WriteByte(NEWLINE);
            for (int i = 0; i < indent; i++) {
                os.WriteByte(TAB);
            }
        }

        /// <summary>
        /// Writes a key-value pair to the stream.
        /// </summary>
        /// <param name="key">the name of an attribute</param>
        /// <param name="value">the value of an attribute</param>
        virtual protected void Write(string key, string value) {
            os.WriteByte(SPACE);
            Write(key);
            os.WriteByte(EQUALS);
            os.WriteByte(QUOTE);
            Write(value);
            os.WriteByte(QUOTE);
        }

        /// <summary>
        /// Writes a starttag to the stream.
        /// </summary>
        /// <param name="tag">the name of the tag</param>
        virtual protected void WriteStart(string tag) {
            os.WriteByte(LT);
            Write(tag);
        }

        /// <summary>
        /// Writes an endtag to the stream.
        /// </summary>
        /// <param name="tag">the name of the tag</param>
        virtual protected void WriteEnd(string tag) {
            os.WriteByte(LT);
            os.WriteByte(FORWARD);
            Write(tag);
            os.WriteByte(GT);
        }

        /// <summary>
        /// Writes an endtag to the stream.
        /// </summary>
        virtual protected void WriteEnd() {
            os.WriteByte(SPACE);
            os.WriteByte(FORWARD);
            os.WriteByte(GT);
        }

        /// <summary>
        /// Writes the markup attributes of the specified MarkupAttributes
        /// object to the stream.
        /// </summary>
        /// <param name="mAtt">the MarkupAttributes to write.</param>
        /// <returns></returns>
        virtual protected bool WriteMarkupAttributes(Properties markup) {
            if (markup == null) return false;
            foreach (String name in markup.Keys) {
                Write(name, markup[name]);
            }
            markup.Clear();
            return true;
        }

        public virtual bool CloseStream {
            get {
                return closeStream;
            }
            set {
                closeStream = value;
            }
        }

        public virtual bool SetMarginMirroring(bool marginMirroring) {
            return false;
        }

        /**
         * @see com.lowagie.text.DocListener#setMarginMirroring(boolean)
         * @since	2.1.6
         */
        public virtual bool SetMarginMirroringTopBottom(bool MarginMirroring) {
            return false;
        }

        public virtual void Dispose() {
            Close();
        }
    }
}
