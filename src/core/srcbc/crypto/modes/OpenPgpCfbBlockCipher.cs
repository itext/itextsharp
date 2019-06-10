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

namespace Org.BouncyCastle.Crypto.Modes
{
    /**
    * Implements OpenPGP's rather strange version of Cipher-FeedBack (CFB) mode
    * on top of a simple cipher. This class assumes the IV has been prepended
    * to the data stream already, and just accomodates the reset after
    * (blockSize + 2) bytes have been read.
    * <p>
    * For further info see <a href="http://www.ietf.org/rfc/rfc2440.html">RFC 2440</a>.
	* </p>
    */
    public class OpenPgpCfbBlockCipher
        : IBlockCipher
    {
        private byte[] IV;
        private byte[] FR;
        private byte[] FRE;

		private readonly IBlockCipher cipher;
		private readonly int blockSize;

		private int count;
        private bool forEncryption;

		/**
        * Basic constructor.
        *
        * @param cipher the block cipher to be used as the basis of the
        * feedback mode.
        */
        public OpenPgpCfbBlockCipher(
            IBlockCipher cipher)
        {
            this.cipher = cipher;

            this.blockSize = cipher.GetBlockSize();
            this.IV = new byte[blockSize];
            this.FR = new byte[blockSize];
            this.FRE = new byte[blockSize];
        }

		/**
        * return the underlying block cipher that we are wrapping.
        *
        * @return the underlying block cipher that we are wrapping.
        */
        public IBlockCipher GetUnderlyingCipher()
        {
            return cipher;
        }

		/**
        * return the algorithm name and mode.
        *
        * @return the name of the underlying algorithm followed by "/PGPCFB"
        * and the block size in bits.
        */
        public string AlgorithmName
        {
            get { return cipher.AlgorithmName + "/OpenPGPCFB"; }
        }

		public bool IsPartialBlockOkay
		{
			get { return true; }
		}

		/**
        * return the block size we are operating at.
        *
        * @return the block size we are operating at (in bytes).
        */
        public int GetBlockSize()
        {
            return cipher.GetBlockSize();
        }

		/**
        * Process one block of input from the array in and write it to
        * the out array.
        *
        * @param in the array containing the input data.
        * @param inOff offset into the in array the data starts at.
        * @param out the array the output data will be copied into.
        * @param outOff the offset into the out array the output will start at.
        * @exception DataLengthException if there isn't enough data in in, or
        * space in out.
        * @exception InvalidOperationException if the cipher isn't initialised.
        * @return the number of bytes processed and produced.
        */
        public int ProcessBlock(
            byte[]	input,
            int		inOff,
            byte[]	output,
            int		outOff)
        {
            return (forEncryption) ? EncryptBlock(input, inOff, output, outOff) : DecryptBlock(input, inOff, output, outOff);
        }

		/**
        * reset the chaining vector back to the IV and reset the underlying
        * cipher.
        */
        public void Reset()
        {
            count = 0;

			Array.Copy(IV, 0, FR, 0, FR.Length);

			cipher.Reset();
        }

        /**
        * Initialise the cipher and, possibly, the initialisation vector (IV).
        * If an IV isn't passed as part of the parameter, the IV will be all zeros.
        * An IV which is too short is handled in FIPS compliant fashion.
        *
        * @param forEncryption if true the cipher is initialised for
        *  encryption, if false for decryption.
        * @param parameters the key and other data required by the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;

            if (parameters is ParametersWithIV)
            {
                ParametersWithIV ivParam = (ParametersWithIV)parameters;
                byte[] iv = ivParam.GetIV();

                if (iv.Length < IV.Length)
                {
                    // prepend the supplied IV with zeros (per FIPS PUB 81)
                    Array.Copy(iv, 0, IV, IV.Length - iv.Length, iv.Length);
                    for (int i = 0; i < IV.Length - iv.Length; i++)
                    {
                        IV[i] = 0;
                    }
                }
                else
                {
                    Array.Copy(iv, 0, IV, 0, IV.Length);
                }

                parameters = ivParam.Parameters;
            }

            Reset();

            cipher.Init(true, parameters);
        }

		/**
        * Encrypt one byte of data according to CFB mode.
        * @param data the byte to encrypt
        * @param blockOff offset in the current block
        * @returns the encrypted byte
        */
        private byte EncryptByte(byte data, int blockOff)
        {
            return (byte)(FRE[blockOff] ^ data);
        }

		/**
        * Do the appropriate processing for CFB IV mode encryption.
        *
        * @param in the array containing the data to be encrypted.
        * @param inOff offset into the in array the data starts at.
        * @param out the array the encrypted data will be copied into.
        * @param outOff the offset into the out array the output will start at.
        * @exception DataLengthException if there isn't enough data in in, or
        * space in out.
        * @exception InvalidOperationException if the cipher isn't initialised.
        * @return the number of bytes processed and produced.
        */
        private int EncryptBlock(
            byte[]	input,
            int		inOff,
            byte[]	outBytes,
            int		outOff)
        {
            if ((inOff + blockSize) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + blockSize) > outBytes.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            if (count > blockSize)
            {
                FR[blockSize - 2] = outBytes[outOff] = EncryptByte(input[inOff], blockSize - 2);
                FR[blockSize - 1] = outBytes[outOff + 1] = EncryptByte(input[inOff + 1], blockSize - 1);

                cipher.ProcessBlock(FR, 0, FRE, 0);

                for (int n = 2; n < blockSize; n++)
                {
					FR[n - 2] = outBytes[outOff + n] = EncryptByte(input[inOff + n], n - 2);
                }
            }
            else if (count == 0)
            {
                cipher.ProcessBlock(FR, 0, FRE, 0);

				for (int n = 0; n < blockSize; n++)
                {
					FR[n] = outBytes[outOff + n] = EncryptByte(input[inOff + n], n);
                }

				count += blockSize;
            }
            else if (count == blockSize)
            {
                cipher.ProcessBlock(FR, 0, FRE, 0);

                outBytes[outOff] = EncryptByte(input[inOff], 0);
                outBytes[outOff + 1] = EncryptByte(input[inOff + 1], 1);

                //
                // do reset
                //
                Array.Copy(FR, 2, FR, 0, blockSize - 2);
                Array.Copy(outBytes, outOff, FR, blockSize - 2, 2);

                cipher.ProcessBlock(FR, 0, FRE, 0);

                for (int n = 2; n < blockSize; n++)
                {
					FR[n - 2] = outBytes[outOff + n] = EncryptByte(input[inOff + n], n - 2);
                }

				count += blockSize;
            }

            return blockSize;
        }

        /**
        * Do the appropriate processing for CFB IV mode decryption.
        *
        * @param in the array containing the data to be decrypted.
        * @param inOff offset into the in array the data starts at.
        * @param out the array the encrypted data will be copied into.
        * @param outOff the offset into the out array the output will start at.
        * @exception DataLengthException if there isn't enough data in in, or
        * space in out.
        * @exception InvalidOperationException if the cipher isn't initialised.
        * @return the number of bytes processed and produced.
        */
        private int DecryptBlock(
            byte[]	input,
            int		inOff,
            byte[]	outBytes,
            int		outOff)
        {
            if ((inOff + blockSize) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + blockSize) > outBytes.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            if (count > blockSize)
            {
				byte inVal = input[inOff];
				FR[blockSize - 2] = inVal;
				outBytes[outOff] = EncryptByte(inVal, blockSize - 2);

				inVal = input[inOff + 1];
				FR[blockSize - 1] = inVal;
				outBytes[outOff + 1] = EncryptByte(inVal, blockSize - 1);

                cipher.ProcessBlock(FR, 0, FRE, 0);

                for (int n = 2; n < blockSize; n++)
                {
					inVal = input[inOff + n];
					FR[n - 2] = inVal;
					outBytes[outOff + n] = EncryptByte(inVal, n - 2);
				}
            }
            else if (count == 0)
            {
                cipher.ProcessBlock(FR, 0, FRE, 0);

                for (int n = 0; n < blockSize; n++)
                {
                    FR[n] = input[inOff + n];
                    outBytes[n] = EncryptByte(input[inOff + n], n);
                }

                count += blockSize;
            }
            else if (count == blockSize)
            {
                cipher.ProcessBlock(FR, 0, FRE, 0);

				byte inVal1 = input[inOff];
				byte inVal2 = input[inOff + 1];
				outBytes[outOff    ] = EncryptByte(inVal1, 0);
				outBytes[outOff + 1] = EncryptByte(inVal2, 1);

                Array.Copy(FR, 2, FR, 0, blockSize - 2);

				FR[blockSize - 2] = inVal1;
				FR[blockSize - 1] = inVal2;

                cipher.ProcessBlock(FR, 0, FRE, 0);

                for (int n = 2; n < blockSize; n++)
                {
					byte inVal = input[inOff + n];
					FR[n - 2] = inVal;
					outBytes[outOff + n] = EncryptByte(inVal, n - 2);
                }

                count += blockSize;
            }

            return blockSize;
        }
    }
}
