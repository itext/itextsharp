using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 7.4.4
	/// </summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum ClientCertificateType : byte
	{
		rsa_sign = 1,
		dss_sign = 2,
		rsa_fixed_dh = 3,
		dss_fixed_dh = 4,

		/*
		 * RFC 4492 5.5
		 */
		ecdsa_sign = 64,
		rsa_fixed_ecdh = 65,
		ecdsa_fixed_ecdh = 66,
	}
}