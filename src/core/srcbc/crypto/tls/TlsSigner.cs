using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal interface TlsSigner
	{
    	byte[] CalculateRawSignature(AsymmetricKeyParameter privateKey, byte[] md5andsha1);
    	ISigner CreateSigner();
	}
}