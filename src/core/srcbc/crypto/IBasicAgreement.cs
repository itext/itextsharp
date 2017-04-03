using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto
{
    /**
     * The basic interface that basic Diffie-Hellman implementations
     * conforms to.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IBasicAgreement
    {
        /**
         * initialise the agreement engine.
         */
        void Init(ICipherParameters parameters);

        /**
         * return the field size for the agreement algorithm in bytes.
         */
        int GetFieldSize();

        /**
         * given a public key from a given party calculate the next
         * message in the agreement sequence.
         */
        BigInteger CalculateAgreement(ICipherParameters pubKey);
    }

}
