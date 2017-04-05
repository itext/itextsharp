using System;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IGcmExponentiator
	{
		void Init(byte[] x);
		void ExponentiateX(long pow, byte[] output);
	}
}
