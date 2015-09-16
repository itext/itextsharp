using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf.security {

    /**
     * Add verification according to PAdES-LTV (part 4)
     * @author psoares
     */
    public class LtvVerification {

        private ILogger LOGGER = LoggerFactory.GetLogger(typeof (LtvVerification));
        private PdfStamper stp;
        private PdfWriter writer;
        private PdfReader reader;
        private AcroFields acroFields;
        private IDictionary<PdfName,ValidationData> validated = new Dictionary<PdfName,ValidationData>();
        private bool used = false;
        /**
         * What type of verification to include 
         */
        public enum Level {
            /**
             * Include only OCSP
             */
            OCSP, 
            /**
             * Include only CRL
             */
            CRL, 
            /**
             * Include both OCSP and CRL
             */
            OCSP_CRL, 
            /**
             * Include CRL only if OCSP can't be read
             */
            OCSP_OPTIONAL_CRL
        }

        /**
         * Options for how many certificates to include
         */
        public enum CertificateOption {
            /**
             * Include verification just for the signing certificate
             */
            SIGNING_CERTIFICATE,
            /**
             * Include verification for the whole chain of certificates
             */
            WHOLE_CHAIN
        }
        
        /**
         * Certificate inclusion in the DSS and VRI dictionaries in the CERT and CERTS
         * keys
         */
        public enum CertificateInclusion {
            /**
             * Include certificates in the DSS and VRI dictionaries
             */
            YES,
            /**
             * Do not include certificates in the DSS and VRI dictionaries
             */
            NO
        }

        /**
         * The verification constructor. This class should only be created with
         * PdfStamper.getLtvVerification() otherwise the information will not be
         * added to the Pdf.
         * @param stp the PdfStamper to apply the validation to
         */
        public LtvVerification(PdfStamper stp) {
            this.stp = stp;
            writer = stp.Writer;
            reader = stp.Reader;
            acroFields = stp.AcroFields;
        }

        /**
         * Add verification for a particular signature
         * @param signatureName the signature to validate (it may be a timestamp)
         * @param ocsp the interface to get the OCSP
         * @param crl the interface to get the CRL
         * @param certOption
         * @param level the validation options to include
         * @param certInclude
         * @return true if a validation was generated, false otherwise
         * @throws Exception
         */
        virtual public bool AddVerification(String signatureName, IOcspClient ocsp, ICrlClient crl, CertificateOption certOption, Level level, CertificateInclusion certInclude) {
            if (used)
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("verification.already.output"));
            PdfPKCS7 pk = acroFields.VerifySignature(signatureName);
            LOGGER.Info("Adding verification for " + signatureName);
            X509Certificate[] xc = pk.Certificates;
            X509Certificate cert;
            X509Certificate signingCert = pk.SigningCertificate;
            ValidationData vd = new ValidationData();
            for (int k = 0; k < xc.Length; ++k) {
                cert = xc[k];
                LOGGER.Info("Certificate: " + cert.SubjectDN);
                if (certOption == CertificateOption.SIGNING_CERTIFICATE
                    && !cert.Equals(signingCert)) {
                    continue;
                }
                byte[] ocspEnc = null;
                if (ocsp != null && level != Level.CRL) {
                    ocspEnc = ocsp.GetEncoded(cert, GetParent(cert, xc), null);
                    if (ocspEnc != null) {
                        vd.ocsps.Add(BuildOCSPResponse(ocspEnc));
                        LOGGER.Info("OCSP added");
                    }
                }
                if (crl != null && (level == Level.CRL || level == Level.OCSP_CRL || (level == Level.OCSP_OPTIONAL_CRL && ocspEnc == null))) {
                    ICollection<byte[]> cims = crl.GetEncoded(xc[k], null);
                    if (cims != null) {
                        foreach (byte[] cim in cims) {
                            bool dup = false;
                            foreach (byte[] b in vd.crls) {
                                if (Arrays.AreEqual(b, cim)) {
                                    dup = true;
                                    break;
                                }
                            }
                            if (!dup) {
                                vd.crls.Add(cim);
                                LOGGER.Info("CRL added");
                            }
                        }
                    }
                }
                if (certInclude == CertificateInclusion.YES) {
                    vd.certs.Add(xc[k].GetEncoded());
                }
            }
            if (vd.crls.Count == 0 && vd.ocsps.Count == 0)
                return false;
            validated[GetSignatureHashKey(signatureName)] = vd;
            return true;
        }

        /**
         * Returns the issuing certificate for a child certificate.
         * @param cert  the certificate for which we search the parent
         * @param certs an array with certificates that contains the parent
         * @return  the partent certificate
         */
        private X509Certificate GetParent(X509Certificate cert, X509Certificate[] certs) {
            X509Certificate parent;
            for (int i = 0; i < certs.Length; i++) {
                parent = certs[i];
                if (!cert.IssuerDN.Equals(parent.SubjectDN))
                    continue;
                try {
                    cert.Verify(parent.GetPublicKey());
                    return parent;
                } catch {
                    // do nothing
                }
            }
            return null;
        }

        /**
         * Alternative addVerification.
         * I assume that inputs are deduplicated.
         *
         * @throws IOException
         * @throws GeneralSecurityException
         *
         */
        virtual public bool AddVerification(String signatureName, ICollection<byte[]> ocsps, ICollection<byte[]> crls, ICollection<byte[]> certs) {
            if (used)
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("verification.already.output"));
            ValidationData vd = new ValidationData();
            if (ocsps != null) {
                foreach (byte[] ocsp in ocsps) {
                    vd.ocsps.Add(BuildOCSPResponse(ocsp));
                }
            }
            if (crls != null) {
                foreach (byte[] crl in crls) {
                    vd.crls.Add(crl);
                }
            }
            if (certs != null) {
                foreach (byte[] cert in certs) {
                    vd.certs.Add(cert);
                }
            }
            validated[GetSignatureHashKey(signatureName)] = vd;
            return true;
        }

        private static byte[] BuildOCSPResponse(byte[] BasicOCSPResponse) {
            DerOctetString doctet = new DerOctetString(BasicOCSPResponse);
            Asn1EncodableVector v2 = new Asn1EncodableVector();
            v2.Add(OcspObjectIdentifiers.PkixOcspBasic);
            v2.Add(doctet);
            DerEnumerated den = new DerEnumerated(0);
            Asn1EncodableVector v3 = new Asn1EncodableVector();
            v3.Add(den);
            v3.Add(new DerTaggedObject(true, 0, new DerSequence(v2)));            
            DerSequence seq = new DerSequence(v3);
            return seq.GetEncoded();
        }

        private PdfName GetSignatureHashKey(String signatureName) {
            PdfDictionary dic = acroFields.GetSignatureDictionary(signatureName);
            PdfString contents = dic.GetAsString(PdfName.CONTENTS);
            byte[] bc = contents.GetOriginalBytes();
            byte[] bt = null;
            if (PdfName.ETSI_RFC3161.Equals(dic.GetAsName(PdfName.SUBFILTER))) {
                Asn1InputStream din = new Asn1InputStream(new MemoryStream(bc));
                Asn1Object pkcs = din.ReadObject();
                bc = pkcs.GetEncoded();
            }
            bt = HashBytesSha1(bc);
            return new PdfName(Utilities.ConvertToHex(bt));
        }
        
        private static byte[] HashBytesSha1(byte[] b) {
            return DigestAlgorithms.Digest("SHA1", b);
        }

        /**
         * Merges the validation with any validation already in the document or creates
         * a new one.
         * @throws IOException 
         */
        virtual public void Merge() {
            if (used || validated.Count == 0)
                return;
            used = true;
            PdfDictionary catalog = reader.Catalog;
            PdfObject dss = catalog.Get(PdfName.DSS);
            if (dss == null)
                CreateDss();
            else
                UpdateDss();
        }
        
        private void UpdateDss() {
            PdfDictionary catalog = reader.Catalog;
            stp.MarkUsed(catalog);
            PdfDictionary dss = catalog.GetAsDict(PdfName.DSS);
            PdfArray ocsps = dss.GetAsArray(PdfName.OCSPS);
            PdfArray crls = dss.GetAsArray(PdfName.CRLS);
            PdfArray certs = dss.GetAsArray(PdfName.CERTS);
            dss.Remove(PdfName.OCSPS);
            dss.Remove(PdfName.CRLS);
            dss.Remove(PdfName.CERTS);
            PdfDictionary vrim = dss.GetAsDict(PdfName.VRI);
            //delete old validations
            if (vrim != null) {
                foreach (PdfName n in vrim.Keys) {
                    if (validated.ContainsKey(n)) {
                        PdfDictionary vri = vrim.GetAsDict(n);
                        if (vri != null) {
                            DeleteOldReferences(ocsps, vri.GetAsArray(PdfName.OCSP));
                            DeleteOldReferences(crls, vri.GetAsArray(PdfName.CRL));
                            DeleteOldReferences(certs, vri.GetAsArray(PdfName.CERT));
                        }
                    }
                }
            }
            if (ocsps == null)
                ocsps = new PdfArray();
            if (crls == null)
                crls = new PdfArray();
            if (certs == null)
                certs = new PdfArray();
            OutputDss(dss, vrim, ocsps, crls, certs);
        }
        
        private static void DeleteOldReferences(PdfArray all, PdfArray toDelete) {
            if (all == null || toDelete == null)
                return;
            foreach (PdfObject pi in toDelete) {
                if (!pi.IsIndirect())
                    continue;
                PRIndirectReference pir = (PRIndirectReference)pi;
                for (int k = 0; k < all.Size; ++k) {
                    PdfObject po = all[k];
                    if (!po.IsIndirect())
                        continue;
                    PRIndirectReference pod = (PRIndirectReference)po;
                    if (pir.Number == pod.Number) {
                        all.Remove(k);
                        --k;
                    }
                }
            }
        }
        
        private void CreateDss() {
            OutputDss(new PdfDictionary(), new PdfDictionary(), new PdfArray(), new PdfArray(), new PdfArray());
        }
        
        private void OutputDss(PdfDictionary dss, PdfDictionary vrim, PdfArray ocsps, PdfArray crls, PdfArray certs) {
            writer.AddDeveloperExtension(PdfDeveloperExtension.ESIC_1_7_EXTENSIONLEVEL5);
            PdfDictionary catalog = reader.Catalog;
            stp.MarkUsed(catalog);
            foreach (PdfName vkey in validated.Keys) {
                PdfArray ocsp = new PdfArray();
                PdfArray crl = new PdfArray();
                PdfArray cert = new PdfArray();
                PdfDictionary vri = new PdfDictionary();
                foreach (byte[] b in validated[vkey].crls) {
                    PdfStream ps = new PdfStream(b);
                    ps.FlateCompress();
                    PdfIndirectReference iref = writer.AddToBody(ps, false).IndirectReference;
                    crl.Add(iref);
                    crls.Add(iref);
                }
                foreach (byte[] b in validated[vkey].ocsps) {
                    PdfStream ps = new PdfStream(b);
                    ps.FlateCompress();
                    PdfIndirectReference iref = writer.AddToBody(ps, false).IndirectReference;
                    ocsp.Add(iref);
                    ocsps.Add(iref);
                }
                foreach (byte[] b in validated[vkey].certs) {
                    PdfStream ps = new PdfStream(b);
                    ps.FlateCompress();
                    PdfIndirectReference iref = writer.AddToBody(ps, false).IndirectReference;
                    cert.Add(iref);
                    certs.Add(iref);
                }
                if (ocsp.Size > 0)
                    vri.Put(PdfName.OCSP, writer.AddToBody(ocsp, false).IndirectReference);
                if (crl.Size > 0)
                    vri.Put(PdfName.CRL, writer.AddToBody(crl, false).IndirectReference);
                if (cert.Size > 0)
                    vri.Put(PdfName.CERT, writer.AddToBody(cert, false).IndirectReference);
                vrim.Put(vkey, writer.AddToBody(vri, false).IndirectReference);
            }
            dss.Put(PdfName.VRI, writer.AddToBody(vrim, false).IndirectReference);
            if (ocsps.Size > 0)
                dss.Put(PdfName.OCSPS, writer.AddToBody(ocsps, false).IndirectReference);
            if (crls.Size > 0)
                dss.Put(PdfName.CRLS, writer.AddToBody(crls, false).IndirectReference);
            if (certs.Size > 0)
                dss.Put(PdfName.CERTS, writer.AddToBody(certs, false).IndirectReference);
            catalog.Put(PdfName.DSS, writer.AddToBody(dss, false).IndirectReference);
        }
        
        private class ValidationData {
            public IList<byte[]> crls = new List<byte[]>();
            public IList<byte[]> ocsps = new List<byte[]>();
            public IList<byte[]> certs = new List<byte[]>();
        }
    }
}
