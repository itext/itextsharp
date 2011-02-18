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
		public virtual byte[] CalculateRawSignature(SecureRandom random,
			AsymmetricKeyParameter privateKey, byte[] md5andsha1)
		{
			// Note: Only use the SHA1 part of the hash
			ISigner sig = new DsaDigestSigner(CreateDsaImpl(), new NullDigest());
			sig.Init(true, new ParametersWithRandom(privateKey, random));
			sig.BlockUpdate(md5andsha1, 16, 20);
			return sig.GenerateSignature();
		}

		public virtual ISigner CreateVerifyer(AsymmetricKeyParameter publicKey)
		{
			ISigner s = new DsaDigestSigner(CreateDsaImpl(), new Sha1Digest());
			s.Init(false, publicKey);
			return s;
		}

		public abstract bool IsValidPublicKey(AsymmetricKeyParameter publicKey);

		protected abstract IDsa CreateDsaImpl();
	}
}
