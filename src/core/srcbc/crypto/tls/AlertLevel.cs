using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 7.2
	/// </summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum AlertLevel : byte
	{
	    warning = 1,
	    fatal = 2,
	}
}
