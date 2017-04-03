using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum EncryptionAlgorithm
	{
		/*
		 * Note that the values here are implementation-specific and arbitrary.
		 * It is recommended not to depend on the particular values (e.g. serialization).
		 */
		NULL,
		RC4_40,
		RC4_128,
		RC2_CBC_40,
		IDEA_CBC,
		DES40_CBC,
		DES_CBC,
		cls_3DES_EDE_CBC,

		/*
		 * RFC 3268
		 */
		AES_128_CBC,
		AES_256_CBC,

		/*
		 * RFC 5289
		 */
		AES_128_GCM,
		AES_256_GCM,
	}
}
