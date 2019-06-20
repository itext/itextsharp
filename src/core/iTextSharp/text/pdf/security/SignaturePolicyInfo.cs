/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
