using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Esf;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace iTextSharp.text.pdf.security {
    /**
     * Class that encapsulates the signature policy information
     * @author J. Arturo
     * 
     * Sample:
     * 
     *      SignaturePolicyInfo spi = new SignaturePolicyInfo("2.16.724.1.3.1.1.2.1.9", 
     *      "G7roucf600+f03r/o0bAOQ6WAs0=", "SHA-1", "https://sede.060.gob.es/politica_de_firma_anexo_1.pdf");
     */

    public class SignaturePolicyInfo {
        private string policyIdentifier;
        private byte[] policyHash;
        private string policyDigestAlgorithm;
        private string policyUri;

        public SignaturePolicyInfo(string policyIdentifier, byte[] policyHash, string policyDigestAlgorithm, string policyUri) {
            if (string.IsNullOrEmpty(policyIdentifier)) {
                throw new ArgumentException("Policy identifier cannot be null");
            }
            if (policyHash == null) {
                throw new ArgumentException("Policy hash cannot be null");
            }
            if (string.IsNullOrEmpty(policyDigestAlgorithm)) {
                throw new ArgumentException("Policy digest algorithm cannot be null");
            }

            this.policyIdentifier = policyIdentifier;
            this.policyHash = policyHash;
            this.policyDigestAlgorithm = policyDigestAlgorithm;
            this.policyUri = policyUri;
        }

        public SignaturePolicyInfo(string policyIdentifier, string policyHashBase64, string policyDigestAlgorithm,
            string policyUri) : this(
                policyIdentifier, policyHashBase64 != null ? Convert.FromBase64String(policyHashBase64) : null,
                policyDigestAlgorithm, policyUri) {
        }

        public string PolicyIdentifier {
            get { return policyIdentifier; }
        }

        public byte[] PolicyHash {
            get { return policyHash; }
        }

        public string PolicyDigestAlgorithm {
            get { return policyDigestAlgorithm; }
        }

        public string PolicyUri {
            get { return policyUri; }
        }

        protected internal SignaturePolicyIdentifier ToSignaturePolicyIdentifier() {
            string algId = DigestAlgorithms.GetAllowedDigests(this.PolicyDigestAlgorithm);

            if (string.IsNullOrEmpty(algId)) {
                throw new ArgumentException("Invalid policy hash algorithm");
            }

            SignaturePolicyIdentifier signaturePolicyIdentifier = null;
            SigPolicyQualifierInfo spqi = null;

            if (!string.IsNullOrEmpty(this.PolicyUri)) {
                spqi = new SigPolicyQualifierInfo(PkcsObjectIdentifiers.IdSpqEtsUri, new DerIA5String(this.PolicyUri));
            }

            signaturePolicyIdentifier = new SignaturePolicyIdentifier(new SignaturePolicyId(
                        DerObjectIdentifier.GetInstance(new DerObjectIdentifier(this.PolicyIdentifier.Replace("urn:oid:", ""))),
                        new OtherHashAlgAndValue(new AlgorithmIdentifier(algId), new DerOctetString(this.PolicyHash)), spqi));

            return signaturePolicyIdentifier;
        }
    }
}
