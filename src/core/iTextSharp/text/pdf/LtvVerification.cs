using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities;
using iTextSharp.text.error_messages;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.pdf {

    /**
     * Add verification according to PAdES-LTV (part 4)
     * @author psoares
     */
    public class LtvVerification {
        private PdfStamperImp writer;
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
         * The verification constructor
         * @param stp the PdfStamper to apply the validation to
         */
        internal LtvVerification(PdfStamper stp) {
            writer = (PdfStamperImp)stp.Writer;
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
        public bool AddVerification(String signatureName, IOcspClient ocsp, ICrlClient crl, CertificateOption certOption, Level level, CertificateInclusion certInclude) {
            if (used)
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("verification.already.output"));
            PdfPKCS7 pk = acroFields.VerifySignature(signatureName);
            X509Certificate[] xc = pk.SignCertificateChain;
            ValidationData vd = new ValidationData();
            for (int k = 0; k < xc.Length; ++k) {
                byte[] ocspEnc = null;
                if (ocsp != null && level != Level.CRL && k < xc.Length - 1) {
                    ocspEnc = ocsp.GetEncoded(xc[k], xc[k + 1], null);
                    if (ocspEnc != null)
                        vd.ocsps.Add(ocspEnc);
                }
                if (crl != null && (level != Level.OCSP || (level == Level.OCSP_OPTIONAL_CRL && ocspEnc == null))) {
                    byte[] cim = crl.GetEncoded(xc[k], null);
                    if (cim != null) {
                        bool dup = false;
                        foreach (byte[] b in vd.crls) {
                            if (Arrays.AreEqual(b, cim)) {
                                dup = true;
                                break;
                            }
                        }
                        if (!dup)
                            vd.crls.Add(cim);
                    }
                }
                if (certOption == CertificateOption.SIGNING_CERTIFICATE)
                    break;
            }
            if (vd.crls.Count == 0 && vd.ocsps.Count == 0)
                return false;
            if (certInclude == CertificateInclusion.YES) {
                foreach (X509Certificate c in xc) {
                    vd.certs.Add(c.GetEncoded());
                }
            }
            validated[GetSignatureHashKey(signatureName)] = vd;
            return true;
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
            return new PdfName(ConvertToHex(bt));
        }

        private static String ConvertToHex(byte[] bt) {
            ByteBuffer buf = new ByteBuffer();
            foreach (byte b in bt) {
                buf.AppendHex(b);
            }
            return PdfEncodings.ConvertToString(buf.ToByteArray(), null).ToUpperInvariant();
        }
        
        private static byte[] HashBytesSha1(byte[] b) {
            return PdfEncryption.DigestComputeHash("SHA1", b);
        }

        /**
         * Merges the validation with any validation already in the document or creates
         * a new one.
         * @throws IOException 
         */
        void Merge() {
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
            writer.MarkUsed(catalog);
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
            PdfDictionary catalog = reader.Catalog;
            writer.MarkUsed(catalog);
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