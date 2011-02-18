using System;

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
	public interface TlsSigner
	{
    	byte[] CalculateRawSignature(SecureRandom random, AsymmetricKeyParameter privateKey,
			byte[] md5andsha1);

		ISigner CreateVerifyer(AsymmetricKeyParameter publicKey);

		bool IsValidPublicKey(AsymmetricKeyParameter publicKey);
	}
}
