using System;

using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Modes.Gcm
{
	internal abstract class GcmUtilities
	{
		internal static uint[] AsUints(byte[] bs)
		{
			uint[] us = new uint[4];
			us[0] = Pack.BE_To_UInt32(bs, 0);
			us[1] = Pack.BE_To_UInt32(bs, 4);
			us[2] = Pack.BE_To_UInt32(bs, 8);
			us[3] = Pack.BE_To_UInt32(bs, 12);
			return us;
		}

		// P is the value with only bit i=1 set
		internal static void MultiplyP(uint[] x)
		{
			bool lsb = (x[3] & 1) != 0;
			ShiftRight(x);
			if (lsb)
			{
				// R = new uint[]{ 0xe1000000, 0, 0, 0 };
				//Xor(v, R);
				x[0] ^= 0xe1000000;
			}
		}

		internal static void MultiplyP8(uint[] x)
		{
			for (int i = 8; i != 0; --i)
			{
				MultiplyP(x);
			}
		}

		internal static void ShiftRight(byte[] block)
		{
			int i = 0;
			byte bit = 0;
			for (;;)
			{
				byte b = block[i];
				block[i] = (byte)((b >> 1) | bit);
				if (++i == 16) break;
				bit = (byte)(b << 7);
			}
		}

		internal static void ShiftRight(uint[] block)
		{
			int i = 0;
			uint bit = 0;
			for (;;)
			{
				uint b = block[i];
				block[i] = (b >> 1) | bit;
				if (++i == 4) break;
				bit = b << 31;
			}
		}

		internal static void Xor(byte[] block, byte[] val)
		{
			for (int i = 15; i >= 0; --i)
			{
				block[i] ^= val[i];
			}
		}

		internal static void Xor(uint[] block, uint[] val)
		{
			for (int i = 3; i >= 0; --i)
			{
				block[i] ^= val[i];
			}
		}
	}
}
