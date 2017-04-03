using System;

namespace Org.BouncyCastle.Crypto
{
    /**
     * base interface for general purpose byte derivation functions.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IDerivationFunction
    {
        void Init(IDerivationParameters parameters);

        /**
         * return the message digest used as the basis for the function
         */
        IDigest Digest
        {
            get;
        }

        int GenerateBytes(byte[] output, int outOff, int length);
        //throws DataLengthException, ArgumentException;
    }

}
