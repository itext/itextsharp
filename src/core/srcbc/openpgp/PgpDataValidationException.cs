using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
	/// Thrown if the IV at the start of a data stream indicates the wrong key is being used.
	/// </remarks>
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PgpDataValidationException
        : PgpException
	{
		public PgpDataValidationException() : base() {}
		public PgpDataValidationException(string message) : base(message) {}
		public PgpDataValidationException(string message, Exception exception) : base(message, exception) {}
	}
}
