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

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
    internal abstract class TlsDsaSigner
        :	TlsSigner
    {
        public virtual byte[] GenerateRawSignature(SecureRandom random,
            AsymmetricKeyParameter privateKey, byte[] md5andsha1)
        {
            ISigner s = MakeSigner(new NullDigest(), true, new ParametersWithRandom(privateKey, random));
            // Note: Only use the SHA1 part of the hash
            s.BlockUpdate(md5andsha1, 16, 20);
            return s.GenerateSignature();
        }

        public bool VerifyRawSignature(byte[] sigBytes, AsymmetricKeyParameter publicKey, byte[] md5andsha1)
        {
            ISigner s = MakeSigner(new NullDigest(), false, publicKey);
            // Note: Only use the SHA1 part of the hash
            s.BlockUpdate(md5andsha1, 16, 20);
            return s.VerifySignature(sigBytes);
        }

        public virtual ISigner CreateSigner(SecureRandom random, AsymmetricKeyParameter privateKey)
        {
            return MakeSigner(new Sha1Digest(), true, new ParametersWithRandom(privateKey, random));
        }

        public virtual ISigner CreateVerifyer(AsymmetricKeyParameter publicKey)
        {
            return MakeSigner(new Sha1Digest(), false, publicKey);
        }

        public abstract bool IsValidPublicKey(AsymmetricKeyParameter publicKey);

        protected virtual ISigner MakeSigner(IDigest d, bool forSigning, ICipherParameters cp)
        {
            ISigner s = new DsaDigestSigner(CreateDsaImpl(), d);
            s.Init(forSigning, cp);
            return s;
        }

        protected abstract IDsa CreateDsaImpl();
    }
}
