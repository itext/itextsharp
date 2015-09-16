using System;
using System.IO;
using System.Text;
using System.Globalization;
using iTextSharp.text.error_messages;

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
     * Acts like a <CODE>StringBuilder</CODE> but works with <CODE>byte</CODE> arrays.
     * floating point is converted to a format suitable to the PDF.
     * @author Paulo Soares
     */

    public class ByteBuffer : Stream {
        /** The count of bytes in the buffer. */
        protected int count;
    
        /** The buffer where the bytes are stored. */
        protected byte[] buf;
    
        private static int byteCacheSize = 0;
    
        private static byte[][] byteCache = new byte[byteCacheSize][];
        public const byte ZERO = (byte)'0';
        private static char[] chars = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private static byte[] bytes = new byte[] {48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 97, 98, 99, 100, 101, 102};
        /**
        * If <CODE>true</CODE> always output floating point numbers with 6 decimal digits.
        * If <CODE>false</CODE> uses the faster, although less precise, representation.
        */    
        public static bool HIGH_PRECISION = false;
    
        /** Creates new ByteBuffer with capacity 128 */
        public ByteBuffer() : this(128) {}
    
        /**
         * Creates a byte buffer with a certain capacity.
         * @param size the initial capacity
         */
        public ByteBuffer(int size) {
            if (size < 1)
                size = 128;
            buf = new byte[size];
        }
    
        /**
         * Sets the cache size.
         * <P>
         * This can only be used to increment the size.
         * If the size that is passed through is smaller than the current size, nothing happens.
         *
         * @param   size    the size of the cache
         */
    
        public static void SetCacheSize(int size) {
            if (size > 3276700) size = 3276700;
            if (size <= byteCacheSize) return;
            byte[][] tmpCache = new byte[size][];
            System.Array.Copy(byteCache, 0, tmpCache, 0, byteCacheSize);
            byteCache = tmpCache;
            byteCacheSize = size;
        }
    
        /**
         * You can fill the cache in advance if you want to.
         *
         * @param   decimals
         */
    
        public static void FillCache(int decimals) {
            int step = 1;
            switch (decimals) {
                case 0:
                    step = 100;
                    break;
                case 1:
                    step = 10;
                    break;
            }
            for (int i = 1; i < byteCacheSize; i += step) {
                if (byteCache[i] != null) continue;
                byteCache[i] = ConvertToBytes(i);
            }
        }
    
        /**
         * Converts an double (multiplied by 100 and cast to an int) into an array of bytes.
         *
         * @param   i   the int
         * @return  a bytearray
         */
    
        private static byte[] ConvertToBytes(int i) {
            int size = (int)Math.Floor(Math.Log(i) / Math.Log(10));
            if (i % 100 != 0) {
                size += 2;
            }
            if (i % 10 != 0) {
                size++;
            }
            if (i < 100) {
                size++;
                if (i < 10) {
                    size++;
                }
            }
            size--;
            byte[] cache = new byte[size];
            size --;
            if (i < 100) {
                cache[0] = (byte)'0';
            }
            if (i % 10 != 0) {
                cache[size--] = bytes[i % 10];
            }
            if (i % 100 != 0) {
                cache[size--] = bytes[(i / 10) % 10];
                cache[size--] = (byte)'.';
            }
            size = (int)Math.Floor(Math.Log(i) / Math.Log(10)) - 1;
            int add = 0;
            while (add < size) {
                cache[add] = bytes[(i / (int)Math.Pow(10, size - add + 1)) % 10];
                add++;
            }
            return cache;
        }
    
        /**
         * Appends an <CODE>int</CODE>. The size of the array will grow by one.
         * @param b the int to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append_i(int b) {
            int newcount = count + 1;
            if (newcount > buf.Length) {
                byte[] newbuf = new byte[Math.Max(buf.Length << 1, newcount)];
                Array.Copy(buf, 0, newbuf, 0, count);
                buf = newbuf;
            }
            buf[count] = (byte)b;
            count = newcount;
            return this;
        }
    
        /**
         * Appends the subarray of the <CODE>byte</CODE> array. The buffer will grow by
         * <CODE>len</CODE> bytes.
         * @param b the array to be appended
         * @param off the offset to the start of the array
         * @param len the length of bytes to Append
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(byte[] b, int off, int len) {
            if ((off < 0) || (off > b.Length) || (len < 0) ||
                ((off + len) > b.Length) || ((off + len) < 0) || len == 0)
                return this;
            int newcount = count + len;
            if (newcount > buf.Length) {
                byte[] newbuf = new byte[Math.Max(buf.Length << 1, newcount)];
                Array.Copy(buf, 0, newbuf, 0, count);
                buf = newbuf;
            }
            Array.Copy(b, off, buf, count, len);
            count = newcount;
            return this;
        }
    
        /**
         * Appends an array of bytes.
         * @param b the array to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(byte[] b) {
            return Append(b, 0, b.Length);
        }
    
        /**
         * Appends a <CODE>string</CODE> to the buffer. The <CODE>string</CODE> is
         * converted according to the encoding ISO-8859-1.
         * @param str the <CODE>string</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(string str) {
            if (str != null)
                return Append(DocWriter.GetISOBytes(str));
            return this;
        }
    
        /**
         * Appends a <CODE>char</CODE> to the buffer. The <CODE>char</CODE> is
         * converted according to the encoding ISO-8859-1.
         * @param c the <CODE>char</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(char c) {
            return Append_i(c);
        }
    
        /**
         * Appends another <CODE>ByteBuffer</CODE> to this buffer.
         * @param buf the <CODE>ByteBuffer</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(ByteBuffer buf) {
            return Append(buf.buf, 0, buf.count);
        }
    
        /**
         * Appends the string representation of an <CODE>int</CODE>.
         * @param i the <CODE>int</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(int i) {
            return Append((double)i);
        }
    
        /**
         * Appends the string representation of a <CODE>long</CODE>.
         * @param i the <CODE>long</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(long i) {
            return Append(i.ToString(CultureInfo.InvariantCulture));
        }
        
        virtual public ByteBuffer Append(byte b) {
            return Append_i(b);
        }
    
        virtual public ByteBuffer AppendHex(byte b) {
            Append(bytes[(b >> 4) & 0x0f]);
            return Append(bytes[b & 0x0f]);
        }

        /**
         * Appends a string representation of a <CODE>float</CODE> according
         * to the Pdf conventions.
         * @param i the <CODE>float</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(float i) {
            return Append((double)i);
        }
    
        /**
         * Appends a string representation of a <CODE>double</CODE> according
         * to the Pdf conventions.
         * @param d the <CODE>double</CODE> to be appended
         * @return a reference to this <CODE>ByteBuffer</CODE> object
         */
        virtual public ByteBuffer Append(double d) {
            Append(FormatDouble(d, this));
            return this;
        }
    
        /**
         * Outputs a <CODE>double</CODE> into a format suitable for the PDF.
         * @param d a double
         * @return the <CODE>string</CODE> representation of the <CODE>double</CODE>
         */
        public static string FormatDouble(double d) {
            return FormatDouble(d, null);
        }
    
        /**
        * Outputs a <CODE>double</CODE> into a format suitable for the PDF.
        * @param d a double
        * @param buf a ByteBuffer
        * @return the <CODE>String</CODE> representation of the <CODE>double</CODE> if
        * <CODE>buf</CODE> is <CODE>null</CODE>. If <CODE>buf</CODE> is <B>not</B> <CODE>null</CODE>,
        * then the double is appended directly to the buffer and this methods returns <CODE>null</CODE>.
        */
        public static string FormatDouble(double d, ByteBuffer buf) {
            if (HIGH_PRECISION) {
                String sform = d.ToString("0.######", CultureInfo.InvariantCulture);
                if (buf == null)
                    return sform;
                else {
                    buf.Append(sform);
                    return null;
                }
            }
            bool negative = false;
            if (Math.Abs(d) < 0.000015) {
                if (buf != null) {
                    buf.Append(ZERO);
                    return null;
                } else {
                    return "0";
                }
            }
            if (d < 0) {
                negative = true;
                d = -d;
            }
            if (d < 1.0) {
                d += 0.000005;
                if (d >= 1) {
                    if (negative) {
                        if (buf != null) {
                            buf.Append((byte)'-');
                            buf.Append((byte)'1');
                            return null;
                        } else {
                            return "-1";
                        }
                    } else {
                        if (buf != null) {
                            buf.Append((byte)'1');
                            return null;
                        } else {
                            return "1";
                        }
                    }
                }
                if (buf != null) {
                    int v = (int) (d * 100000);
                
                    if (negative) buf.Append((byte)'-');
                    buf.Append((byte)'0');
                    buf.Append((byte)'.');
                
                    buf.Append( (byte)(v / 10000 + ZERO) );
                    if (v % 10000 != 0) {
                        buf.Append( (byte)((v / 1000) % 10 + ZERO) );
                        if (v % 1000 != 0) {
                            buf.Append( (byte)((v / 100) % 10 + ZERO) );
                            if (v % 100 != 0) {
                                buf.Append((byte)((v / 10) % 10 + ZERO) );
                                if (v % 10 != 0) {
                                    buf.Append((byte)((v) % 10 + ZERO) );
                                }
                            }
                        }
                    }
                    return null;
                } else {
                    int x = 100000;
                    int v = (int) (d * x);
                
                    StringBuilder res = new StringBuilder();
                    if (negative) res.Append('-');
                    res.Append("0.");
                
                    while ( v < x/10 ) {
                        res.Append('0');
                        x /= 10;
                    }
                    res.Append(v);
                    int cut = res.Length - 1;
                    while (res[cut] == '0') {
                        --cut;
                    }
                    res.Length = cut + 1;
                    return res.ToString();
                }
            } else if (d <= 32767) {
                d += 0.005;
                int v = (int) (d * 100);
            
                if (v < byteCacheSize && byteCache[v] != null) {
                    if (buf != null) {
                        if (negative) buf.Append((byte)'-');
                        buf.Append(byteCache[v]);
                        return null;
                    } else {
                        string tmp = PdfEncodings.ConvertToString(byteCache[v], null);
                        if (negative) tmp = "-" + tmp;
                        return tmp;
                    }
                }
                if (buf != null) {
                    if (v < byteCacheSize) {
                        //create the cachebyte[]
                        byte[] cache;
                        int size = 0;
                        if (v >= 1000000) {
                            //the original number is >=10000, we need 5 more bytes
                            size += 5;
                        } else if (v >= 100000) {
                            //the original number is >=1000, we need 4 more bytes
                            size += 4;
                        } else if (v >= 10000) {
                            //the original number is >=100, we need 3 more bytes
                            size += 3;
                        } else if (v >= 1000) {
                            //the original number is >=10, we need 2 more bytes
                            size += 2;
                        } else if (v >= 100) {
                            //the original number is >=1, we need 1 more bytes
                            size += 1;
                        }
                    
                        //now we must check if we have a decimal number
                        if (v % 100 != 0) {
                            //yes, do not forget the "."
                            size += 2;
                        }
                        if (v % 10 != 0) {
                            size++;
                        }
                        cache = new byte[size];
                        int add = 0;
                        if (v >= 1000000) {
                            cache[add++] = bytes[(v / 1000000)];
                        }
                        if (v >= 100000) {
                            cache[add++] = bytes[(v / 100000) % 10];
                        }
                        if (v >= 10000) {
                            cache[add++] = bytes[(v / 10000) % 10];
                        }
                        if (v >= 1000) {
                            cache[add++] = bytes[(v / 1000) % 10];
                        }
                        if (v >= 100) {
                            cache[add++] = bytes[(v / 100) % 10];
                        }
                    
                        if (v % 100 != 0) {
                            cache[add++] = (byte)'.';
                            cache[add++] = bytes[(v / 10) % 10];
                            if (v % 10 != 0) {
                                cache[add++] = bytes[v % 10];
                            }
                        }
                        byteCache[v] = cache;
                    }
                
                    if (negative) buf.Append((byte)'-');
                    if (v >= 1000000) {
                        buf.Append( bytes[(v / 1000000)] );
                    }
                    if (v >= 100000) {
                        buf.Append( bytes[(v / 100000) % 10] );
                    }
                    if (v >= 10000) {
                        buf.Append( bytes[(v / 10000) % 10] );
                    }
                    if (v >= 1000) {
                        buf.Append( bytes[(v / 1000) % 10] );
                    }
                    if (v >= 100) {
                        buf.Append( bytes[(v / 100) % 10] );
                    }
                
                    if (v % 100 != 0) {
                        buf.Append((byte)'.');
                        buf.Append( bytes[(v / 10) % 10] );
                        if (v % 10 != 0) {
                            buf.Append( bytes[v % 10] );
                        }
                    }
                    return null;
                } else {
                    StringBuilder res = new StringBuilder();
                    if (negative) res.Append('-');
                    if (v >= 1000000) {
                        res.Append( chars[(v / 1000000)] );
                    }
                    if (v >= 100000) {
                        res.Append( chars[(v / 100000) % 10] );
                    }
                    if (v >= 10000) {
                        res.Append( chars[(v / 10000) % 10] );
                    }
                    if (v >= 1000) {
                        res.Append( chars[(v / 1000) % 10] );
                    }
                    if (v >= 100) {
                        res.Append( chars[(v / 100) % 10] );
                    }
                
                    if (v % 100 != 0) {
                        res.Append('.');
                        res.Append( chars[(v / 10) % 10] );
                        if (v % 10 != 0) {
                            res.Append( chars[v % 10] );
                        }
                    }
                    return res.ToString();
                }
            } else {
                d += 0.5;
                long v = (long) d;
                if (negative) 
                    return "-" + v.ToString(CultureInfo.InvariantCulture);
                else
                    return v.ToString(CultureInfo.InvariantCulture);
            }
        }
    
        /**
         * Sets the size to zero.
         */
        virtual public void Reset() {
            count = 0;
        }
    
        /**
         * Creates a newly allocated byte array. Its size is the current
         * size of this output stream and the valid contents of the buffer
         * have been copied into it.
         *
         * @return  the current contents of this output stream, as a byte array.
         */
        virtual public byte[] ToByteArray() {
            byte[] newbuf = new byte[count];
            Array.Copy(buf, 0, newbuf, 0, count);
            return newbuf;
        }
    
        /**
         * Returns the current size of the buffer.
         *
         * @return the value of the <code>count</code> field, which is the number of valid bytes in this byte buffer.
         */
        virtual public int Size {
            get {
                return count;
            }
            set {
                if (value > count || value < 0)
                    throw new ArgumentOutOfRangeException(MessageLocalization.GetComposedMessage("the.new.size.must.be.positive.and.lt.eq.of.the.current.size"));
                count = value;
            }
        }
    
        /**
         * Converts the buffer's contents into a string, translating bytes into
         * characters according to the platform's default character encoding.
         *
         * @return string translated from the buffer's contents.
         */
        public override string ToString() {
            char[] tmp = this.ConvertToChar(buf);
            return new String(tmp, 0, count);
        }
    
        /**
         * Converts the buffer's contents into a string, translating bytes into
         * characters according to the specified character encoding.
         *
         * @param   enc  a character-encoding name.
         * @return string translated from the buffer's contents.
         * @throws UnsupportedEncodingException
         *         If the named encoding is not supported.
         */
//      public string ToString(System.Text.Encoding enc) {
//          return new String(ref buf, 0, count, enc);
//      }
    
        /**
         * Writes the complete contents of this byte buffer output to
         * the specified output stream argument, as if by calling the output
         * stream's write method using <code>out.Write(buf, 0, count)</code>.
         *
         * @param      out   the output stream to which to write the data.
         * @exception  IOException  if an I/O error occurs.
         */
        virtual public void WriteTo(Stream str) {
            str.Write(buf, 0, count);
        }

        private char[] ConvertToChar(byte[] buf) {
            char[] retVal = new char[count + 1];
            for (int i = 0; i <= count; i++) {
                retVal[i] = (char)buf[i];
            }
            return retVal;
        }

        virtual public byte[] Buffer {
            get {
                return buf;
            }
        }
    
        public override bool CanRead {
            get {
                return false;
            }
        }
    
        public override bool CanSeek {
            get {
                return false;
            }
        }
    
        public override bool CanWrite {
            get {
                return true;
            }
        }
    
        public override long Length {
            get {
                return count;
            }
        }
    
        public override long Position {
            get {
                return count;
            }
            set {
            }
        }
    
        public override void Flush() {
        }
    
        public override int Read(byte[] buffer, int offset, int count) {
            return 0;
        }
    
        public override long Seek(long offset, SeekOrigin origin) {
            return 0;
        }
    
        public override void SetLength(long value) {
        }
    
        public override void Write(byte[] buffer, int offset, int count) {
            Append(buffer, offset, count);
        }
    
        public override void WriteByte(byte value) {
            Append(value);
        }
    }
}
