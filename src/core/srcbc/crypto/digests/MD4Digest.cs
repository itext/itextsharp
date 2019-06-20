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
    * implementation of MD4 as RFC 1320 by R. Rivest, MIT Laboratory for
    * Computer Science and RSA Data Security, Inc.
    * <p>
    * <b>NOTE</b>: This algorithm is only included for backwards compatibility
    * with legacy applications, it's not secure, don't use it for anything new!</p>
    */
    public class MD4Digest
		: GeneralDigest
    {
        private const int    DigestLength = 16;

        private int     H1, H2, H3, H4;         // IV's

        private int[]   X = new int[16];
        private int     xOff;

        /**
        * Standard constructor
        */
        public MD4Digest()
        {
            Reset();
        }

        /**
        * Copy constructor.  This will copy the state of the provided
        * message digest.
        */
        public MD4Digest(MD4Digest t) : base(t)
        {
            H1 = t.H1;
            H2 = t.H2;
            H3 = t.H3;
            H4 = t.H4;

            Array.Copy(t.X, 0, X, 0, t.X.Length);
            xOff = t.xOff;
        }

		public override string AlgorithmName
		{
			get { return "MD4"; }
		}

		public override int GetDigestSize()
		{
			return DigestLength;
		}

		internal override void ProcessWord(
            byte[]  input,
            int     inOff)
        {
            X[xOff++] = (input[inOff] & 0xff) | ((input[inOff + 1] & 0xff) << 8)
                | ((input[inOff + 2] & 0xff) << 16) | ((input[inOff + 3] & 0xff) << 24);

            if (xOff == 16)
            {
                ProcessBlock();
            }
        }

        internal override void ProcessLength(
            long    bitLength)
        {
            if (xOff > 14)
            {
                ProcessBlock();
            }

            X[14] = (int)(bitLength & 0xffffffff);
            X[15] = (int)((ulong) bitLength >> 32);
        }

        private void UnpackWord(
            int     word,
            byte[]  outBytes,
            int     outOff)
        {
            outBytes[outOff]     = (byte)word;
            outBytes[outOff + 1] = (byte)((uint) word >> 8);
            outBytes[outOff + 2] = (byte)((uint) word >> 16);
            outBytes[outOff + 3] = (byte)((uint) word >> 24);
        }

        public override int DoFinal(
            byte[]  output,
            int     outOff)
        {
            Finish();

            UnpackWord(H1, output, outOff);
            UnpackWord(H2, output, outOff + 4);
            UnpackWord(H3, output, outOff + 8);
            UnpackWord(H4, output, outOff + 12);

            Reset();

            return DigestLength;
        }

        /**
        * reset the chaining variables to the IV values.
        */
        public override void Reset()
        {
            base.Reset();

            H1 = unchecked((int) 0x67452301);
            H2 = unchecked((int) 0xefcdab89);
            H3 = unchecked((int) 0x98badcfe);
            H4 = unchecked((int) 0x10325476);

            xOff = 0;

            for (int i = 0; i != X.Length; i++)
            {
                X[i] = 0;
            }
        }

        //
        // round 1 left rotates
        //
        private const int S11 = 3;
        private const int S12 = 7;
        private const int S13 = 11;
        private const int S14 = 19;

        //
        // round 2 left rotates
        //
        private const int S21 = 3;
        private const int S22 = 5;
        private const int S23 = 9;
        private const int S24 = 13;

        //
        // round 3 left rotates
        //
        private const int S31 = 3;
        private const int S32 = 9;
        private const int S33 = 11;
        private const int S34 = 15;

        /*
        * rotate int x left n bits.
        */
        private int RotateLeft(
            int x,
            int n)
        {
            return (x << n) | (int) ((uint) x >> (32 - n));
        }

        /*
        * F, G, H and I are the basic MD4 functions.
        */
        private int F(
            int u,
            int v,
            int w)
        {
            return (u & v) | (~u & w);
        }

        private int G(
            int u,
            int v,
            int w)
        {
            return (u & v) | (u & w) | (v & w);
        }

        private int H(
            int u,
            int v,
            int w)
        {
            return u ^ v ^ w;
        }

        internal override void ProcessBlock()
        {
            int a = H1;
            int b = H2;
            int c = H3;
            int d = H4;

            //
            // Round 1 - F cycle, 16 times.
            //
            a = RotateLeft((a + F(b, c, d) + X[ 0]), S11);
            d = RotateLeft((d + F(a, b, c) + X[ 1]), S12);
            c = RotateLeft((c + F(d, a, b) + X[ 2]), S13);
            b = RotateLeft((b + F(c, d, a) + X[ 3]), S14);
            a = RotateLeft((a + F(b, c, d) + X[ 4]), S11);
            d = RotateLeft((d + F(a, b, c) + X[ 5]), S12);
            c = RotateLeft((c + F(d, a, b) + X[ 6]), S13);
            b = RotateLeft((b + F(c, d, a) + X[ 7]), S14);
            a = RotateLeft((a + F(b, c, d) + X[ 8]), S11);
            d = RotateLeft((d + F(a, b, c) + X[ 9]), S12);
            c = RotateLeft((c + F(d, a, b) + X[10]), S13);
            b = RotateLeft((b + F(c, d, a) + X[11]), S14);
            a = RotateLeft((a + F(b, c, d) + X[12]), S11);
            d = RotateLeft((d + F(a, b, c) + X[13]), S12);
            c = RotateLeft((c + F(d, a, b) + X[14]), S13);
            b = RotateLeft((b + F(c, d, a) + X[15]), S14);

            //
            // Round 2 - G cycle, 16 times.
            //
            a = RotateLeft((a + G(b, c, d) + X[ 0] + 0x5a827999), S21);
            d = RotateLeft((d + G(a, b, c) + X[ 4] + 0x5a827999), S22);
            c = RotateLeft((c + G(d, a, b) + X[ 8] + 0x5a827999), S23);
            b = RotateLeft((b + G(c, d, a) + X[12] + 0x5a827999), S24);
            a = RotateLeft((a + G(b, c, d) + X[ 1] + 0x5a827999), S21);
            d = RotateLeft((d + G(a, b, c) + X[ 5] + 0x5a827999), S22);
            c = RotateLeft((c + G(d, a, b) + X[ 9] + 0x5a827999), S23);
            b = RotateLeft((b + G(c, d, a) + X[13] + 0x5a827999), S24);
            a = RotateLeft((a + G(b, c, d) + X[ 2] + 0x5a827999), S21);
            d = RotateLeft((d + G(a, b, c) + X[ 6] + 0x5a827999), S22);
            c = RotateLeft((c + G(d, a, b) + X[10] + 0x5a827999), S23);
            b = RotateLeft((b + G(c, d, a) + X[14] + 0x5a827999), S24);
            a = RotateLeft((a + G(b, c, d) + X[ 3] + 0x5a827999), S21);
            d = RotateLeft((d + G(a, b, c) + X[ 7] + 0x5a827999), S22);
            c = RotateLeft((c + G(d, a, b) + X[11] + 0x5a827999), S23);
            b = RotateLeft((b + G(c, d, a) + X[15] + 0x5a827999), S24);

            //
            // Round 3 - H cycle, 16 times.
            //
            a = RotateLeft((a + H(b, c, d) + X[ 0] + 0x6ed9eba1), S31);
            d = RotateLeft((d + H(a, b, c) + X[ 8] + 0x6ed9eba1), S32);
            c = RotateLeft((c + H(d, a, b) + X[ 4] + 0x6ed9eba1), S33);
            b = RotateLeft((b + H(c, d, a) + X[12] + 0x6ed9eba1), S34);
            a = RotateLeft((a + H(b, c, d) + X[ 2] + 0x6ed9eba1), S31);
            d = RotateLeft((d + H(a, b, c) + X[10] + 0x6ed9eba1), S32);
            c = RotateLeft((c + H(d, a, b) + X[ 6] + 0x6ed9eba1), S33);
            b = RotateLeft((b + H(c, d, a) + X[14] + 0x6ed9eba1), S34);
            a = RotateLeft((a + H(b, c, d) + X[ 1] + 0x6ed9eba1), S31);
            d = RotateLeft((d + H(a, b, c) + X[ 9] + 0x6ed9eba1), S32);
            c = RotateLeft((c + H(d, a, b) + X[ 5] + 0x6ed9eba1), S33);
            b = RotateLeft((b + H(c, d, a) + X[13] + 0x6ed9eba1), S34);
            a = RotateLeft((a + H(b, c, d) + X[ 3] + 0x6ed9eba1), S31);
            d = RotateLeft((d + H(a, b, c) + X[11] + 0x6ed9eba1), S32);
            c = RotateLeft((c + H(d, a, b) + X[ 7] + 0x6ed9eba1), S33);
            b = RotateLeft((b + H(c, d, a) + X[15] + 0x6ed9eba1), S34);

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;

            //
            // reset the offset and clean out the word buffer.
            //
            xOff = 0;
            for (int i = 0; i != X.Length; i++)
            {
                X[i] = 0;
            }
        }
    }

}
