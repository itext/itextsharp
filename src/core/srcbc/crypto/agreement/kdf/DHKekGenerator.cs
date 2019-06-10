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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Kdf
{
    /**
    * RFC 2631 Diffie-hellman KEK derivation function.
    */
    public class DHKekGenerator
        : IDerivationFunction
    {
        private readonly IDigest digest;

        private DerObjectIdentifier	algorithm;
        private int					keySize;
        private byte[]				z;
        private byte[]				partyAInfo;

        public DHKekGenerator(IDigest digest)
        {
            this.digest = digest;
        }

        public virtual void Init(IDerivationParameters param)
        {
            DHKdfParameters parameters = (DHKdfParameters)param;

            this.algorithm = parameters.Algorithm;
            this.keySize = parameters.KeySize;
            this.z = parameters.GetZ(); // TODO Clone?
            this.partyAInfo = parameters.GetExtraInfo(); // TODO Clone?
        }

        public virtual IDigest Digest
        {
            get { return digest; }
        }

        public virtual int GenerateBytes(byte[]	outBytes, int outOff, int len)
        {
            if ((outBytes.Length - len) < outOff)
            {
                throw new DataLengthException("output buffer too small");
            }

            long oBytes = len;
            int outLen = digest.GetDigestSize();

            //
            // this is at odds with the standard implementation, the
            // maximum value should be hBits * (2^32 - 1) where hBits
            // is the digest output size in bits. We can't have an
            // array with a long index at the moment...
            //
            if (oBytes > ((2L << 32) - 1))
            {
                throw new ArgumentException("Output length too large");
            }

            int cThreshold = (int)((oBytes + outLen - 1) / outLen);

            byte[] dig = new byte[digest.GetDigestSize()];

            uint counter = 1;

            for (int i = 0; i < cThreshold; i++)
            {
                digest.BlockUpdate(z, 0, z.Length);

                // KeySpecificInfo
                DerSequence keyInfo = new DerSequence(
                    algorithm,
                    new DerOctetString(Pack.UInt32_To_BE(counter)));

                // OtherInfo
                Asn1EncodableVector v1 = new Asn1EncodableVector(keyInfo);

                if (partyAInfo != null)
                {
                    v1.Add(new DerTaggedObject(true, 0, new DerOctetString(partyAInfo)));
                }

                v1.Add(new DerTaggedObject(true, 2, new DerOctetString(Pack.UInt32_To_BE((uint)keySize))));

                byte[] other = new DerSequence(v1).GetDerEncoded();

                digest.BlockUpdate(other, 0, other.Length);

                digest.DoFinal(dig, 0);

                if (len > outLen)
                {
                    Array.Copy(dig, 0, outBytes, outOff, outLen);
                    outOff += outLen;
                    len -= outLen;
                }
                else
                {
                    Array.Copy(dig, 0, outBytes, outOff, len);
                }

                counter++;
            }

            digest.Reset();

            return (int)oBytes;
        }
    }
}
