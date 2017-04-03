using System;

namespace Org.BouncyCastle.Security.Certificates
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CertificateException : GeneralSecurityException
	{
		public CertificateException() : base() { }
		public CertificateException(string message) : base(message) { }
		public CertificateException(string message, Exception exception) : base(message, exception) { }
	}
}
