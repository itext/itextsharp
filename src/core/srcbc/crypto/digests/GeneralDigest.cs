/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;

namespace Org.BouncyCastle.Crypto.Digests
{
    /**
    * base implementation of MD4 family style digest as outlined in
    * "Handbook of Applied Cryptography", pages 344 - 347.
    */
    public abstract class GeneralDigest
		: IDigest
    {
        private const int BYTE_LENGTH = 64;

        private byte[]  xBuf;
        private int     xBufOff;

        private long    byteCount;

        internal GeneralDigest()
        {
            xBuf = new byte[4];
        }

        internal GeneralDigest(GeneralDigest t)
        {
            xBuf = new byte[t.xBuf.Length];
            Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);

            xBufOff = t.xBufOff;
            byteCount = t.byteCount;
        }

        public void Update(byte input)
        {
            xBuf[xBufOff++] = input;

            if (xBufOff == xBuf.Length)
            {
                ProcessWord(xBuf, 0);
                xBufOff = 0;
            }

            byteCount++;
        }

        public void BlockUpdate(
            byte[]  input,
            int     inOff,
            int     length)
        {
            //
            // fill the current word
            //
            while ((xBufOff != 0) && (length > 0))
            {
                Update(input[inOff]);
                inOff++;
                length--;
            }

            //
            // process whole words.
            //
            while (length > xBuf.Length)
            {
                ProcessWord(input, inOff);

                inOff += xBuf.Length;
                length -= xBuf.Length;
                byteCount += xBuf.Length;
            }

            //
            // load in the remainder.
            //
            while (length > 0)
            {
                Update(input[inOff]);

                inOff++;
                length--;
            }
        }

        public void Finish()
        {
            long    bitLength = (byteCount << 3);

            //
            // add the pad bytes.
            //
            Update((byte)128);

            while (xBufOff != 0) Update((byte)0);
            ProcessLength(bitLength);
            ProcessBlock();
        }

        public virtual void Reset()
        {
            byteCount = 0;
            xBufOff = 0;
			Array.Clear(xBuf, 0, xBuf.Length);
        }

		public int GetByteLength()
		{
			return BYTE_LENGTH;
		}

		internal abstract void ProcessWord(byte[] input, int inOff);
        internal abstract void ProcessLength(long bitLength);
        internal abstract void ProcessBlock();
        public abstract string AlgorithmName { get; }
		public abstract int GetDigestSize();
        public abstract int DoFinal(byte[] output, int outOff);
    }
}
