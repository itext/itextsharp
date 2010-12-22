namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 2246 6.1
	/// </summary>
    public enum CompressionMethod : byte
	{
		NULL = 0,

		/*
		 * RFC 3749 2
		 */
		DEFLATE = 1
	}
}
