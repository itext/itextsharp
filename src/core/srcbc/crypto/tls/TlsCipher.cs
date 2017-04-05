using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface TlsCipher
	{
		/// <exception cref="IOException"></exception>
		byte[] EncodePlaintext(ContentType type, byte[] plaintext, int offset, int len);

		/// <exception cref="IOException"></exception>
		byte[] DecodeCiphertext(ContentType type, byte[] ciphertext, int offset, int len);
	}
}
