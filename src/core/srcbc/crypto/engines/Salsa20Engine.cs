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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	 * Implementation of Daniel J. Bernstein's Salsa20 stream cipher, Snuffle 2005
	 */
	public class Salsa20Engine
		: IStreamCipher
	{
		/** Constants */
		private const int StateSize = 16; // 16, 32 bit ints = 64 bytes

		private readonly static byte[]
			sigma = Strings.ToAsciiByteArray("expand 32-byte k"),
			tau = Strings.ToAsciiByteArray("expand 16-byte k");

		/*
		 * variables to hold the state of the engine
		 * during encryption and decryption
		 */
		private int		index = 0;
		private uint[]  engineState = new uint[StateSize]; // state
		private uint[]  x = new uint[StateSize]; // internal buffer
		private byte[]  keyStream = new byte[StateSize * 4], // expanded state, 64 bytes
						workingKey  = null,
						workingIV   = null;
		private bool	initialised = false;

		/*
		 * internal counter
		 */
		private uint cW0, cW1, cW2;

		/**
		 * initialise a Salsa20 cipher.
		 *
		 * @param forEncryption whether or not we are for encryption.
		 * @param params the parameters required to set up the cipher.
		 * @exception ArgumentException if the params argument is
		 * inappropriate.
		 */
		public void Init(
			bool				forEncryption, 
			ICipherParameters	parameters)
		{
			/* 
			 * Salsa20 encryption and decryption is completely
			 * symmetrical, so the 'forEncryption' is 
			 * irrelevant. (Like 90% of stream ciphers)
			 */

			ParametersWithIV ivParams = parameters as ParametersWithIV;

			if (ivParams == null)
				throw new ArgumentException("Salsa20 Init requires an IV", "parameters");

			byte[] iv = ivParams.GetIV();

			if (iv == null || iv.Length != 8)
				throw new ArgumentException("Salsa20 requires exactly 8 bytes of IV");

			KeyParameter key = ivParams.Parameters as KeyParameter;

			if (key == null)
				throw new ArgumentException("Salsa20 Init requires a key", "parameters");

			workingKey = key.GetKey();
			workingIV = iv;

			SetKey(workingKey, workingIV);
		}

		public string AlgorithmName
		{
			get { return "Salsa20"; }
		}

		public byte ReturnByte(
			byte input)
		{
			if (LimitExceeded())
			{
				throw new MaxBytesExceededException("2^70 byte limit per IV; Change IV");
			}

			if (index == 0)
			{
				GenerateKeyStream(keyStream);

				if (++engineState[8] == 0)
				{
					++engineState[9];
				}
			}

			byte output = (byte)(keyStream[index] ^ input);
			index = (index + 1) & 63;

			return output;
		}

		public void ProcessBytes(
			byte[]	inBytes, 
			int		inOff, 
			int		len, 
			byte[]	outBytes, 
			int		outOff)
		{
			if (!initialised)
			{
				throw new InvalidOperationException(AlgorithmName + " not initialised");
			}

			if ((inOff + len) > inBytes.Length)
			{
				throw new DataLengthException("input buffer too short");
			}

			if ((outOff + len) > outBytes.Length)
			{
				throw new DataLengthException("output buffer too short");
			}

			if (LimitExceeded((uint)len))
			{
				throw new MaxBytesExceededException("2^70 byte limit per IV would be exceeded; Change IV");
			}

			for (int i = 0; i < len; i++)
			{
				if (index == 0)
				{
					GenerateKeyStream(keyStream);

					if (++engineState[8] == 0)
					{
						++engineState[9];
					}
				}
				outBytes[i+outOff] = (byte)(keyStream[index]^inBytes[i+inOff]);
				index = (index + 1) & 63;
			}
		}

		public void Reset()
		{
			SetKey(workingKey, workingIV);
		}

		// Private implementation

		private void SetKey(byte[] keyBytes, byte[] ivBytes)
		{
			workingKey = keyBytes;
			workingIV  = ivBytes;

			index = 0;
			ResetCounter();
			int offset = 0;
			byte[] constants;

			// Key
			engineState[1] = Pack.LE_To_UInt32(workingKey, 0);
			engineState[2] = Pack.LE_To_UInt32(workingKey, 4);
			engineState[3] = Pack.LE_To_UInt32(workingKey, 8);
			engineState[4] = Pack.LE_To_UInt32(workingKey, 12);

			if (workingKey.Length == 32)
			{
				constants = sigma;
				offset = 16;
			}
			else
			{
				constants = tau;
			}

			engineState[11] = Pack.LE_To_UInt32(workingKey, offset);
			engineState[12] = Pack.LE_To_UInt32(workingKey, offset + 4);
			engineState[13] = Pack.LE_To_UInt32(workingKey, offset + 8);
			engineState[14] = Pack.LE_To_UInt32(workingKey, offset + 12);
			engineState[0] = Pack.LE_To_UInt32(constants, 0);
			engineState[5] = Pack.LE_To_UInt32(constants, 4);
			engineState[10] = Pack.LE_To_UInt32(constants, 8);
			engineState[15] = Pack.LE_To_UInt32(constants, 12);

			// IV
			engineState[6] = Pack.LE_To_UInt32(workingIV, 0);
			engineState[7] = Pack.LE_To_UInt32(workingIV, 4);
			engineState[8] = engineState[9] = 0;

			initialised = true;
		}

		private void GenerateKeyStream(byte[] output)
		{
			SalsaCore(20, engineState, x);
			Pack.UInt32_To_LE(x, output, 0);
		}

		internal static void SalsaCore(int rounds, uint[] state, uint[] x)
		{
            // TODO Exception if rounds odd?

            Array.Copy(state, 0, x, 0, state.Length);

			for (int i = rounds; i > 0; i -= 2)
			{
				x[ 4] ^= R((x[ 0]+x[12]), 7);
				x[ 8] ^= R((x[ 4]+x[ 0]), 9);
				x[12] ^= R((x[ 8]+x[ 4]),13);
				x[ 0] ^= R((x[12]+x[ 8]),18);
				x[ 9] ^= R((x[ 5]+x[ 1]), 7);
				x[13] ^= R((x[ 9]+x[ 5]), 9);
				x[ 1] ^= R((x[13]+x[ 9]),13);
				x[ 5] ^= R((x[ 1]+x[13]),18);
				x[14] ^= R((x[10]+x[ 6]), 7);
				x[ 2] ^= R((x[14]+x[10]), 9);
				x[ 6] ^= R((x[ 2]+x[14]),13);
				x[10] ^= R((x[ 6]+x[ 2]),18);
				x[ 3] ^= R((x[15]+x[11]), 7);
				x[ 7] ^= R((x[ 3]+x[15]), 9);
				x[11] ^= R((x[ 7]+x[ 3]),13);
				x[15] ^= R((x[11]+x[ 7]),18);
				x[ 1] ^= R((x[ 0]+x[ 3]), 7);
				x[ 2] ^= R((x[ 1]+x[ 0]), 9);
				x[ 3] ^= R((x[ 2]+x[ 1]),13);
				x[ 0] ^= R((x[ 3]+x[ 2]),18);
				x[ 6] ^= R((x[ 5]+x[ 4]), 7);
				x[ 7] ^= R((x[ 6]+x[ 5]), 9);
				x[ 4] ^= R((x[ 7]+x[ 6]),13);
				x[ 5] ^= R((x[ 4]+x[ 7]),18);
				x[11] ^= R((x[10]+x[ 9]), 7);
				x[ 8] ^= R((x[11]+x[10]), 9);
				x[ 9] ^= R((x[ 8]+x[11]),13);
				x[10] ^= R((x[ 9]+x[ 8]),18);
				x[12] ^= R((x[15]+x[14]), 7);
				x[13] ^= R((x[12]+x[15]), 9);
				x[14] ^= R((x[13]+x[12]),13);
				x[15] ^= R((x[14]+x[13]),18);
			}

			for (int i = 0; i < StateSize; ++i)
			{
				x[i] += state[i];
			}
		}

		private static uint R(uint x, int y)
		{
			return (x << y) | (x >> (32 - y));
		}

		private void ResetCounter()
		{
			cW0 = 0;
			cW1 = 0;
			cW2 = 0;
		}

		private bool LimitExceeded()
		{
			if (++cW0 == 0)
			{
				if (++cW1 == 0)
				{
					return (++cW2 & 0x20) != 0;          // 2^(32 + 32 + 6)
				}
			}

			return false;
		}

		/*
		 * this relies on the fact len will always be positive.
		 */
		private bool LimitExceeded(
			uint len)
		{
			uint old = cW0;
			cW0 += len;
			if (cW0 < old)
			{
				if (++cW1 == 0)
				{
					return (++cW2 & 0x20) != 0;          // 2^(32 + 32 + 6)
				}
			}

			return false;
		}
	}
}
