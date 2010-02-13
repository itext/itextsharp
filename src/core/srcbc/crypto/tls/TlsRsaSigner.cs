using System;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsRsaSigner
    	: TlsSigner
	{
		public byte[] CalculateRawSignature(AsymmetricKeyParameter privateKey, byte[] md5andsha1)
		{
			ISigner sig = new GenericSigner(new Pkcs1Encoding(new RsaBlindedEngine()), new NullDigest());
			sig.Init(true, privateKey);
			sig.BlockUpdate(md5andsha1, 0, md5andsha1.Length);
			return sig.GenerateSignature();
		}

		public ISigner CreateSigner()
		{
			return new GenericSigner(new Pkcs1Encoding(new RsaBlindedEngine()), new CombinedHash());
		}
	}
}
