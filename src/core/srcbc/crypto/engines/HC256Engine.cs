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
	* HC-256 is a software-efficient stream cipher created by Hongjun Wu. It 
	* generates keystream from a 256-bit secret key and a 256-bit initialization 
	* vector.
	* <p>
	* http://www.ecrypt.eu.org/stream/p3ciphers/hc/hc256_p3.pdf
	* </p><p>
	* Its brother, HC-128, is a third phase candidate in the eStream contest.
	* The algorithm is patent-free. No attacks are known as of today (April 2007). 
	* See
	* 
	* http://www.ecrypt.eu.org/stream/hcp3.html
	* </p>
	*/
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class HC256Engine
		: IStreamCipher
	{
		private uint[] p = new uint[1024];
		private uint[] q = new uint[1024];
		private uint cnt = 0;

		private uint Step()
		{
			uint j = cnt & 0x3FF;
			uint ret;
			if (cnt < 1024)
			{
				uint x = p[(j - 3 & 0x3FF)];
				uint y = p[(j - 1023 & 0x3FF)];
				p[j] += p[(j - 10 & 0x3FF)]
					+ (RotateRight(x, 10) ^ RotateRight(y, 23))
					+ q[((x ^ y) & 0x3FF)];

				x = p[(j - 12 & 0x3FF)];
				ret = (q[x & 0xFF] + q[((x >> 8) & 0xFF) + 256]
					+ q[((x >> 16) & 0xFF) + 512] + q[((x >> 24) & 0xFF) + 768])
					^ p[j];
			}
			else
			{
				uint x = q[(j - 3 & 0x3FF)];
				uint y = q[(j - 1023 & 0x3FF)];
				q[j] += q[(j - 10 & 0x3FF)]
					+ (RotateRight(x, 10) ^ RotateRight(y, 23))
					+ p[((x ^ y) & 0x3FF)];

				x = q[(j - 12 & 0x3FF)];
				ret = (p[x & 0xFF] + p[((x >> 8) & 0xFF) + 256]
					+ p[((x >> 16) & 0xFF) + 512] + p[((x >> 24) & 0xFF) + 768])
					^ q[j];
			}
			cnt = cnt + 1 & 0x7FF;
			return ret;
		}

		private byte[] key, iv;
		private bool initialised;

		private void Init()
		{
			if (key.Length != 32 && key.Length != 16)
				throw new ArgumentException("The key must be 128/256 bits long");

			if (iv.Length < 16)
				throw new ArgumentException("The IV must be at least 128 bits long");

			if (key.Length != 32)
	        {
				byte[] k = new byte[32];

				Array.Copy(key, 0, k, 0, key.Length);
				Array.Copy(key, 0, k, 16, key.Length);

				key = k;
			}

			if (iv.Length < 32)
			{
				byte[] newIV = new byte[32];

				Array.Copy(iv, 0, newIV, 0, iv.Length);
				Array.Copy(iv, 0, newIV, iv.Length, newIV.Length - iv.Length);

				iv = newIV;
			}

			cnt = 0;

			uint[] w = new uint[2560];

			for (int i = 0; i < 32; i++)
			{
				w[i >> 2] |= ((uint)key[i] << (8 * (i & 0x3)));
			}

			for (int i = 0; i < 32; i++)
			{
				w[(i >> 2) + 8] |= ((uint)iv[i] << (8 * (i & 0x3)));
			}

			for (uint i = 16; i < 2560; i++)
			{
				uint x = w[i - 2];
				uint y = w[i - 15];
				w[i] = (RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10))
					+ w[i - 7]
					+ (RotateRight(y, 7) ^ RotateRight(y, 18) ^ (y >> 3))
					+ w[i - 16] + i;
			}

			Array.Copy(w, 512, p, 0, 1024);
			Array.Copy(w, 1536, q, 0, 1024);

			for (int i = 0; i < 4096; i++)
			{
				Step();
			}

			cnt = 0;
		}

		public string AlgorithmName
		{
			get { return "HC-256"; }
		}

		/**
		* Initialise a HC-256 cipher.
		*
		* @param forEncryption whether or not we are for encryption. Irrelevant, as
		*                      encryption and decryption are the same.
		* @param params        the parameters required to set up the cipher.
		* @throws ArgumentException if the params argument is
		*                                  inappropriate (ie. the key is not 256 bit long).
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
					"Invalid parameter passed to HC256 init - " + parameters.GetType().Name,
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

		private static uint RotateRight(uint x, int bits)
		{
			return (x >> bits) | (x << -bits);
		}
	}
}
