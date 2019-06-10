/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.Text;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
	public class DerBitString
		: DerStringBase
	{
		private static readonly char[] table
			= { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		private readonly byte[]	data;
		private readonly int	padBits;

		/**
		 * return the correct number of pad bits for a bit string defined in
		 * a 32 bit constant
		 */
		static internal int GetPadBits(
			int bitString)
		{
			int val = 0;
			for (int i = 3; i >= 0; i--)
			{
				//
				// this may look a little odd, but if it isn't done like this pre jdk1.2
				// JVM's break!
				//
				if (i != 0)
				{
					if ((bitString >> (i * 8)) != 0)
					{
						val = (bitString >> (i * 8)) & 0xFF;
						break;
					}
				}
				else
				{
					if (bitString != 0)
					{
						val = bitString & 0xFF;
						break;
					}
				}
			}

			if (val == 0)
			{
				return 7;
			}

			int bits = 1;

			while (((val <<= 1) & 0xFF) != 0)
			{
				bits++;
			}

			return 8 - bits;
		}

		/**
		 * return the correct number of bytes for a bit string defined in
		 * a 32 bit constant
		 */
		static internal byte[] GetBytes(
			int bitString)
		{
			int bytes = 4;
			for (int i = 3; i >= 1; i--)
			{
				if ((bitString & (0xFF << (i * 8))) != 0)
				{
					break;
				}
				bytes--;
			}

			byte[] result = new byte[bytes];
			for (int i = 0; i < bytes; i++)
			{
				result[i] = (byte) ((bitString >> (i * 8)) & 0xFF);
			}

			return result;
		}

		/**
		 * return a Bit string from the passed in object
		 *
		 * @exception ArgumentException if the object cannot be converted.
		 */
		public static DerBitString GetInstance(
			object obj)
		{
			if (obj == null || obj is DerBitString)
			{
				return (DerBitString) obj;
			}

			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		/**
		 * return a Bit string from a tagged object.
		 *
		 * @param obj the tagged object holding the object we want
		 * @param explicitly true if the object is meant to be explicitly
		 *              tagged false otherwise.
		 * @exception ArgumentException if the tagged object cannot
		 *               be converted.
		 */
		public static DerBitString GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerBitString)
			{
				return GetInstance(o);
			}

			return FromAsn1Octets(((Asn1OctetString)o).GetOctets());
		}

		internal DerBitString(
			byte	data,
			int		padBits)
		{
			this.data = new byte[]{ data };
			this.padBits = padBits;
		}

		/**
		 * @param data the octets making up the bit string.
		 * @param padBits the number of extra bits at the end of the string.
		 */
		public DerBitString(
			byte[]	data,
			int		padBits)
		{
			// TODO Deep copy?
			this.data = data;
			this.padBits = padBits;
		}

		public DerBitString(
			byte[] data)
		{
			// TODO Deep copy?
			this.data = data;
		}

		public DerBitString(
			Asn1Encodable obj)
		{
			this.data = obj.GetDerEncoded();
			//this.padBits = 0;
		}

		public byte[] GetBytes()
		{
			return data;
		}

		public int PadBits
		{
			get { return padBits; }
		}

		/**
		 * @return the value of the bit string as an int (truncating if necessary)
		 */
		public int IntValue
		{
			get
			{
				int value = 0;

				for (int i = 0; i != data.Length && i != 4; i++)
				{
					value |= (data[i] & 0xff) << (8 * i);
				}

				return value;
			}
		}

		internal override void Encode(
			DerOutputStream derOut)
		{
			byte[] bytes = new byte[GetBytes().Length + 1];

			bytes[0] = (byte) PadBits;
			Array.Copy(GetBytes(), 0, bytes, 1, bytes.Length - 1);

			derOut.WriteEncoded(Asn1Tags.BitString, bytes);
		}

		protected override int Asn1GetHashCode()
		{
			return padBits.GetHashCode() ^ Arrays.GetHashCode(data);
		}

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
		{
			DerBitString other = asn1Object as DerBitString;

			if (other == null)
				return false;

			return this.padBits == other.padBits
				&& Arrays.AreEqual(this.data, other.data);
		}

		public override string GetString()
		{
			StringBuilder buffer = new StringBuilder("#");

			byte[] str = GetDerEncoded();

			for (int i = 0; i != str.Length; i++)
			{
				uint ubyte = str[i];
				buffer.Append(table[(ubyte >> 4) & 0xf]);
				buffer.Append(table[str[i] & 0xf]);
			}

			return buffer.ToString();
		}

		internal static DerBitString FromAsn1Octets(byte[] octets)
		{
	        if (octets.Length < 1)
	            throw new ArgumentException("truncated BIT STRING detected");

			int padBits = octets[0];
			byte[] data = new byte[octets.Length - 1];
			Array.Copy(octets, 1, data, 0, data.Length);
			return new DerBitString(data, padBits);
		}
	}
}

