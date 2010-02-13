using System;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsDssSigner
		: TlsSigner
	{
		public byte[] CalculateRawSignature(AsymmetricKeyParameter privateKey, byte[] md5andsha1)
		{
			// Note: Only use the SHA1 part of the hash
			ISigner sig = new DsaDigestSigner(new DsaSigner(), new NullDigest());
			sig.Init(true, privateKey);
			sig.BlockUpdate(md5andsha1, 16, 20);
			return sig.GenerateSignature();
		}

		public ISigner CreateSigner()
		{
			return new DsaDigestSigner(new DsaSigner(), new Sha1Digest());
		}
	}
}
