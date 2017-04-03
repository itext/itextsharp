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
     * implementation of SHA-1 as outlined in "Handbook of Applied Cryptography", pages 346 - 349.
     *
     * It is interesting to ponder why the, apart from the extra IV, the other difference here from MD5
     * is the "endianness" of the word processing!
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class Sha1Digest
        : GeneralDigest
    {
        private const int DigestLength = 20;

        private uint H1, H2, H3, H4, H5;

        private uint[] X = new uint[80];
        private int xOff;

        public Sha1Digest()
        {
            Reset();
        }

        /**
         * Copy constructor.  This will copy the state of the provided
         * message digest.
         */
        public Sha1Digest(Sha1Digest t)
            : base(t)
        {
            H1 = t.H1;
            H2 = t.H2;
            H3 = t.H3;
            H4 = t.H4;
            H5 = t.H5;

            Array.Copy(t.X, 0, X, 0, t.X.Length);
            xOff = t.xOff;
        }

        public override string AlgorithmName
        {
            get { return "SHA-1"; }
        }

        public override int GetDigestSize()
        {
            return DigestLength;
        }

        internal override void ProcessWord(
            byte[]  input,
            int     inOff)
        {
            X[xOff] = Pack.BE_To_UInt32(input, inOff);

            if (++xOff == 16)
            {
                ProcessBlock();
            }
        }

        internal override void ProcessLength(long    bitLength)
        {
            if (xOff > 14)
            {
                ProcessBlock();
            }

            X[14] = (uint)((ulong)bitLength >> 32);
            X[15] = (uint)((ulong)bitLength);
        }

        public override int DoFinal(
            byte[]  output,
            int     outOff)
        {
            Finish();

            Pack.UInt32_To_BE(H1, output, outOff);
            Pack.UInt32_To_BE(H2, output, outOff + 4);
            Pack.UInt32_To_BE(H3, output, outOff + 8);
            Pack.UInt32_To_BE(H4, output, outOff + 12);
            Pack.UInt32_To_BE(H5, output, outOff + 16);

            Reset();

            return DigestLength;
        }

        /**
         * reset the chaining variables
         */
        public override void Reset()
        {
            base.Reset();

            H1 = 0x67452301;
            H2 = 0xefcdab89;
            H3 = 0x98badcfe;
            H4 = 0x10325476;
            H5 = 0xc3d2e1f0;

            xOff = 0;
            Array.Clear(X, 0, X.Length);
        }

        //
        // Additive constants
        //
        private const uint Y1 = 0x5a827999;
        private const uint Y2 = 0x6ed9eba1;
        private const uint Y3 = 0x8f1bbcdc;
        private const uint Y4 = 0xca62c1d6;

        private static uint F(uint u, uint v, uint w)
        {
            return (u & v) | (~u & w);
        }

        private static uint H(uint u, uint v, uint w)
        {
            return u ^ v ^ w;
        }

        private static uint G(uint u, uint v, uint w)
        {
            return (u & v) | (u & w) | (v & w);
        }

        internal override void ProcessBlock()
        {
            //
            // expand 16 word block into 80 word block.
            //
            for (int i = 16; i < 80; i++)
            {
                uint t = X[i - 3] ^ X[i - 8] ^ X[i - 14] ^ X[i - 16];
                X[i] = t << 1 | t >> 31;
            }

            //
            // set up working variables.
            //
            uint A = H1;
            uint B = H2;
            uint C = H3;
            uint D = H4;
            uint E = H5;

            //
            // round 1
            //
            int idx = 0;

            for (int j = 0; j < 4; j++)
            {
                // E = rotateLeft(A, 5) + F(B, C, D) + E + X[idx++] + Y1
                // B = rotateLeft(B, 30)
                E += (A << 5 | (A >> 27)) + F(B, C, D) + X[idx++] + Y1;
                B = B << 30 | (B >> 2);

                D += (E << 5 | (E >> 27)) + F(A, B, C) + X[idx++] + Y1;
                A = A << 30 | (A >> 2);

                C += (D << 5 | (D >> 27)) + F(E, A, B) + X[idx++] + Y1;
                E = E << 30 | (E >> 2);

                B += (C << 5 | (C >> 27)) + F(D, E, A) + X[idx++] + Y1;
                D = D << 30 | (D >> 2);

                A += (B << 5 | (B >> 27)) + F(C, D, E) + X[idx++] + Y1;
                C = C << 30 | (C >> 2);
            }

            //
            // round 2
            //
            for (int j = 0; j < 4; j++)
            {
                // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y2
                // B = rotateLeft(B, 30)
                E += (A << 5 | (A >> 27)) + H(B, C, D) + X[idx++] + Y2;
                B = B << 30 | (B >> 2);

                D += (E << 5 | (E >> 27)) + H(A, B, C) + X[idx++] + Y2;
                A = A << 30 | (A >> 2);

                C += (D << 5 | (D >> 27)) + H(E, A, B) + X[idx++] + Y2;
                E = E << 30 | (E >> 2);

                B += (C << 5 | (C >> 27)) + H(D, E, A) + X[idx++] + Y2;
                D = D << 30 | (D >> 2);

                A += (B << 5 | (B >> 27)) + H(C, D, E) + X[idx++] + Y2;
                C = C << 30 | (C >> 2);
            }

            //
            // round 3
            //
            for (int j = 0; j < 4; j++)
            {
                // E = rotateLeft(A, 5) + G(B, C, D) + E + X[idx++] + Y3
                // B = rotateLeft(B, 30)
                E += (A << 5 | (A >> 27)) + G(B, C, D) + X[idx++] + Y3;
                B = B << 30 | (B >> 2);

                D += (E << 5 | (E >> 27)) + G(A, B, C) + X[idx++] + Y3;
                A = A << 30 | (A >> 2);

                C += (D << 5 | (D >> 27)) + G(E, A, B) + X[idx++] + Y3;
                E = E << 30 | (E >> 2);

                B += (C << 5 | (C >> 27)) + G(D, E, A) + X[idx++] + Y3;
                D = D << 30 | (D >> 2);

                A += (B << 5 | (B >> 27)) + G(C, D, E) + X[idx++] + Y3;
                C = C << 30 | (C >> 2);
            }

            //
            // round 4
            //
            for (int j = 0; j < 4; j++)
            {
                // E = rotateLeft(A, 5) + H(B, C, D) + E + X[idx++] + Y4
                // B = rotateLeft(B, 30)
                E += (A << 5 | (A >> 27)) + H(B, C, D) + X[idx++] + Y4;
                B = B << 30 | (B >> 2);

                D += (E << 5 | (E >> 27)) + H(A, B, C) + X[idx++] + Y4;
                A = A << 30 | (A >> 2);

                C += (D << 5 | (D >> 27)) + H(E, A, B) + X[idx++] + Y4;
                E = E << 30 | (E >> 2);

                B += (C << 5 | (C >> 27)) + H(D, E, A) + X[idx++] + Y4;
                D = D << 30 | (D >> 2);

                A += (B << 5 | (B >> 27)) + H(C, D, E) + X[idx++] + Y4;
                C = C << 30 | (C >> 2);
            }

            H1 += A;
            H2 += B;
            H3 += C;
            H4 += D;
            H5 += E;

            //
            // reset start of the buffer.
            //
            xOff = 0;
            Array.Clear(X, 0, 16);
        }
    }
}
