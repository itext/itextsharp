using System;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class BasicGcmMultiplier
		: IGcmMultiplier
	{
		private byte[] H;

		public void Init(byte[] H)
		{
            this.H = Arrays.Clone(H);
		}

        public void MultiplyH(byte[] x)
		{
			GcmUtilities.Multiply(x, H);
		}
	}
}
