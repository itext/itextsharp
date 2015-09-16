/*
 * $Id: LtvVerifier.java 5472 2012-10-07 17:08:45Z blowagie $
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
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using iTextSharp.text.log;

/**
 * Verifies the signatures in an LTV document.
 */
namespace iTextSharp.text.pdf.security {
	public class LtvVerifier : RootStoreVerifier {
        /** The Logger instance */
	    private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(LtvVerifier));
    	
	    /** Do we need to check all certificate, or only the signing certificate? */
	    protected LtvVerification.CertificateOption option = LtvVerification.CertificateOption.SIGNING_CERTIFICATE;
	    /** Verify root. */
	    protected bool verifyRootCertificate = true;
    	
	    /** A reader object for the revision that is being verified. */
	    protected PdfReader reader;
	    /** The fields in the revision that is being verified. */
	    protected AcroFields fields;
	    /** The date the revision was signed, or <code>null</code> for the highest revision. */
	    protected DateTime signDate;
	    /** The signature that covers the revision. */
	    protected String signatureName;
	    /** The PdfPKCS7 object for the signature. */
	    protected PdfPKCS7 pkcs7;
	    /** Indicates if we're working with the latest revision. */
	    protected bool latestRevision = true;
	    /** The document security store for the revision that is being verified */
	    protected PdfDictionary dss;
    	
	    /**
	     * Creates a VerificationData object for a PdfReader
	     * @param reader	a reader for the document we want to verify.
	     * @throws GeneralSecurityException 
	     */
	    public LtvVerifier(PdfReader reader) : base(null) {
		    this.reader = reader;
		    fields = reader.AcroFields;
		    List<String> names = fields.GetSignatureNames();
		    signatureName = names[names.Count - 1];
		    signDate = DateTime.Now;
		    pkcs7 = CoversWholeDocument();
		    LOGGER.Info(String.Format("Checking {0}signature {1}", pkcs7.IsTsp ? "document-level timestamp " : "", signatureName));
	    }
    	
	    /**
	     * Sets an extra verifier.
	     * @param verifier the verifier to set
	     */
	    virtual public CertificateVerifier Verifier {
            set { verifier = value; }
	    }
    	
	    /**
	     * Sets the certificate option.
	     * @param	option	Either CertificateOption.SIGNING_CERTIFICATE (default) or CertificateOption.WHOLE_CHAIN
	     */
	    virtual public LtvVerification.CertificateOption CertificateOption {
            set { option = value; }
	    }
    	
	    /**
	     * Set the verifyRootCertificate to false if you can't verify the root certificate.
	     */
	    virtual public bool VerifyRootCertificate {
            set { verifyRootCertificate = value; }
	    }
    	
	    /**
	     * Checks if the signature covers the whole document
	     * and throws an exception if the document was altered
	     * @return a PdfPKCS7 object
	     * @throws GeneralSecurityException
	     */
	    virtual protected PdfPKCS7 CoversWholeDocument() {
		    PdfPKCS7 pkcs7 = fields.VerifySignature(signatureName);
		    if (fields.SignatureCoversWholeDocument(signatureName))
			    LOGGER.Info("The timestamp covers whole document.");
		    else throw new VerificationException(null, "Signature doesn't cover whole document.");
		    if (pkcs7.Verify()) {
			    LOGGER.Info("The signed document has not been modified.");
			    return pkcs7;
		    }
		    throw new VerificationException(null, "The document was altered after the signature was applied.");
	    }
    	
	    /**
	     * Verifies all the document-level timestamps and all the signatures in the document.
	     * @throws IOException
	     * @throws GeneralSecurityException
	     */
	    virtual public List<VerificationOK> Verify(List<VerificationOK> result) {
		    if (result == null)
			    result = new List<VerificationOK>();
		    while (pkcs7 != null) {
			    result.AddRange(VerifySignature());
		    }
		    return result;
	    }
    	
	    /**
	     * Verifies a document level timestamp.
	     * @throws GeneralSecurityException
	     * @throws IOException
	     */
	    virtual public List<VerificationOK> VerifySignature() {
            LOGGER.Info("Verifying signature.");
            List<VerificationOK> result = new List<VerificationOK>();
		    // Get the certificate chain
		    X509Certificate[] chain = pkcs7.SignCertificateChain;
		    VerifyChain(chain);
		    // how many certificates in the chain do we need to check?
		    int total = 1;
		    if (LtvVerification.CertificateOption.WHOLE_CHAIN.Equals(option)) {
			    total = chain.Length;
		    }
		    // loop over the certificates
		    X509Certificate signCert;
		    X509Certificate issuerCert;
		    for (int i = 0; i < total; ) {
			    // the certificate to check
			    signCert = chain[i];
			    // its issuer
			    issuerCert = null;
			    if (++i < chain.Length)
				    issuerCert = chain[i];
			    // now lets verify the certificate
			    LOGGER.Info(signCert.SubjectDN.ToString());
			    List<VerificationOK> list = Verify(signCert, issuerCert, signDate);
			    if (list.Count == 0) {
				    try {
					    signCert.Verify(signCert.GetPublicKey()); 
					    if (latestRevision && chain.Length > 1) {
						    list.Add(new VerificationOK(signCert, this, "Root certificate in revision"));
					    }
					    if (list.Count == 0 && verifyRootCertificate)
						    throw new GeneralSecurityException();
					    if (chain.Length > 1)
						    list.Add(new VerificationOK(signCert, this, "Root certificate passed without checking"));
				    }
				    catch (GeneralSecurityException) {
					    throw new VerificationException(signCert, "Couldn't verify with CRL or OCSP or trusted anchor");
				    }
			    }
			    result.AddRange(list);
		    }
		    // go to the previous revision
		    SwitchToPreviousRevision();
		    return result;
	    }

