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

using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Digests
{
    /**
     * FIPS 180-4 implementation of SHA-512/t
     */
    public class Sha512tDigest
        : LongDigest
    {
        private const ulong A5 = 0xa5a5a5a5a5a5a5a5UL;

        private readonly int digestLength;

        private ulong H1t, H2t, H3t, H4t, H5t, H6t, H7t, H8t;

        /**
         * Standard constructor
         */
        public Sha512tDigest(int bitLength)
        {
            if (bitLength >= 512)
                throw new ArgumentException("cannot be >= 512", "bitLength");
            if (bitLength % 8 != 0)
                throw new ArgumentException("needs to be a multiple of 8", "bitLength");
            if (bitLength == 384)
                throw new ArgumentException("cannot be 384 use SHA384 instead", "bitLength");

            this.digestLength = bitLength / 8;

            tIvGenerate(digestLength * 8);

            Reset();
        }

        /**
         * Copy constructor.  This will copy the state of the provided
         * message digest.
         */
        public Sha512tDigest(Sha512tDigest t)
            : base(t)
        {
            this.digestLength = t.digestLength;

            this.H1t = t.H1t;
            this.H2t = t.H2t;
            this.H3t = t.H3t;
            this.H4t = t.H4t;
            this.H5t = t.H5t;
            this.H6t = t.H6t;
            this.H7t = t.H7t;
            this.H8t = t.H8t;
        }

        public override string AlgorithmName
        {
            get { return "SHA-512/" + (digestLength * 8); }
        }

        public override int GetDigestSize()
        {
            return digestLength;
        }

        public override int DoFinal(byte[] output, int outOff)
        {
            Finish();

            UInt64_To_BE(H1, output, outOff, digestLength);
            UInt64_To_BE(H2, output, outOff + 8, digestLength - 8);
            UInt64_To_BE(H3, output, outOff + 16, digestLength - 16);
            UInt64_To_BE(H4, output, outOff + 24, digestLength - 24);
            UInt64_To_BE(H5, output, outOff + 32, digestLength - 32);
            UInt64_To_BE(H6, output, outOff + 40, digestLength - 40);
            UInt64_To_BE(H7, output, outOff + 48, digestLength - 48);
            UInt64_To_BE(H8, output, outOff + 56, digestLength - 56);

            Reset();

            return digestLength;
        }

        /**
         * reset the chaining variables
         */
        public override void Reset()
        {
            base.Reset();

            /*
             * initial hash values use the iv generation algorithm for t.
             */
            H1 = H1t;
            H2 = H2t;
            H3 = H3t;
            H4 = H4t;
            H5 = H5t;
            H6 = H6t;
            H7 = H7t;
            H8 = H8t;
        }

        private void tIvGenerate(int bitLength)
        {
            H1 = 0x6a09e667f3bcc908UL ^ A5;
            H2 = 0xbb67ae8584caa73bUL ^ A5;
            H3 = 0x3c6ef372fe94f82bUL ^ A5;
            H4 = 0xa54ff53a5f1d36f1UL ^ A5;
            H5 = 0x510e527fade682d1UL ^ A5;
            H6 = 0x9b05688c2b3e6c1fUL ^ A5;
            H7 = 0x1f83d9abfb41bd6bUL ^ A5;
            H8 = 0x5be0cd19137e2179UL ^ A5;

            Update(0x53);
            Update(0x48);
            Update(0x41);
            Update(0x2D);
            Update(0x35);
            Update(0x31);
            Update(0x32);
            Update(0x2F);

            if (bitLength > 100)
            {
                Update((byte)(bitLength / 100 + 0x30));
                bitLength = bitLength % 100;
                Update((byte)(bitLength / 10 + 0x30));
                bitLength = bitLength % 10;
                Update((byte)(bitLength + 0x30));
            }
            else if (bitLength > 10)
            {
                Update((byte)(bitLength / 10 + 0x30));
                bitLength = bitLength % 10;
                Update((byte)(bitLength + 0x30));
            }
            else
            {
                Update((byte)(bitLength + 0x30));
            }

            Finish();

            H1t = H1;
            H2t = H2;
            H3t = H3;
            H4t = H4;
            H5t = H5;
            H6t = H6;
            H7t = H7;
            H8t = H8;
        }

        private static void UInt64_To_BE(ulong n, byte[] bs, int off, int max)
        {
            if (max > 0)
            {
                UInt32_To_BE((uint)(n >> 32), bs, off, max);

                if (max > 4)
                {
                    UInt32_To_BE((uint)n, bs, off + 4, max - 4);
                }
            }
        }

        private static void UInt32_To_BE(uint n, byte[] bs, int off, int max)
        {
            int num = System.Math.Min(4, max);
            while (--num >= 0)
            {
                int shift = 8 * (3 - num);
                bs[off + num] = (byte)(n >> shift);
            }
        }
    }
}
