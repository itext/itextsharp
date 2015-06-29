using System;
using System.IO;
using iTextSharp.text.pdf.intern;

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

namespace iTextSharp.text.pdf {

    /**
     * A <CODE>PdfString</CODE>-class is the PDF-equivalent of a JAVA-<CODE>string</CODE>-object.
     * <P>
     * A string is a sequence of characters delimited by parenthesis. If a string is too long
     * to be conveniently placed on a single line, it may be split across multiple lines by using
     * the backslash character (\) at the end of a line to indicate that the string continues
     * on the following line. Within a string, the backslash character is used as an escape to
     * specify unbalanced parenthesis, non-printing ASCII characters, and the backslash character
     * itself. Use of the \<I>ddd</I> escape sequence is the preferred way to represent characters
     * outside the printable ASCII character set.<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 4.4 (page 37-39).
     *
     * @see        PdfObject
     * @see        BadPdfFormatException
     */

    public class PdfString : PdfObject {
    
        // membervariables
    
        /** The value of this object. */
        protected string value = NOTHING;
        protected string originalValue = null;
    
        /** The encoding. */
        protected string encoding = TEXT_PDFDOCENCODING;
        protected int objNum = 0;
        protected int objGen = 0;
        protected bool hexWriting = false;
        // constructors
    
        /**
         * Constructs an empty <CODE>PdfString</CODE>-object.
         */
    
        public PdfString() : base(STRING) {}
    
        /**
         * Constructs a <CODE>PdfString</CODE>-object.
         *
         * @param        value        the content of the string
         */
    
        public PdfString(string value) : base(STRING) {
            this.value = value;
        }
    
        /**
         * Constructs a <CODE>PdfString</CODE>-object.
         *
         * @param        value        the content of the string
         * @param        encoding    an encoding
         */
    
        public PdfString(string value, string encoding) : base(STRING) {
            this.value = value;
            this.encoding = encoding;
        }
    
        /**
         * Constructs a <CODE>PdfString</CODE>-object.
         *
         * @param        bytes    an array of <CODE>byte</CODE>
         */
    
        public PdfString(byte[] bytes) : base(STRING) {
            value = PdfEncodings.ConvertToString(bytes, null);
            encoding = NOTHING;
        }
    
        // methods overriding some methods in PdfObject
    
        /**
         * Returns the PDF representation of this <CODE>PdfString</CODE>.
         *
         * @return        an array of <CODE>byte</CODE>s
         */
    
        public override void ToPdf(PdfWriter writer, Stream os) {
            PdfWriter.CheckPdfIsoConformance(writer, PdfIsoKeys.PDFISOKEY_OBJECT, this);
            byte[] b = GetBytes();
            PdfEncryption crypto = null;
            if (writer != null)
                crypto = writer.Encryption;
            if (crypto != null && !crypto.IsEmbeddedFilesOnly()) {
                b = crypto.EncryptByteArray(b);
            }
            if (hexWriting) {
                ByteBuffer buf = new ByteBuffer();
                buf.Append('<');
                int len = b.Length;
                for (int k = 0; k < len; ++k)
                    buf.AppendHex(b[k]);
                buf.Append('>');
                os.Write(buf.ToByteArray(), 0, buf.Size);
            }
            else {
                b = StringUtils.EscapeString(b);
                os.Write(b, 0, b.Length);
            }
        }
    
        /**
         * Returns the <CODE>string</CODE> value of the <CODE>PdfString</CODE>-object.
         *
         * @return        a <CODE>string</CODE>
         */
    
        public override string ToString() {
            return value;
        }
    
        // other methods
    
        /**
         * Gets the encoding of this string.
         *
         * @return        a <CODE>string</CODE>
         */
    
        virtual public string Encoding {
            get {
                return encoding;
            }
        }
        virtual public String ToUnicodeString() {
            if (encoding != null && encoding.Length != 0)
                return value;
            GetBytes();
            if (bytes.Length >= 2 && bytes[0] == (byte)254 && bytes[1] == (byte)255)
                return PdfEncodings.ConvertToString(bytes, PdfObject.TEXT_UNICODE);
            else
                return PdfEncodings.ConvertToString(bytes, PdfObject.TEXT_PDFDOCENCODING);
        }
    
        internal void SetObjNum(int objNum, int objGen) {
            this.objNum = objNum;
            this.objGen = objGen;
        }
    
        internal void Decrypt(PdfReader reader) {
            PdfEncryption decrypt = reader.Decrypt;
            if (decrypt != null) {
                originalValue = value;
                decrypt.SetHashKey(objNum, objGen);
                bytes = PdfEncodings.ConvertToBytes(value, null);
                bytes = decrypt.DecryptByteArray(bytes);
                value = PdfEncodings.ConvertToString(bytes, null);
            }
        }
   
        public override byte[] GetBytes() {
            if (bytes == null) {
                if (encoding != null && encoding.Equals(TEXT_UNICODE) && PdfEncodings.IsPdfDocEncoding(value))
                    bytes = PdfEncodings.ConvertToBytes(value, TEXT_PDFDOCENCODING);
                else
                    bytes = PdfEncodings.ConvertToBytes(value, encoding);
            }
            return bytes;
        }
    
        virtual public byte[] GetOriginalBytes() {
            if (originalValue == null)
                return GetBytes();
            return PdfEncodings.ConvertToBytes(originalValue, null);
        }
    
        virtual public PdfString SetHexWriting(bool hexWriting) {
            this.hexWriting = hexWriting;
            return this;
        }
    
        virtual public bool IsHexWriting() {
            return hexWriting;
        }
    }
}
