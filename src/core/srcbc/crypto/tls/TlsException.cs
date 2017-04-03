using System;

namespace Org.BouncyCastle.Crypto.Tls
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class TlsException : Exception
	{
		public TlsException() : base() { }
		public TlsException(string message) : base(message) { }
		public TlsException(string message, Exception exception) : base(message, exception) { }
	}
}
