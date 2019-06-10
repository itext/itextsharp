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
using System.IO;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace Org.BouncyCastle.Crypto.Tls
{
	public class DefaultTlsCipherFactory
		: TlsCipherFactory
	{
		public virtual TlsCipher CreateCipher(TlsClientContext context,
			EncryptionAlgorithm encryptionAlgorithm, DigestAlgorithm digestAlgorithm)
		{
			switch (encryptionAlgorithm)
			{
				case EncryptionAlgorithm.cls_3DES_EDE_CBC:
					return CreateDesEdeCipher(context, 24, digestAlgorithm);
				case EncryptionAlgorithm.AES_128_CBC:
					return CreateAesCipher(context, 16, digestAlgorithm);
				case EncryptionAlgorithm.AES_256_CBC:
					return CreateAesCipher(context, 32, digestAlgorithm);
                case EncryptionAlgorithm.RC4_128:
                    return CreateRC4Cipher(context, 16, digestAlgorithm);
				default:
					throw new TlsFatalAlert(AlertDescription.internal_error);
			}
		}

        /// <exception cref="IOException"></exception>
        protected virtual TlsCipher CreateRC4Cipher(TlsClientContext context, int cipherKeySize, DigestAlgorithm digestAlgorithm)
        {
            return new TlsStreamCipher(context, CreateRC4StreamCipher(), CreateRC4StreamCipher(), CreateDigest(digestAlgorithm), CreateDigest(digestAlgorithm), cipherKeySize);
        }

		/// <exception cref="IOException"></exception>
		protected virtual TlsCipher CreateAesCipher(TlsClientContext context, int cipherKeySize,
			DigestAlgorithm digestAlgorithm)
		{
			return new TlsBlockCipher(context, CreateAesBlockCipher(), CreateAesBlockCipher(),
				CreateDigest(digestAlgorithm), CreateDigest(digestAlgorithm), cipherKeySize);
		}

		/// <exception cref="IOException"></exception>
		protected virtual TlsCipher CreateDesEdeCipher(TlsClientContext context, int cipherKeySize,
			DigestAlgorithm digestAlgorithm)
		{
			return new TlsBlockCipher(context, CreateDesEdeBlockCipher(), CreateDesEdeBlockCipher(),
				CreateDigest(digestAlgorithm), CreateDigest(digestAlgorithm), cipherKeySize);
		}

        protected virtual IStreamCipher CreateRC4StreamCipher()
        {
            return new RC4Engine();
        }

		protected virtual IBlockCipher CreateAesBlockCipher()
		{
			return new CbcBlockCipher(new AesFastEngine());
		}

		protected virtual IBlockCipher CreateDesEdeBlockCipher()
		{
			return new CbcBlockCipher(new DesEdeEngine());
		}

		/// <exception cref="IOException"></exception>
		protected virtual IDigest CreateDigest(DigestAlgorithm digestAlgorithm)
		{
			switch (digestAlgorithm)
			{
				case DigestAlgorithm.MD5:
					return new MD5Digest();
				case DigestAlgorithm.SHA:
					return new Sha1Digest();
				case DigestAlgorithm.SHA256:
					return new Sha256Digest();
				case DigestAlgorithm.SHA384:
					return new Sha384Digest();
				default:
					throw new TlsFatalAlert(AlertDescription.internal_error);
			}
		}
	}
}
