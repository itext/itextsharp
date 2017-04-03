using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 6.1
	/// </summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum CompressionMethod : byte
	{
		NULL = 0,

		/*
		 * RFC 3749 2
		 */
		DEFLATE = 1

		/*
		 * Values from 224 decimal (0xE0) through 255 decimal (0xFF)
		 * inclusive are reserved for private use.
		 */
	}
}
