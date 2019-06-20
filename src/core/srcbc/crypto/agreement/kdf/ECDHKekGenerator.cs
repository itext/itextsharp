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
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Kdf
{
    /**
    * X9.63 based key derivation function for ECDH CMS.
    */
    public class ECDHKekGenerator
        : IDerivationFunction
    {
        private readonly IDerivationFunction kdf;

        private DerObjectIdentifier	algorithm;
        private int					keySize;
        private byte[]				z;

        public ECDHKekGenerator(IDigest digest)
        {
            this.kdf = new Kdf2BytesGenerator(digest);
        }

        public virtual void Init(IDerivationParameters param)
        {
            DHKdfParameters parameters = (DHKdfParameters)param;

            this.algorithm = parameters.Algorithm;
            this.keySize = parameters.KeySize;
            this.z = parameters.GetZ(); // TODO Clone?
        }

        public virtual IDigest Digest
        {
            get { return kdf.Digest; }
        }

        public virtual int GenerateBytes(byte[]	outBytes, int outOff, int len)
        {
            // TODO Create an ASN.1 class for this (RFC3278)
            // ECC-CMS-SharedInfo
            DerSequence s = new DerSequence(
                new AlgorithmIdentifier(algorithm, DerNull.Instance),
                new DerTaggedObject(true, 2, new DerOctetString(Pack.UInt32_To_BE((uint)keySize))));

            kdf.Init(new KdfParameters(z, s.GetDerEncoded()));

            return kdf.GenerateBytes(outBytes, outOff, len);
        }
    }
}
