using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 7.4
	/// </summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum HandshakeType : byte
	{
		hello_request = 0,
		client_hello = 1,
		server_hello = 2,
		certificate = 11,
		server_key_exchange = 12,
		certificate_request = 13,
		server_hello_done = 14,
		certificate_verify = 15,
		client_key_exchange = 16,
		finished = 20,
	}
}
