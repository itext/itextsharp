/*
 * $Id: OCSPVerifier.java 5465 2012-10-07 12:37:23Z blowagie $
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

using System;
using System.Collections.Generic;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using iTextSharp.text.log;
using Org.BouncyCastle.Utilities.Date;

/**
 * Class that allows you to verify a certificate against
 * one or more OCSP responses.
 */
namespace iTextSharp.text.pdf.security {
	public class OcspVerifier : RootStoreVerifier {
        /** The Logger instance */
        private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(OcspVerifier));
    	
	    /** The list of OCSP responses. */
	    protected List<BasicOcspResp> ocsps;
    	
	    /**
	     * Creates an OCSPVerifier instance.
	     * @param verifier	the next verifier in the chain
	     * @param ocsps a list of OCSP responses
	     */
	    public OcspVerifier(CertificateVerifier verifier, List<BasicOcspResp> ocsps) : base(verifier) {
		    this.ocsps = ocsps;
	    }

	    /**
	     * Verifies if a a valid OCSP response is found for the certificate.
	     * If this method returns false, it doesn't mean the certificate isn't valid.
	     * It means we couldn't verify it against any OCSP response that was available.
	     * @param signCert	the certificate that needs to be checked
	     * @param issuerCert	its issuer
	     * @return a list of <code>VerificationOK</code> objects.
	     * The list will be empty if the certificate couldn't be verified.
	     * @see com.itextpdf.text.pdf.security.RootStoreVerifier#verify(java.security.cert.X509Certificate, java.security.cert.X509Certificate, java.util.Date)
	     */
	    override public List<VerificationOK> Verify(X509Certificate signCert, X509Certificate issuerCert, DateTime signDate) {
		    List<VerificationOK> result = new List<VerificationOK>();
		    int validOCSPsFound = 0;
		    // first check in the list of OCSP responses that was provided
		    if (ocsps != null) {
			    foreach (BasicOcspResp ocspResp in ocsps) {
				    if (Verify(ocspResp, signCert, issuerCert, signDate))
					    validOCSPsFound++;
			    }
		    }
		    // then check online if allowed
		    bool online = false;
		    if (onlineCheckingAllowed && validOCSPsFound == 0) {
			    if (Verify(GetOcspResponse(signCert, issuerCert), signCert, issuerCert, signDate)) {
				    validOCSPsFound++;
				    online = true;
			    }
		    }
		    // show how many valid OCSP responses were found
		    LOGGER.Info("Valid OCSPs found: " + validOCSPsFound);
		    if (validOCSPsFound > 0)
			    result.Add(new VerificationOK(signCert, this, "Valid OCSPs Found: " + validOCSPsFound + (online ? " (online)" : "")));
		    if (verifier != null)
			    result.AddRange(verifier.Verify(signCert, issuerCert, signDate));
		    // verify using the previous verifier in the chain (if any)
		    return result;
	    }
    	
    	
	    /**
	     * Verifies a certificate against a single OCSP response
	     * @param ocspResp	the OCSP response
	     * @param issuerCert
	     * @param signDate
	     * @return
	     * @throws GeneralSecurityException
	     * @throws IOException
	     */
	    public bool Verify(BasicOcspResp ocspResp, X509Certificate signCert, X509Certificate issuerCert, DateTime signDate) {
		    if (ocspResp == null)
			    return false;
		    // Getting the responses
		    SingleResp[] resp = ocspResp.Responses;
		    for (int i = 0; i < resp.Length; ++i) {
			    // check if the serial number corresponds
			    if (!signCert.SerialNumber.Equals(resp[i].GetCertID().SerialNumber))
				    continue;
			    // check if the issuer matches
			    try {
                    if (issuerCert == null) issuerCert = signCert;
                    if (!resp[i].GetCertID().MatchesIssuer(issuerCert)) {
					    LOGGER.Info("OCSP: Issuers doesn't match.");
					    continue;
				    }
			    } catch (OcspException) {
				    continue;
			    }
			    // check if the OCSP response was valid at the time of signing
                DateTimeObject nextUpdate = resp[i].NextUpdate;
                DateTime nextUpdateDate;
                if (nextUpdate == null) {
                    nextUpdateDate = resp[i].ThisUpdate.AddSeconds(180);
                    LOGGER.Info("No 'next update' for OCSP Response; assuming " + nextUpdateDate);
                }
                else
                    nextUpdateDate = nextUpdate.Value;
                if (signDate > nextUpdateDate) {
                    LOGGER.Info(String.Format("OCSP no longer valid: {0} after {1}", signDate, nextUpdateDate));
                    continue;
                }
			    // check the status of the certificate
			    Object status = resp[i].GetCertStatus();
			    if (status == CertificateStatus.Good) {
				    // check if the OCSP response was genuine
				    IsValidResponse(ocspResp, issuerCert);
				    return true;
			    }
		    }
		    return false;
	    }
    	
