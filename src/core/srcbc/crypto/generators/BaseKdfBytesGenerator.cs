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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Generators
{
    /**
    * Basic KDF generator for derived keys and ivs as defined by IEEE P1363a/ISO 18033
    * <br/>
    * This implementation is based on ISO 18033/P1363a.
    */
    public class BaseKdfBytesGenerator
        : IDerivationFunction
    {
        private int     counterStart;
        private IDigest  digest;
        private byte[]  shared;
        private byte[]  iv;

        /**
        * Construct a KDF Parameters generator.
        *
        * @param counterStart value of counter.
        * @param digest the digest to be used as the source of derived keys.
        */
        public BaseKdfBytesGenerator(int counterStart, IDigest digest)
        {
            this.counterStart = counterStart;
            this.digest = digest;
        }

        public virtual void Init(IDerivationParameters parameters)
        {
            if (parameters is KdfParameters)
            {
                KdfParameters   p = (KdfParameters)parameters;

                shared = p.GetSharedSecret();
                iv = p.GetIV();
            }
            else if (parameters is Iso18033KdfParameters)
            {
                Iso18033KdfParameters p = (Iso18033KdfParameters)parameters;

                shared = p.GetSeed();
                iv = null;
            }
            else
            {
                throw new ArgumentException("KDF parameters required for KDF Generator");
            }
        }

        /**
        * return the underlying digest.
        */
        public virtual IDigest Digest
        {
            get { return digest; }
        }

        /**
        * fill len bytes of the output buffer with bytes generated from
        * the derivation function.
        *
        * @throws ArgumentException if the size of the request will cause an overflow.
        * @throws DataLengthException if the out buffer is too small.
        */
        public virtual int GenerateBytes(byte[] output, int outOff, int length)
        {
            if ((output.Length - length) < outOff)
                throw new DataLengthException("output buffer too small");

            long oBytes = length;
            int outLen = digest.GetDigestSize();

            //
            // this is at odds with the standard implementation, the
            // maximum value should be hBits * (2^32 - 1) where hBits
            // is the digest output size in bits. We can't have an
            // array with a long index at the moment...
            //
            if (oBytes > ((2L << 32) - 1))
                throw new ArgumentException("Output length too large");

            int cThreshold = (int)((oBytes + outLen - 1) / outLen);

            byte[] dig = new byte[digest.GetDigestSize()];

            byte[] C = new byte[4];
            Pack.UInt32_To_BE((uint)counterStart, C, 0);

            uint counterBase = (uint)(counterStart & ~0xFF);

            for (int i = 0; i < cThreshold; i++)
            {
                digest.BlockUpdate(shared, 0, shared.Length);
                digest.BlockUpdate(C, 0, 4);

                if (iv != null)
                {
                    digest.BlockUpdate(iv, 0, iv.Length);
                }

                digest.DoFinal(dig, 0);

                if (length > outLen)
                {
                    Array.Copy(dig, 0, output, outOff, outLen);
                    outOff += outLen;
                    length -= outLen;
                }
                else
                {
                    Array.Copy(dig, 0, output, outOff, length);
                }

                if (++C[3] == 0)
                {
                    counterBase += 0x100;
                    Pack.UInt32_To_BE(counterBase, C, 0);
                }
            }

            digest.Reset();

            return (int)oBytes;
        }
    }
}