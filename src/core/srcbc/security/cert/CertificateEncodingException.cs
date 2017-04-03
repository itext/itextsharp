using System;

namespace Org.BouncyCastle.Security.Certificates
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CertificateEncodingException : CertificateException
	{
		public CertificateEncodingException() : base() { }
		public CertificateEncodingException(string msg) : base(msg) { }
		public CertificateEncodingException(string msg, Exception e) : base(msg, e) { }
	}
}
