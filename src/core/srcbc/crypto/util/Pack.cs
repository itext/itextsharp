using System;

namespace Org.BouncyCastle.Crypto.Utilities
{
	internal sealed class Pack
	{
		private Pack()
		{
		}

		internal static void UInt32_To_BE(uint n, byte[] bs)
		{
			bs[0] = (byte)(n >> 24);
			bs[1] = (byte)(n >> 16);
			bs[2] = (byte)(n >>  8);
			bs[3] = (byte)(n      );
		}

		internal static void UInt32_To_BE(uint n, byte[] bs, int off)
		{
			bs[off++] = (byte)(n >> 24);
			bs[off++] = (byte)(n >> 16);
			bs[off++] = (byte)(n >>  8);
			bs[off  ] = (byte)(n      );
		}

		internal static uint BE_To_UInt32(byte[] bs)
		{
			uint n = (uint)bs[0] << 24;
			n |= (uint)bs[1] << 16;
			n |= (uint)bs[2] << 8;
			n |= (uint)bs[3];
			return n;
		}

		internal static uint BE_To_UInt32(byte[] bs, int off)
		{
			uint n = (uint)bs[off++] << 24;
			n |= (uint)bs[off++] << 16;
			n |= (uint)bs[off++] << 8;
			n |= (uint)bs[off];
			return n;
		}

		internal static void UInt32_To_LE(uint n, byte[] bs)
		{
			bs[0] = (byte)(n      );
			bs[1] = (byte)(n >>  8);
			bs[2] = (byte)(n >> 16);
			bs[3] = (byte)(n >> 24);
		}

		internal static void UInt32_To_LE(uint n, byte[] bs, int off)
		{
			bs[off++] = (byte)(n      );
			bs[off++] = (byte)(n >>  8);
			bs[off++] = (byte)(n >> 16);
			bs[off  ] = (byte)(n >> 24);
		}

		internal static uint LE_To_UInt32(byte[] bs)
		{
			uint n = (uint)bs[0];
			n |= (uint)bs[1] << 8;
			n |= (uint)bs[2] << 16;
			n |= (uint)bs[3] << 24;
			return n;
		}

		internal static uint LE_To_UInt32(byte[] bs, int off)
		{
			uint n = (uint)bs[off++];
			n |= (uint)bs[off++] << 8;
			n |= (uint)bs[off++] << 16;
			n |= (uint)bs[off] << 24;
			return n;
		}
	}
}
