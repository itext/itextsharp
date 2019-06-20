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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Macs
{
    /// <summary>
    /// Implementation of SipHash as specified in "SipHash: a fast short-input PRF", by Jean-Philippe
    /// Aumasson and Daniel J. Bernstein (https://131002.net/siphash/siphash.pdf).
    /// </summary>
    /// <remarks>
    /// "SipHash is a family of PRFs SipHash-c-d where the integer parameters c and d are the number of
    /// compression rounds and the number of finalization rounds. A compression round is identical to a
    /// finalization round and this round function is called SipRound. Given a 128-bit key k and a
    /// (possibly empty) byte string m, SipHash-c-d returns a 64-bit value..."
    /// </remarks>
    public class SipHash
        : IMac
    {
        protected readonly int c, d;

        protected long k0, k1;
        protected long v0, v1, v2, v3, v4;

        protected byte[] buf = new byte[8];
        protected int bufPos = 0;
        protected int wordCount = 0;

        /// <summary>SipHash-2-4</summary>
        public SipHash()
            : this(2, 4)
        {
        }

        /// <summary>SipHash-c-d</summary>
        /// <param name="c">the number of compression rounds</param>
        /// <param name="d">the number of finalization rounds</param>
        public SipHash(int c, int d)
        {
            this.c = c;
            this.d = d;
        }

        public virtual string AlgorithmName
        {
            get { return "SipHash-" + c + "-" + d; }
        }

        public virtual int GetMacSize()
        {
            return 8;
        }

        public virtual void Init(ICipherParameters parameters)
        {
            KeyParameter keyParameter = parameters as KeyParameter;
            if (keyParameter == null)
                throw new ArgumentException("must be an instance of KeyParameter", "parameters");
            byte[] key = keyParameter.GetKey();
            if (key.Length != 16)
                throw new ArgumentException("must be a 128-bit key", "parameters");

            this.k0 = (long)Pack.LE_To_UInt64(key, 0);
            this.k1 = (long)Pack.LE_To_UInt64(key, 8);

            Reset();
        }

        public virtual void Update(byte input)
        {
            buf[bufPos] = input;
            if (++bufPos == buf.Length)
            {
                ProcessMessageWord();
                bufPos = 0;
            }
        }

        public virtual void BlockUpdate(byte[] input, int offset, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                buf[bufPos] = input[offset + i];
                if (++bufPos == buf.Length)
                {
                    ProcessMessageWord();
                    bufPos = 0;
                }
            }
        }

        public virtual long DoFinal()
        {
            buf[7] = (byte)((wordCount << 3) + bufPos);
            while (bufPos < 7)
            {
                buf[bufPos++] = 0;
            }

            ProcessMessageWord();

            v2 ^= 0xffL;

            ApplySipRounds(d);

            long result = v0 ^ v1 ^ v2 ^ v3;

            Reset();

            return result;
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            long result = DoFinal();
            Pack.UInt64_To_LE((ulong)result, output, outOff);
            return 8;
        }

        public virtual void Reset()
        {
            v0 = k0 ^ 0x736f6d6570736575L;
            v1 = k1 ^ 0x646f72616e646f6dL;
            v2 = k0 ^ 0x6c7967656e657261L;
            v3 = k1 ^ 0x7465646279746573L;

            Array.Clear(buf, 0, buf.Length);
            bufPos = 0;
            wordCount = 0;
        }

        protected virtual void ProcessMessageWord()
        {
            ++wordCount;
            long m = (long)Pack.LE_To_UInt64(buf, 0);
            v3 ^= m;
            ApplySipRounds(c);
            v0 ^= m;
        }

        protected virtual void ApplySipRounds(int n)
        {
            for (int r = 0; r < n; ++r)
            {
                v0 += v1;
                v2 += v3;
                v1 = RotateLeft(v1, 13);
                v3 = RotateLeft(v3, 16);
                v1 ^= v0;
                v3 ^= v2;
                v0 = RotateLeft(v0, 32);
                v2 += v1;
                v0 += v3;
                v1 = RotateLeft(v1, 17);
                v3 = RotateLeft(v3, 21);
                v1 ^= v2;
                v3 ^= v0;
                v2 = RotateLeft(v2, 32);
            }
        }

        protected static long RotateLeft(long x, int n)
        {
            ulong ux = (ulong)x;
            ux = (ux << n) | (ux >> (64 - n));
            return (long)ux;
        }
    }
}
