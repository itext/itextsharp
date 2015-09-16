using System;
using System.IO;
using iTextSharp.text.pdf.crypto;
/*
 * $Id$
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
    public class OutputStreamEncryption : Stream {
    protected Stream outc;
    protected ARCFOUREncryption arcfour;
    protected AESCipher cipher;
    private byte[] buf = new byte[1];
    private const int AES_128 = 4;
    private const int AES_256 = 5;
    private bool aes;
    private bool finished;

        public OutputStreamEncryption(Stream outc, byte[] key, int off, int len, int revision) {
            this.outc = outc;
            aes = (revision == AES_128 || revision == AES_256);
            if (aes) {
                byte[] iv = IVGenerator.GetIV();
                byte[] nkey = new byte[len];
                System.Array.Copy(key, off, nkey, 0, len);
                cipher = new AESCipher(true, nkey, iv);
                Write(iv, 0, iv.Length);
            }
            else {
                arcfour = new ARCFOUREncryption();
                arcfour.PrepareARCFOURKey(key, off, len);
            }
        }  

        public OutputStreamEncryption(Stream outc, byte[] key, int revision) : this(outc, key, 0, key.Length, revision) {
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
                throw new NotSupportedException();
            }
        }
    
        public override long Position {
            get {
                throw new NotSupportedException();
            }
            set {
                throw new NotSupportedException();
            }
        }
    
        public override void Flush() {
            outc.Flush();
        }
    
        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }
    
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }
    
        public override void SetLength(long value) {
            throw new NotSupportedException();
        }
    
        public override void Write(byte[] b, int off, int len) {
            if (aes) {
                byte[] b2 = cipher.Update(b, off, len);
                if (b2 == null || b2.Length == 0)
                    return;
                outc.Write(b2, 0, b2.Length);
            }
            else {
                byte[] b2 = new byte[Math.Min(len, 4192)];
                while (len > 0) {
                    int sz = Math.Min(len, b2.Length);
                    arcfour.EncryptARCFOUR(b, off, sz, b2, 0);
                    outc.Write(b2, 0, sz);
                    len -= sz;
                    off += sz;
                }
            }
        }
    
        public override void Close() {
            Finish();
            outc.Close();
        }
    
        public override void WriteByte(byte value) {
            buf[0] = value;
            Write(buf, 0, 1);
        }

        virtual public void Finish() {
            if (!finished) {
                finished = true;
                if (aes) {
                    byte[] b = cipher.DoFinal();
                    outc.Write(b, 0, b.Length);
                }
            }
        }
    }
}
