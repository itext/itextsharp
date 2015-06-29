/*
 * $Id: CRLVerifier.java 5465 2012-10-07 12:37:23Z blowagie $
 *
 * This file is part of the iText (R) project.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using iTextSharp.text.log;

/**
 * Class that allows you to verify a certificate against
 * one or more Certificate Revocation Lists.
 */
namespace iTextSharp.text.pdf.security {
	public class CrlVerifier : RootStoreVerifier {
        /** The Logger instance */
	    private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(CrlVerifier));
    	
	    /** The list of CRLs to check for revocation date. */
        
	    List<X509Crl> crls;
    	
	    /**
	     * Creates a CRLVerifier instance.
	     * @param verifier	the next verifier in the chain
	     * @param crls a list of CRLs
	     */
	    public CrlVerifier(CertificateVerifier verifier, List<X509Crl> crls) : base(verifier) {
		    this.crls = crls;
	    }
    	
	    /**
	     * Verifies if a a valid CRL is found for the certificate.
	     * If this method returns false, it doesn't mean the certificate isn't valid.
	     * It means we couldn't verify it against any CRL that was available.
	     * @param signCert	the certificate that needs to be checked
	     * @param issuerCert	its issuer
	     * @return a list of <code>VerificationOK</code> objects.
	     * The list will be empty if the certificate couldn't be verified.
	     * @see com.itextpdf.text.pdf.security.RootStoreVerifier#verify(java.security.cert.X509Certificate, java.security.cert.X509Certificate, java.util.Date)
	     */
	    override public List<VerificationOK> Verify(X509Certificate signCert, X509Certificate issuerCert, DateTime signDate) {
		    List<VerificationOK> result = new List<VerificationOK>();
		    int validCrlsFound = 0;
		    // first check the list of CRLs that is provided
		    if (crls != null) {
			    foreach (X509Crl crl in crls) {
				    if (Verify(crl, signCert, issuerCert, signDate))
					    validCrlsFound++;
			    }
		    }
		    // then check online if allowed
		    bool online = false;
		    if (onlineCheckingAllowed && validCrlsFound == 0) {
			    if (Verify(GetCrl(signCert, issuerCert), signCert, issuerCert, signDate)) {
				    validCrlsFound++;
				    online = true;
			    }
		    }
		    // show how many valid CRLs were found
		    LOGGER.Info("Valid CRLs found: " + validCrlsFound);
		    if (validCrlsFound > 0) {
			    result.Add(new VerificationOK(signCert, this, "Valid CRLs found: " + validCrlsFound + (online ? " (online)" : "")));
		    }
		    if (verifier != null)
			    result.AddRange(verifier.Verify(signCert, issuerCert, signDate));
		    // verify using the previous verifier in the chain (if any)
		    return result;
	    }

	    /**
	     * Verifies a certificate against a single CRL.
	     * @param crl	the Certificate Revocation List
	     * @param signCert	a certificate that needs to be verified
	     * @param issuerCert	its issuer
	     * @param signDate		the sign date
	     * @return true if the verification succeeded
	     * @throws GeneralSecurityException
	     */
	    virtual public bool Verify(X509Crl crl, X509Certificate signCert, X509Certificate issuerCert, DateTime signDate) {
		    if (crl == null || signDate == DateTime.MaxValue)
			    return false;
		    // We only check CRLs valid on the signing date for which the issuer matches
    	    if (crl.IssuerDN.Equals(signCert.IssuerDN)
			    && signDate.CompareTo(crl.ThisUpdate) > 0 && signDate.CompareTo(crl.NextUpdate.Value) < 0) {
			    // the signing certificate may not be revoked
			    if (IsSignatureValid(crl, issuerCert) && crl.IsRevoked(signCert)) {
				    throw new VerificationException(signCert, String.Format("{0} The certificate has been revoked.", signCert));
			    }
			    return true;
		    }
		    return false;
	    }
    	
	    /**
	     * Fetches a CRL for a specific certificate online (without further checking).
	     * @param signCert	the certificate
	     * @param issuerCert	its issuer
	     * @return	an X509CRL object
	     */
	    virtual public X509Crl GetCrl(X509Certificate signCert, X509Certificate issuerCert) {
		    try {
			    // gets the URL from the certificate
			    String crlurl = CertificateUtil.GetCRLURL(signCert);
			    if (crlurl == null)
				    return null;
			    LOGGER.Info("Getting CRL from " + crlurl);

                X509CrlParser crlParser = new X509CrlParser();
			    // Creates the CRL
		        Stream url = WebRequest.Create(crlurl).GetResponse().GetResponseStream();
			    return crlParser.ReadCrl(url);
		    }
		    catch (IOException) {
			    return null;
		    }
		    catch (GeneralSecurityException) {
			    return null;
		    }
	    }
    	
	    /**
	     * Checks if a CRL verifies against the issuer certificate or a trusted anchor.
	     * @param crl	the CRL
	     * @param crlIssuer	the trusted anchor
	     * @return	true if the CRL can be trusted
	     */
	    virtual public bool IsSignatureValid(X509Crl crl, X509Certificate crlIssuer) {
		    // check if the CRL was issued by the issuer
		    if (crlIssuer != null) {
			    try {
				    crl.Verify(crlIssuer.GetPublicKey());
				    return true;
			    } catch (GeneralSecurityException) {
				    LOGGER.Warn("CRL not issued by the same authority as the certificate that is being checked");
			    }
		    }
		    // check the CRL against trusted anchors
		    if (certificates == null)
			    return false;
		    try {
			    // loop over the certificate in the key store
        	    foreach (X509Certificate anchor in certificates) {
                    try {
                        crl.Verify(anchor.GetPublicKey());
	                    return true;
                    } catch (GeneralSecurityException) {}
        	    }
		    }
            catch (GeneralSecurityException) {
        	    return false;
            }
		    return false;
	    }
	}
}
