using System;

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface TlsClientContext
	{
		SecureRandom SecureRandom { get; }

		SecurityParameters SecurityParameters { get; }

		object UserObject { get; set; }
	}
}