	    /**
	     * Verifies if an OCSP response is genuine
	     * @param ocspResp	the OCSP response
	     * @param issuerCert	the issuer certificate
	     * @throws GeneralSecurityException
	     * @throws IOException
	     */
	    public void IsValidResponse(BasicOcspResp ocspResp, X509Certificate issuerCert) {
		    // by default the OCSP responder certificate is the issuer certificate
		    X509Certificate responderCert = issuerCert;
		    // check if there's a responder certificate
		    X509Certificate[] certs = ocspResp.GetCerts();
		    if (certs.Length > 0) {
			    responderCert = certs[0];
			    try {
				    responderCert.Verify(issuerCert.GetPublicKey());
			    }
			    catch (GeneralSecurityException) {
				    if (base.Verify(responderCert, issuerCert, DateTime.MaxValue).Count == 0)
                        throw new VerificationException(responderCert, String.Format("{0} Responder certificate couldn't be verified", responderCert));
			    }
		    }
		    // verify if the signature of the response is valid
		    if (!VerifyResponse(ocspResp, responderCert))
                throw new VerificationException(responderCert, String.Format("{0} OCSP response could not be verified", responderCert));
	    }
    	
	    /**
	     * Verifies if the signature of the response is valid.
	     * If it doesn't verify against the responder certificate, it may verify
	     * using a trusted anchor.
	     * @param ocspResp	the response object
	     * @param responderCert	the certificate that may be used to sign the response
	     * @return	true if the response can be trusted
	     */
	    public bool VerifyResponse(BasicOcspResp ocspResp, X509Certificate responderCert) {
		    // testing using the responder certificate
		    if (IsSignatureValid(ocspResp, responderCert))
			    return true;
		    // testing using trusted anchors
		    if (certificates == null)
			    return false;
		    try {
			    // loop over the certificates in the root store
        	    foreach (X509Certificate anchor in certificates) {
                    try {
                        if (IsSignatureValid(ocspResp, anchor))
	                        return true;
				    } catch (GeneralSecurityException) {}
        	    }
		    }
            catch (GeneralSecurityException) {
        	    return false;
            }
		    return false;
	    }
    	
	    /**
	     * Checks if an OCSP response is genuine
	     * @param ocspResp	the OCSP response
	     * @param responderCert	the responder certificate
	     * @return	true if the OCSP response verifies against the responder certificate
	     */
        public bool IsSignatureValid(BasicOcspResp ocspResp, X509Certificate responderCert) {
		    try {
			    return ocspResp.Verify(responderCert.GetPublicKey());
		    } catch (OcspException) {
			    return false;
		    }
	    }
    	
	    /**
	     * Gets an OCSP response online and returns it if the status is GOOD
	     * (without further checking).
	     * @param signCert	the signing certificate
	     * @param issuerCert	the issuer certificate
	     * @return an OCSP response
	     */
	    public BasicOcspResp GetOcspResponse(X509Certificate signCert, X509Certificate issuerCert) {
		    if (signCert == null && issuerCert == null) {
			    return null;
		    }
		    OcspClientBouncyCastle ocsp = new OcspClientBouncyCastle();
		    BasicOcspResp ocspResp = ocsp.GetBasicOCSPResp(signCert, issuerCert, null);
		    if (ocspResp == null) {
			    return null;
		    }
		    SingleResp[] resp = ocspResp.Responses;
		    for (int i = 0; i < resp.Length; ++i) {
			    Object status = resp[i].GetCertStatus();
			    if (status == CertificateStatus.Good) {
				    return ocspResp;
			    }
		    }
		    return null;
	    }
	}
}
