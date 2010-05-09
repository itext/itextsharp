using System;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal interface TlsCipher
	{
		byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len);// throws IOException;

		byte[] DecodeCiphertext(short type, byte[] ciphertext, int offset, int len);// throws IOException;
	}
}
