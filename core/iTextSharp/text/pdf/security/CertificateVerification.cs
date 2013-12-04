using System;
using System.Collections.Generic;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
/*
 * $Id: CertificateVerification.cs 475 2012-11-12 09:25:23Z eugenemark $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
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
     * This class consists of some methods that allow you to verify certificates.
     */
    public static class CertificateVerification {

        /**
         * Verifies a single certificate.
         * @param cert the certificate to verify
         * @param crls the certificate revocation list or <CODE>null</CODE>
         * @param calendar the date or <CODE>null</CODE> for the current date
         * @return a <CODE>String</CODE> with the error description or <CODE>null</CODE>
         * if no error
         */
        public static String VerifyCertificate(X509Certificate cert, ICollection<X509Crl> crls, DateTime calendar) {
            foreach (String oid in cert.GetCriticalExtensionOids()) {
                if (oid == X509Extensions.KeyUsage.Id
                    || oid == X509Extensions.CertificatePolicies.Id
                    || oid == X509Extensions.PolicyMappings.Id
                    || oid == X509Extensions.InhibitAnyPolicy.Id
                    || oid == X509Extensions.CrlDistributionPoints.Id
                    || oid == X509Extensions.IssuingDistributionPoint.Id
                    || oid == X509Extensions.DeltaCrlIndicator.Id
                    || oid == X509Extensions.PolicyConstraints.Id
                    || oid == X509Extensions.BasicConstraints.Id
                    || oid == X509Extensions.SubjectAlternativeName.Id
                    || oid == X509Extensions.NameConstraints.Id) {
                    continue;
                }
                try {
                    // EXTENDED KEY USAGE and TIMESTAMPING is ALLOWED
                    if (oid == X509Extensions.ExtendedKeyUsage.Id && cert.GetExtendedKeyUsage().Contains("1.3.6.1.5.5.7.3.8")) {
                        continue;
                    }
                }
                catch (CertificateParsingException) {
                    // DO NOTHING;
                }
                return "Has unsupported critical extension";
            }

            try {
                if (!cert.IsValid(calendar.ToUniversalTime()))
                    return "The certificate has expired or is not yet valid";
                if (crls != null) {
                    foreach (X509Crl crl in crls) {
                        if (crl.IsRevoked(cert))
                            return "Certificate revoked";
                    }
                }
            }
            catch (Exception e) {
                return e.ToString();
            }
            return null;
        }

        /**
         * Verifies a certificate chain against a KeyStore.
         * @param certs the certificate chain
         * @param keystore the <CODE>KeyStore</CODE>
         * @param crls the certificate revocation list or <CODE>null</CODE>
         * @param calendar the date or <CODE>null</CODE> for the current date
         * @return <CODE>null</CODE> if the certificate chain could be validated or a
         * <CODE>Object[]{cert,error}</CODE> where <CODE>cert</CODE> is the
         * failed certificate and <CODE>error</CODE> is the error message
         */
        public static IList<VerificationException> VerifyCertificates(ICollection<X509Certificate> certs, ICollection<X509Certificate> keystore, ICollection<X509Crl> crls, DateTime calendar) {
            IList<VerificationException> result = new List<VerificationException>();
            X509Certificate[] certArray = new X509Certificate[certs.Count];
            certs.CopyTo(certArray, 0);
            for (int k = 0; k < certArray.Length; ++k) {
                X509Certificate cert = certArray[k];
                String err = VerifyCertificate(cert, crls, calendar);
                if (err != null)
                    result.Add(new VerificationException(cert, err));
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        if (VerifyCertificate(certStoreX509, crls, calendar) != null)
                            continue;
                        try {
                            cert.Verify(certStoreX509.GetPublicKey());
                            return result;
                        }
                        catch {
                            continue;
                        }
                    }
                    catch {
                    }
                }
                int j;
                for (j = 0; j < certArray.Length; ++j) {
                    if (j == k)
                        continue;
                    X509Certificate certNext = certArray[j];
                    try {
                        cert.Verify(certNext.GetPublicKey());
                        break;
                    }
                    catch {
                    }
                }
                if (j == certArray.Length)
                    result.Add(new VerificationException(cert, "Cannot be verified against the KeyStore or the certificate chain"));
            }
            if (result.Count == 0)
                result.Add(new VerificationException(null, "Invalid state. Possible circular certificate chain"));
            return result;
        }

        /**
	     * Verifies a certificate chain against a KeyStore.
	     * @param certs the certificate chain
	     * @param keystore the <CODE>KeyStore</CODE>
	     * @param calendar the date or <CODE>null</CODE> for the current date
	     * @return <CODE>null</CODE> if the certificate chain could be validated or a
	     * <CODE>Object[]{cert,error}</CODE> where <CODE>cert</CODE> is the
	     * failed certificate and <CODE>error</CODE> is the error message
	     */
        public static IList<VerificationException> VerifyCertificates(ICollection<X509Certificate> certs, ICollection<X509Certificate> keystore, DateTime calendar) {

	        return VerifyCertificates(certs, keystore, null, calendar);
	    }
    
        /**
         * Verifies an OCSP response against a KeyStore.
         * @param ocsp the OCSP response
         * @param keystore the <CODE>KeyStore</CODE>
         * @param provider the provider or <CODE>null</CODE> to use the BouncyCastle provider
         * @return <CODE>true</CODE> is a certificate was found
         */
        public static bool VerifyOcspCertificates(BasicOcspResp ocsp, ICollection<X509Certificate> keystore) {
            try {
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        if (ocsp.Verify(certStoreX509.GetPublicKey()))
                            return true;
                    }
                    catch {
                    }
                }
            }
            catch {
            }
            return false;
        }

        /**
         * Verifies a time stamp against a KeyStore.
         * @param ts the time stamp
         * @param keystore the <CODE>KeyStore</CODE>
         * @param provider the provider or <CODE>null</CODE> to use the BouncyCastle provider
         * @return <CODE>true</CODE> is a certificate was found
         */
        public static bool VerifyTimestampCertificates(TimeStampToken ts, ICollection<X509Certificate> keystore) {
            try {
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        ts.Validate(certStoreX509);
                        return true;
                    }
                    catch {
                    }
                }
            }
            catch {
            }
            return false;
        }
    }
}