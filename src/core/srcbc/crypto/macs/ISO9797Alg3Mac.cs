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

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs
{
	/**
	* DES based CBC Block Cipher MAC according to ISO9797, algorithm 3 (ANSI X9.19 Retail MAC)
	*
	* This could as well be derived from CBCBlockCipherMac, but then the property mac in the base
	* class must be changed to protected
	*/
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class ISO9797Alg3Mac : IMac
	{
		private byte[] mac;
		private byte[] buf;
		private int bufOff;
		private IBlockCipher cipher;
		private IBlockCipherPadding padding;
		private int macSize;
		private KeyParameter lastKey2;
		private KeyParameter lastKey3;

		/**
		* create a Retail-MAC based on a CBC block cipher. This will produce an
		* authentication code of the length of the block size of the cipher.
		*
		* @param cipher the cipher to be used as the basis of the MAC generation. This must
		* be DESEngine.
		*/
		public ISO9797Alg3Mac(
			IBlockCipher cipher)
			: this(cipher, cipher.GetBlockSize() * 8, null)
		{
		}

		/**
		* create a Retail-MAC based on a CBC block cipher. This will produce an
		* authentication code of the length of the block size of the cipher.
		*
		* @param cipher the cipher to be used as the basis of the MAC generation.
		* @param padding the padding to be used to complete the last block.
		*/
		public ISO9797Alg3Mac(
			IBlockCipher		cipher,
			IBlockCipherPadding	padding)
			: this(cipher, cipher.GetBlockSize() * 8, padding)
		{
		}

		/**
		* create a Retail-MAC based on a block cipher with the size of the
		* MAC been given in bits. This class uses single DES CBC mode as the basis for the
		* MAC generation.
		* <p>
		* Note: the size of the MAC must be at least 24 bits (FIPS Publication 81),
		* or 16 bits if being used as a data authenticator (FIPS Publication 113),
		* and in general should be less than the size of the block cipher as it reduces
		* the chance of an exhaustive attack (see Handbook of Applied Cryptography).
		* </p>
		* @param cipher the cipher to be used as the basis of the MAC generation.
		* @param macSizeInBits the size of the MAC in bits, must be a multiple of 8.
		*/
		public ISO9797Alg3Mac(
			IBlockCipher	cipher,
			int				macSizeInBits)
			: this(cipher, macSizeInBits, null)
		{
		}

		/**
		* create a standard MAC based on a block cipher with the size of the
		* MAC been given in bits. This class uses single DES CBC mode as the basis for the
		* MAC generation. The final block is decrypted and then encrypted using the
		* middle and right part of the key.
		* <p>
		* Note: the size of the MAC must be at least 24 bits (FIPS Publication 81),
		* or 16 bits if being used as a data authenticator (FIPS Publication 113),
		* and in general should be less than the size of the block cipher as it reduces
		* the chance of an exhaustive attack (see Handbook of Applied Cryptography).
		* </p>
		* @param cipher the cipher to be used as the basis of the MAC generation.
		* @param macSizeInBits the size of the MAC in bits, must be a multiple of 8.
		* @param padding the padding to be used to complete the last block.
		*/
		public ISO9797Alg3Mac(
			IBlockCipher		cipher,
			int					macSizeInBits,
			IBlockCipherPadding	padding)
		{
			if ((macSizeInBits % 8) != 0)
				throw new ArgumentException("MAC size must be multiple of 8");

			if (!(cipher is DesEngine))
				throw new ArgumentException("cipher must be instance of DesEngine");

			this.cipher = new CbcBlockCipher(cipher);
			this.padding = padding;
			this.macSize = macSizeInBits / 8;

			mac = new byte[cipher.GetBlockSize()];
			buf = new byte[cipher.GetBlockSize()];
			bufOff = 0;
		}

		public string AlgorithmName
		{
			get { return "ISO9797Alg3"; }
		}

		public void Init(
			ICipherParameters parameters)
		{
			Reset();

			if (!(parameters is KeyParameter || parameters is ParametersWithIV))
				throw new ArgumentException("parameters must be an instance of KeyParameter or ParametersWithIV");

			// KeyParameter must contain a double or triple length DES key,
			// however the underlying cipher is a single DES. The middle and
			// right key are used only in the final step.

			KeyParameter kp;
			if (parameters is KeyParameter)
			{
				kp = (KeyParameter)parameters;
			}
			else
			{
				kp = (KeyParameter)((ParametersWithIV)parameters).Parameters;
			}

			KeyParameter key1;
			byte[] keyvalue = kp.GetKey();

			if (keyvalue.Length == 16)
			{ // Double length DES key
				key1 = new KeyParameter(keyvalue, 0, 8);
				this.lastKey2 = new KeyParameter(keyvalue, 8, 8);
				this.lastKey3 = key1;
			}
			else if (keyvalue.Length == 24)
			{ // Triple length DES key
				key1 = new KeyParameter(keyvalue, 0, 8);
				this.lastKey2 = new KeyParameter(keyvalue, 8, 8);
				this.lastKey3 = new KeyParameter(keyvalue, 16, 8);
			}
			else
			{
				throw new ArgumentException("Key must be either 112 or 168 bit long");
			}

			if (parameters is ParametersWithIV)
			{
				cipher.Init(true, new ParametersWithIV(key1, ((ParametersWithIV)parameters).GetIV()));
			}
			else
			{
				cipher.Init(true, key1);
			}
		}

		public int GetMacSize()
		{
			return macSize;
		}

		public void Update(
			byte input)
		{
			if (bufOff == buf.Length)
			{
				cipher.ProcessBlock(buf, 0, mac, 0);
				bufOff = 0;
			}

			buf[bufOff++] = input;
		}

		public void BlockUpdate(
			byte[]	input,
			int		inOff,
			int		len)
		{
			if (len < 0)
				throw new ArgumentException("Can't have a negative input length!");

			int blockSize = cipher.GetBlockSize();
			int resultLen = 0;
			int gapLen = blockSize - bufOff;

			if (len > gapLen)
			{
				Array.Copy(input, inOff, buf, bufOff, gapLen);

				resultLen += cipher.ProcessBlock(buf, 0, mac, 0);

				bufOff = 0;
				len -= gapLen;
				inOff += gapLen;

				while (len > blockSize)
				{
					resultLen += cipher.ProcessBlock(input, inOff, mac, 0);

					len -= blockSize;
					inOff += blockSize;
				}
			}

			Array.Copy(input, inOff, buf, bufOff, len);

			bufOff += len;
		}

		public int DoFinal(
			byte[]	output,
			int		outOff)
		{
			int blockSize = cipher.GetBlockSize();

			if (padding == null)
			{
				// pad with zeroes
				while (bufOff < blockSize)
				{
					buf[bufOff++] = 0;
				}
			}
			else
			{
				if (bufOff == blockSize)
				{
					cipher.ProcessBlock(buf, 0, mac, 0);
					bufOff = 0;
				}

				padding.AddPadding(buf, bufOff);
			}

			cipher.ProcessBlock(buf, 0, mac, 0);

			// Added to code from base class
			DesEngine deseng = new DesEngine();

			deseng.Init(false, this.lastKey2);
			deseng.ProcessBlock(mac, 0, mac, 0);

			deseng.Init(true, this.lastKey3);
			deseng.ProcessBlock(mac, 0, mac, 0);
			// ****

			Array.Copy(mac, 0, output, outOff, macSize);

			Reset();

			return macSize;
		}

		/**
		* Reset the mac generator.
		*/
		public void Reset()
		{
			Array.Clear(buf, 0, buf.Length);
			bufOff = 0;

			// reset the underlying cipher.
			cipher.Reset();
		}
	}
}
