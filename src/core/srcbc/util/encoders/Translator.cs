using System;

namespace Org.BouncyCastle.Utilities.Encoders
{
    /// <summary>
    /// Translator interface.
    /// </summary>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface ITranslator
    {
        int GetEncodedBlockSize();

        int Encode(byte[] input, int inOff, int length, byte[] outBytes, int outOff);

        int GetDecodedBlockSize();

        int Decode(byte[] input, int inOff, int length, byte[] outBytes, int outOff);
    }

}
