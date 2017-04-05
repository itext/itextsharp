using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
	/// Thrown if the key checksum is invalid.
	/// </remarks>
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PgpKeyValidationException
		: PgpException
	{
		public PgpKeyValidationException() : base() {}
		public PgpKeyValidationException(string message) : base(message) {}
		public PgpKeyValidationException(string message, Exception exception) : base(message, exception) {}
	}
}
