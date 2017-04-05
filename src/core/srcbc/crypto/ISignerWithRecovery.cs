using System;
using System.Text;

namespace Org.BouncyCastle.Crypto
{
    /**
     * Signer with message recovery.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface ISignerWithRecovery
        : ISigner
    {
        /**
         * Returns true if the signer has recovered the full message as
         * part of signature verification.
         *
         * @return true if full message recovered.
         */
        bool HasFullMessage();

        /**
         * Returns a reference to what message was recovered (if any).
         *
         * @return full/partial message, null if nothing.
         */
        byte[] GetRecoveredMessage();

		/**
		 * Perform an update with the recovered message before adding any other data. This must
		 * be the first update method called, and calling it will result in the signer assuming
		 * that further calls to update will include message content past what is recoverable.
		 *
		 * @param signature the signature that we are in the process of verifying.
		 * @throws IllegalStateException
		 */
		void UpdateWithRecoveredMessage(byte[] signature);
	}
}
