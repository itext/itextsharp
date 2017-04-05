using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Generic exception class for PGP encoding/decoding problems.</remarks>
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PgpException
		: Exception
	{
		public PgpException() : base() {}
		public PgpException(string message) : base(message) {}
		public PgpException(string message, Exception exception) : base(message, exception) {}

		[Obsolete("Use InnerException property")]
		public Exception UnderlyingException
		{
			get { return InnerException; }
		}
	}
}
