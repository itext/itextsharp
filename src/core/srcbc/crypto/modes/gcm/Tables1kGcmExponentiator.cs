using System;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	public class Tables1kGcmExponentiator
		: IGcmExponentiator
	{
	    // A lookup table of the power-of-two powers of 'x'
	    private byte[][] lookupPowX2 = new byte[64][];

		public void Init(byte[] x)
		{
			// Initial value is little-endian 1
			lookupPowX2[0] = new byte[16];
			lookupPowX2[0][0] = (byte)0x80;

			lookupPowX2[1] = Arrays.Clone(x); 

			for (int i = 2; i != 64; ++i)
			{
				byte[] tmp = Arrays.Clone(lookupPowX2[i - 1]);
				GcmUtilities.Multiply(tmp, tmp);
				lookupPowX2[i] = tmp;
			}
		}

		public void ExponentiateX(long pow, byte[] output)
		{
			// Initial value is little-endian 1
			byte[] y = GcmUtilities.OneAsBytes();
			int powX2 = 1;

			while (pow > 0)
			{
				if ((pow & 1L) != 0)
				{
					GcmUtilities.Multiply(y, lookupPowX2[powX2]);
				}
				++powX2;
				pow >>= 1;
			}

			Array.Copy(y, 0, output, 0, 16);
		}
	}
}
