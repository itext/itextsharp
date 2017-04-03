using System;
using System.IO;

namespace Org.BouncyCastle.Utilities.Encoders
{
	/**
	 * Encode and decode byte arrays (typically from binary to 7-bit ASCII
	 * encodings).
	 */
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IEncoder
	{
		int Encode(byte[] data, int off, int length, Stream outStream);

		int Decode(byte[] data, int off, int length, Stream outStream);

		int DecodeString(string data, Stream outStream);
	}
}
