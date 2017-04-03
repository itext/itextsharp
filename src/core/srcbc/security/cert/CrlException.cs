using System;

namespace Org.BouncyCastle.Security.Certificates
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CrlException : GeneralSecurityException
	{
		public CrlException() : base() { }
		public CrlException(string msg) : base(msg) {}
		public CrlException(string msg, Exception e) : base(msg, e) {}
	}
}
