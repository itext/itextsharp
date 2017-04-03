using System;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Asn1.X9
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public sealed class X9IntegerConverter
    {
		private X9IntegerConverter()
		{
		}

		public static int GetByteLength(
            ECFieldElement fe)
        {
			return (fe.FieldSize + 7) / 8;
        }

		public static int GetByteLength(
			ECCurve c)
		{
			return (c.FieldSize + 7) / 8;
		}

		public static byte[] IntegerToBytes(
			BigInteger	s,
			int			qLength)
		{
			byte[] bytes = s.ToByteArrayUnsigned();

			if (qLength < bytes.Length)
			{
				byte[] tmp = new byte[qLength];
				Array.Copy(bytes, bytes.Length - tmp.Length, tmp, 0, tmp.Length);
				return tmp;
			}
			else if (qLength > bytes.Length)
			{
				byte[] tmp = new byte[qLength];
				Array.Copy(bytes, 0, tmp, tmp.Length - bytes.Length, bytes.Length);
				return tmp;
			}

			return bytes;
		}
    }
}
