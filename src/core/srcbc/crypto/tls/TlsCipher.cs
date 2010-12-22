using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal interface TlsCipher
	{
		byte[] EncodePlaintext(ContentType type, byte[] plaintext, int offset, int len);// throws IOException;

		byte[] DecodeCiphertext(ContentType type, byte[] ciphertext, int offset, int len);// throws IOException;
	}
}