	    /**
	     * Checks the certificates in a certificate chain:
	     * are they valid on a specific date, and
	     * do they chain up correctly?
	     * @param chain
	     * @throws GeneralSecurityException
	     */
	    virtual public void VerifyChain(X509Certificate[] chain) {
		    // Loop over the certificates in the chain
		    for (int i = 0; i < chain.Length; ++i) {
			    X509Certificate cert = chain[i];
			    // check if the certificate was/is valid
			    cert.CheckValidity(signDate);
			    // check if the previous certificate was issued by this certificate
			    if (i > 0)
				    chain[i-1].Verify(chain[i].GetPublicKey());
		    }
            LOGGER.Info("All certificates are valid on " + signDate.ToString("F"));
	    }
    	
	    /**
	     * Verifies certificates against a list of CRLs and OCSP responses.
	     * @param signingCert
	     * @param issuerCert
	     * @return a list of <code>VerificationOK</code> objects.
	     * The list will be empty if the certificate couldn't be verified.
	     * @throws GeneralSecurityException
	     * @throws IOException
	     * @see com.itextpdf.text.pdf.security.RootStoreVerifier#verify(java.security.cert.X509Certificate, java.security.cert.X509Certificate)
	     */
	    override public List<VerificationOK> Verify(X509Certificate signCert, X509Certificate issuerCert, DateTime sigDate) {
		    // we'll verify agains the rootstore (if present)
		    RootStoreVerifier rootStoreVerifier = new RootStoreVerifier(verifier);
		    rootStoreVerifier.Certificates = certificates;
		    // We'll verify against a list of CRLs
		    CrlVerifier crlVerifier = new CrlVerifier(rootStoreVerifier, GetCRLsFromDSS());
		    crlVerifier.Certificates = certificates;
		    crlVerifier.OnlineCheckingAllowed = latestRevision || onlineCheckingAllowed;
		    // We'll verify against a list of OCSPs
		    OcspVerifier ocspVerifier = new OcspVerifier(crlVerifier, GetOCSPResponsesFromDSS());
		    ocspVerifier.Certificates = certificates;
		    ocspVerifier.OnlineCheckingAllowed = latestRevision || onlineCheckingAllowed;
		    // We verify the chain
		    return ocspVerifier.Verify(signCert, issuerCert, sigDate);
	    }
    	
	    /**
	     * Switches to the previous revision.
	     * @throws IOException
	     * @throws GeneralSecurityException 
	     */
	    virtual public void SwitchToPreviousRevision() {
		    LOGGER.Info("Switching to previous revision.");
		    latestRevision = false;
		    dss = reader.Catalog.GetAsDict(PdfName.DSS);
		    DateTime cal = pkcs7.TimeStampDate;
		    if (cal == DateTime.MaxValue)
			    cal = pkcs7.SignDate;
		    // TODO: get date from signature
	        signDate = cal;
		    List<String> names = fields.GetSignatureNames();
		    if (names.Count > 1) {
			    signatureName = names[names.Count - 2];
			    reader = new PdfReader(fields.ExtractRevision(signatureName));
			    fields = reader.AcroFields;
			    names = fields.GetSignatureNames();
			    signatureName = names[names.Count - 1];
			    pkcs7 = CoversWholeDocument();
			    LOGGER.Info(String.Format("Checking {0}signature {1}", pkcs7.IsTsp ? "document-level timestamp " : "", signatureName));
		    }
		    else {
			    LOGGER.Info("No signatures in revision");
			    pkcs7 = null;
		    }
	    }
    	
	    /**
	     * Gets a list of X509CRL objects from a Document Security Store.
	     * @return	a list of CRLs
	     * @throws GeneralSecurityException
	     * @throws IOException
	     */
	    virtual public List<X509Crl> GetCRLsFromDSS() {
		    List<X509Crl> crls = new List<X509Crl>();
		    if (dss == null)
			    return crls;
		    PdfArray crlarray = dss.GetAsArray(PdfName.CRLS);
		    if (crlarray == null)
			    return crls;
            X509CrlParser crlParser = new X509CrlParser();
		    for (int i = 0; i < crlarray.Size; ++i) {
			    PRStream stream = (PRStream) crlarray.GetAsStream(i);
			    X509Crl crl = crlParser.ReadCrl(new MemoryStream(PdfReader.GetStreamBytes(stream)));
			    crls.Add(crl);
		    }
		    return crls;
	    }
    	
	    /**
	     * Gets OCSP responses from the Document Security Store.
	     * @return	a list of BasicOCSPResp objects
	     * @throws IOException
	     * @throws GeneralSecurityException
	     */
	    virtual public List<BasicOcspResp> GetOCSPResponsesFromDSS() {
		    List<BasicOcspResp> ocsps = new List<BasicOcspResp>();
		    if (dss == null)
			    return ocsps;
		    PdfArray ocsparray = dss.GetAsArray(PdfName.OCSPS);
		    if (ocsparray == null)
			    return ocsps;
		    for (int i = 0; i < ocsparray.Size; i++) {
			    PRStream stream = (PRStream) ocsparray.GetAsStream(i);
			    OcspResp ocspResponse = new OcspResp(PdfReader.GetStreamBytes(stream));
			    if (ocspResponse.Status == 0)
				    try {
					    ocsps.Add((BasicOcspResp) ocspResponse.GetResponseObject());
				    } catch (OcspException e) {
					    throw new GeneralSecurityException(e.ToString());
				    }
		    }
		    return ocsps;
	    }
	}
}
