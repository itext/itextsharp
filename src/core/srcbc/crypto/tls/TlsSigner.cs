using System;

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface TlsSigner
    {
        byte[] GenerateRawSignature(SecureRandom random, AsymmetricKeyParameter privateKey,
            byte[] md5andsha1);
        bool VerifyRawSignature(byte[] sigBytes, AsymmetricKeyParameter publicKey, byte[] md5andsha1);

        ISigner CreateSigner(SecureRandom random, AsymmetricKeyParameter privateKey);
        ISigner CreateVerifyer(AsymmetricKeyParameter publicKey);

        bool IsValidPublicKey(AsymmetricKeyParameter publicKey);
    }
}
