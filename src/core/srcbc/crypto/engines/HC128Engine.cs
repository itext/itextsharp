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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* HC-128 is a software-efficient stream cipher created by Hongjun Wu. It
	* generates keystream from a 128-bit secret key and a 128-bit initialization
	* vector.
	* <p>
	* http://www.ecrypt.eu.org/stream/p3ciphers/hc/hc128_p3.pdf
	* </p><p>
	* It is a third phase candidate in the eStream contest, and is patent-free.
	* No attacks are known as of today (April 2007). See
	*
	* http://www.ecrypt.eu.org/stream/hcp3.html
	* </p>
	*/
	public class HC128Engine
		: IStreamCipher
	{
		private uint[] p = new uint[512];
		private uint[] q = new uint[512];
		private uint cnt = 0;

		private static uint F1(uint x)
		{
			return RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >> 3);
		}

		private static uint F2(uint x)
		{
			return RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10);
		}

		private uint G1(uint x, uint y, uint z)
		{
			return (RotateRight(x, 10) ^ RotateRight(z, 23)) + RotateRight(y, 8);
		}

		private uint G2(uint x, uint y, uint z)
		{
			return (RotateLeft(x, 10) ^ RotateLeft(z, 23)) + RotateLeft(y, 8);
		}

		private static uint RotateLeft(uint	x, int bits)
		{
			return (x << bits) | (x >> -bits);
		}

		private static uint RotateRight(uint x, int bits)
		{
			return (x >> bits) | (x << -bits);
		}

		private uint H1(uint x)
		{
			return q[x & 0xFF] + q[((x >> 16) & 0xFF) + 256];
		}

		private uint H2(uint x)
		{
			return p[x & 0xFF] + p[((x >> 16) & 0xFF) + 256];
		}

		private static uint Mod1024(uint x)
		{
			return x & 0x3FF;
		}

		private static uint Mod512(uint x)
		{
			return x & 0x1FF;
		}

		private static uint Dim(uint x, uint y)
		{
			return Mod512(x - y);
		}

		private uint Step()
		{
			uint j = Mod512(cnt);
			uint ret;
			if (cnt < 512)
			{
				p[j] += G1(p[Dim(j, 3)], p[Dim(j, 10)], p[Dim(j, 511)]);
				ret = H1(p[Dim(j, 12)]) ^ p[j];
			}
			else
			{
				q[j] += G2(q[Dim(j, 3)], q[Dim(j, 10)], q[Dim(j, 511)]);
				ret = H2(q[Dim(j, 12)]) ^ q[j];
			}
			cnt = Mod1024(cnt + 1);
			return ret;
		}

		private byte[] key, iv;
		private bool initialised;

		private void Init()
		{
			if (key.Length != 16)
				throw new ArgumentException("The key must be 128 bits long");

			cnt = 0;

			uint[] w = new uint[1280];

			for (int i = 0; i < 16; i++)
			{
				w[i >> 2] |= ((uint)key[i] << (8 * (i & 0x3)));
			}
			Array.Copy(w, 0, w, 4, 4);

			for (int i = 0; i < iv.Length && i < 16; i++)
			{
				w[(i >> 2) + 8] |= ((uint)iv[i] << (8 * (i & 0x3)));
			}
			Array.Copy(w, 8, w, 12, 4);

			for (uint i = 16; i < 1280; i++)
			{
				w[i] = F2(w[i - 2]) + w[i - 7] + F1(w[i - 15]) + w[i - 16] + i;
			}

			Array.Copy(w, 256, p, 0, 512);
			Array.Copy(w, 768, q, 0, 512);

			for (int i = 0; i < 512; i++)
			{
				p[i] = Step();
			}
			for (int i = 0; i < 512; i++)
			{
				q[i] = Step();
			}

			cnt = 0;
		}

		public string AlgorithmName
		{
			get { return "HC-128"; }
		}

		/**
		* Initialise a HC-128 cipher.
		*
		* @param forEncryption whether or not we are for encryption. Irrelevant, as
		*                      encryption and decryption are the same.
		* @param params        the parameters required to set up the cipher.
		* @throws ArgumentException if the params argument is
		*                                  inappropriate (ie. the key is not 128 bit long).
		*/
		public void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			ICipherParameters keyParam = parameters;

			if (parameters is ParametersWithIV)
			{
				iv = ((ParametersWithIV)parameters).GetIV();
				keyParam = ((ParametersWithIV)parameters).Parameters;
			}
			else
			{
				iv = new byte[0];
			}

			if (keyParam is KeyParameter)
			{
				key = ((KeyParameter)keyParam).GetKey();
				Init();
			}
			else
			{
				throw new ArgumentException(
					"Invalid parameter passed to HC128 init - " + parameters.GetType().Name,
					"parameters");
			}

			initialised = true;
		}

		private byte[] buf = new byte[4];
		private int idx = 0;

		private byte GetByte()
		{
			if (idx == 0)
			{
				Pack.UInt32_To_LE(Step(), buf);				
			}
			byte ret = buf[idx];
			idx = idx + 1 & 0x3;
			return ret;
		}

		public void ProcessBytes(
			byte[]	input,
			int		inOff,
			int		len,
			byte[]	output,
			int		outOff)
		{
			if (!initialised)
				throw new InvalidOperationException(AlgorithmName + " not initialised");
			if ((inOff + len) > input.Length)
				throw new DataLengthException("input buffer too short");
			if ((outOff + len) > output.Length)
				throw new DataLengthException("output buffer too short");

			for (int i = 0; i < len; i++)
			{
				output[outOff + i] = (byte)(input[inOff + i] ^ GetByte());
			}
		}

		public void Reset()
		{
			idx = 0;
			Init();
		}

		public byte ReturnByte(byte input)
		{
			return (byte)(input ^ GetByte());
		}
	}
}
