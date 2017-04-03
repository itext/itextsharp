using System;

namespace Org.BouncyCastle.Crypto
{
    /**
     * interface that a public/private key pair generator should conform to.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IAsymmetricCipherKeyPairGenerator
    {
        /**
         * intialise the key pair generator.
         *
         * @param the parameters the key pair is to be initialised with.
         */
        void Init(KeyGenerationParameters parameters);

        /**
         * return an AsymmetricCipherKeyPair containing the Generated keys.
         *
         * @return an AsymmetricCipherKeyPair containing the Generated keys.
         */
        AsymmetricCipherKeyPair GenerateKeyPair();
    }
}
