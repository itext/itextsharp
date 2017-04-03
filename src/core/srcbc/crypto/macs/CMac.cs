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

using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;

namespace Org.BouncyCastle.Crypto.Macs
{
	/**
	* CMAC - as specified at www.nuee.nagoya-u.ac.jp/labs/tiwata/omac/omac.html
	* <p>
	* CMAC is analogous to OMAC1 - see also en.wikipedia.org/wiki/CMAC
	* </p><p>
	* CMAC is a NIST recomendation - see 
	* csrc.nist.gov/CryptoToolkit/modes/800-38_Series_Publications/SP800-38B.pdf
	* </p><p>
	* CMAC/OMAC1 is a blockcipher-based message authentication code designed and
	* analyzed by Tetsu Iwata and Kaoru Kurosawa.
	* </p><p>
	* CMAC/OMAC1 is a simple variant of the CBC MAC (Cipher Block Chaining Message 
	* Authentication Code). OMAC stands for One-Key CBC MAC.
	* </p><p>
	* It supports 128- or 64-bits block ciphers, with any key size, and returns
	* a MAC with dimension less or equal to the block size of the underlying 
	* cipher.
	* </p>
	*/
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CMac
		: IMac
	{
		private const byte CONSTANT_128 = (byte)0x87;
		private const byte CONSTANT_64 = (byte)0x1b;

		private byte[] ZEROES;

		private byte[] mac;

		private byte[] buf;
		private int bufOff;
		private IBlockCipher cipher;

		private int macSize;

		private byte[] L, Lu, Lu2;

		/**
		* create a standard MAC based on a CBC block cipher (64 or 128 bit block).
		* This will produce an authentication code the length of the block size
		* of the cipher.
		*
		* @param cipher the cipher to be used as the basis of the MAC generation.
		*/
		public CMac(
			IBlockCipher cipher)
			: this(cipher, cipher.GetBlockSize() * 8)
		{
		}

		/**
		* create a standard MAC based on a block cipher with the size of the
		* MAC been given in bits.
		* <p/>
		* Note: the size of the MAC must be at least 24 bits (FIPS Publication 81),
		* or 16 bits if being used as a data authenticator (FIPS Publication 113),
		* and in general should be less than the size of the block cipher as it reduces
		* the chance of an exhaustive attack (see Handbook of Applied Cryptography).
		*
		* @param cipher        the cipher to be used as the basis of the MAC generation.
		* @param macSizeInBits the size of the MAC in bits, must be a multiple of 8 and @lt;= 128.
		*/
		public CMac(
			IBlockCipher	cipher,
			int				macSizeInBits)
		{
			if ((macSizeInBits % 8) != 0)
				throw new ArgumentException("MAC size must be multiple of 8");

			if (macSizeInBits > (cipher.GetBlockSize() * 8))
			{
				throw new ArgumentException(
					"MAC size must be less or equal to "
						+ (cipher.GetBlockSize() * 8));
			}

			if (cipher.GetBlockSize() != 8 && cipher.GetBlockSize() != 16)
			{
				throw new ArgumentException(
					"Block size must be either 64 or 128 bits");
			}

			this.cipher = new CbcBlockCipher(cipher);
			this.macSize = macSizeInBits / 8;

			mac = new byte[cipher.GetBlockSize()];

			buf = new byte[cipher.GetBlockSize()];

			ZEROES = new byte[cipher.GetBlockSize()];

			bufOff = 0;
		}

		public string AlgorithmName
		{
			get { return cipher.AlgorithmName; }
		}

		private static byte[] doubleLu(
			byte[] inBytes)
		{
			int FirstBit = (inBytes[0] & 0xFF) >> 7;
			byte[] ret = new byte[inBytes.Length];
			for (int i = 0; i < inBytes.Length - 1; i++)
			{
				ret[i] = (byte)((inBytes[i] << 1) + ((inBytes[i + 1] & 0xFF) >> 7));
			}
			ret[inBytes.Length - 1] = (byte)(inBytes[inBytes.Length - 1] << 1);
			if (FirstBit == 1)
			{
				ret[inBytes.Length - 1] ^= inBytes.Length == 16 ? CONSTANT_128 : CONSTANT_64;
			}
			return ret;
		}

		public void Init(
			ICipherParameters parameters)
		{
            if (parameters != null)
            {
                cipher.Init(true, parameters);

                //initializes the L, Lu, Lu2 numbers
                L = new byte[ZEROES.Length];
                cipher.ProcessBlock(ZEROES, 0, L, 0);
                Lu = doubleLu(L);
                Lu2 = doubleLu(Lu);
            }

            Reset();
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
			byte[]	inBytes,
			int		inOff,
			int		len)
		{
			if (len < 0)
				throw new ArgumentException("Can't have a negative input length!");

			int blockSize = cipher.GetBlockSize();
			int gapLen = blockSize - bufOff;

			if (len > gapLen)
			{
				Array.Copy(inBytes, inOff, buf, bufOff, gapLen);

				cipher.ProcessBlock(buf, 0, mac, 0);

				bufOff = 0;
				len -= gapLen;
				inOff += gapLen;

				while (len > blockSize)
				{
					cipher.ProcessBlock(inBytes, inOff, mac, 0);

					len -= blockSize;
					inOff += blockSize;
				}
			}

			Array.Copy(inBytes, inOff, buf, bufOff, len);

			bufOff += len;
		}

		public int DoFinal(
			byte[]	outBytes,
			int		outOff)
		{
			int blockSize = cipher.GetBlockSize();

			byte[] lu;
			if (bufOff == blockSize)
			{
				lu = Lu;
			}
			else
			{
				new ISO7816d4Padding().AddPadding(buf, bufOff);
				lu = Lu2;
			}

			for (int i = 0; i < mac.Length; i++)
			{
				buf[i] ^= lu[i];
			}

			cipher.ProcessBlock(buf, 0, mac, 0);

			Array.Copy(mac, 0, outBytes, outOff, macSize);

			Reset();

			return macSize;
		}

		/**
		* Reset the mac generator.
		*/
		public void Reset()
		{
			/*
			* clean the buffer.
			*/
			Array.Clear(buf, 0, buf.Length);
			bufOff = 0;

			/*
			* Reset the underlying cipher.
			*/
			cipher.Reset();
		}
	}
}
