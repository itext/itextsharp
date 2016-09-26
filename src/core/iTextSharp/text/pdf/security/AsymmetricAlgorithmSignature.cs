using System;
using System.Security.Cryptography;

namespace iTextSharp.text.pdf.security {
    public class AsymmetricAlgorithmSignature : IExternalSignature {
        private AsymmetricAlgorithm algorithm;
        /** The hash algorithm. */
        private String hashAlgorithm;
        /** The encryption algorithm (obtained from the private key) */
        private String encryptionAlgorithm;

        private AsymmetricAlgorithmSignature(AsymmetricAlgorithm algorithm, String hashAlgorithm) {
            this.algorithm = algorithm;
            this.hashAlgorithm = DigestAlgorithms.GetDigest(DigestAlgorithms.GetAllowedDigests(hashAlgorithm));

            if (algorithm is RSACryptoServiceProvider)
                encryptionAlgorithm = "RSA";
            else if (algorithm is DSACryptoServiceProvider)
                encryptionAlgorithm = "DSA";
            else
                throw new ArgumentException("Not supported encryption algorithm " + algorithm);
        }

        public AsymmetricAlgorithmSignature(RSACryptoServiceProvider algorithm, String hashAlgorithm)
            : this((AsymmetricAlgorithm) algorithm, hashAlgorithm) {
        }

        public AsymmetricAlgorithmSignature(DSACryptoServiceProvider algorithm)
            : this((AsymmetricAlgorithm) algorithm, null) {
        }

        public byte[] Sign(byte[] message) {
            if (algorithm is RSACryptoServiceProvider) {
                RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) algorithm;
                return rsa.SignData(message, hashAlgorithm);
            } else {
                DSACryptoServiceProvider dsa = (DSACryptoServiceProvider) algorithm;
                return dsa.SignData(message);
            }
        }

        public string GetHashAlgorithm() {
            return hashAlgorithm;
        }

        /**
         * Returns the encryption algorithm used for signing.
         * @return the encryption algorithm ("RSA" or "DSA")
         * @see com.itextpdf.text.pdf.security.ExternalSignature#getEncryptionAlgorithm()
         */

        public string GetEncryptionAlgorithm() {
            return encryptionAlgorithm;
        }
    }
}
