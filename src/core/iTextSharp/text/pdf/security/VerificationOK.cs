using System;
using System.Text;
using Org.BouncyCastle.X509;

/**
 * Class that informs you that the verification of a Certificate
 * succeeded using a specific CertificateVerifier and for a specific
 * reason.
 */
namespace iTextSharp.text.pdf.security {
	public class VerificationOK {

        /** The certificate that was verified successfully. */
	    protected X509Certificate certificate;
	    /** The CertificateVerifier that was used for verifying. */
	    protected CertificateVerifier verifierClass;
	    /** The reason why the certificate verified successfully. */
	    protected String message;
    	
	    /**
	     * Creates a VerificationOK object
	     * @param certificate	the certificate that was successfully verified
	     * @param verifierClass	the class that was used for verification
	     * @param message		the reason why the certificate could be verified
	     */
	    public VerificationOK(X509Certificate cert, CertificateVerifier verifierClass, String message) {
		    certificate = cert;
		    this.verifierClass = verifierClass;
		    this.message = message;
	    }
    	
	    /**
	     * A single String explaining which certificate was verified, how and why.
	     * @see java.lang.Object#toString()
	     */
	    public override String ToString() {
		    StringBuilder sb = new StringBuilder();
		    if (certificate != null) {
			    sb.Append(certificate.SubjectDN);
			    sb.Append(" verified with ");
		    }
		    sb.Append(verifierClass.GetType().Name);
		    sb.Append(": ");
		    sb.Append(message);
		    return sb.ToString();
	    }
	}
}
