using System;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	public class BasicGcmMultiplier
		: IGcmMultiplier
	{
		private byte[] H;

		public void Init(byte[] H)
		{
			this.H = (byte[])H.Clone();
		}

		public void MultiplyH(byte[] x)
		{
			byte[] z = new byte[16];

			for (int i = 0; i < 16; ++i)
			{
				byte h = H[i];
				for (int j = 7; j >= 0; --j)
				{
					if ((h & (1 << j)) != 0)
					{
						GcmUtilities.Xor(z, x);
					}

					bool lsb = (x[15] & 1) != 0;
					GcmUtilities.ShiftRight(x);
					if (lsb)
					{
						// R = new byte[]{ 0xe1, ... };
						//GcmUtilities.Xor(v, R);
						x[0] ^= (byte)0xe1;
					}
				}
			}

			Array.Copy(z, x, 16);        
		}
	}
}
