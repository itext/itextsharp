using System;

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IWrapper
    {
		/// <summary>The name of the algorithm this cipher implements.</summary>
		string AlgorithmName { get; }

		void Init(bool forWrapping, ICipherParameters parameters);

		byte[] Wrap(byte[] input, int inOff, int length);

        byte[] Unwrap(byte[] input, int inOff, int length);
    }
}
