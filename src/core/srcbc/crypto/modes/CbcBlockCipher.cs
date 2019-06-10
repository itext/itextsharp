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
    * implements Cipher-Block-Chaining (CBC) mode on top of a simple cipher.
    */
    public class CbcBlockCipher
		: IBlockCipher
    {
        private byte[]			IV, cbcV, cbcNextV;
		private int				blockSize;
        private IBlockCipher	cipher;
        private bool			encrypting;

        /**
        * Basic constructor.
        *
        * @param cipher the block cipher to be used as the basis of chaining.
        */
        public CbcBlockCipher(
            IBlockCipher cipher)
        {
            this.cipher = cipher;
            this.blockSize = cipher.GetBlockSize();

            this.IV = new byte[blockSize];
            this.cbcV = new byte[blockSize];
            this.cbcNextV = new byte[blockSize];
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
        * Initialise the cipher and, possibly, the initialisation vector (IV).
        * If an IV isn't passed as part of the parameter, the IV will be all zeros.
        *
        * @param forEncryption if true the cipher is initialised for
        *  encryption, if false for decryption.
        * @param param the key and other data required by the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public void Init(
            bool forEncryption,
            ICipherParameters parameters)
        {
            bool oldEncrypting = this.encrypting;

            this.encrypting = forEncryption;

            if (parameters is ParametersWithIV)
            {
                ParametersWithIV ivParam = (ParametersWithIV)parameters;
                byte[]      iv = ivParam.GetIV();

                if (iv.Length != blockSize)
                {
                    throw new ArgumentException("initialisation vector must be the same length as block size");
                }

                Array.Copy(iv, 0, IV, 0, iv.Length);

				parameters = ivParam.Parameters;
            }

			Reset();

            // if null it's an IV changed only.
            if (parameters != null)
            {
                cipher.Init(encrypting, parameters);
            }
            else if (oldEncrypting != encrypting)
            {
                throw new ArgumentException("cannot change encrypting state without providing key.");
            }
        }

		/**
        * return the algorithm name and mode.
        *
        * @return the name of the underlying algorithm followed by "/CBC".
        */
        public string AlgorithmName
        {
            get { return cipher.AlgorithmName + "/CBC"; }
        }

		public bool IsPartialBlockOkay
		{
			get { return false; }
		}

		/**
        * return the block size of the underlying cipher.
        *
        * @return the block size of the underlying cipher.
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
            return (encrypting)
				?	EncryptBlock(input, inOff, output, outOff)
				:	DecryptBlock(input, inOff, output, outOff);
        }

        /**
        * reset the chaining vector back to the IV and reset the underlying
        * cipher.
        */
        public void Reset()
        {
            Array.Copy(IV, 0, cbcV, 0, IV.Length);
			Array.Clear(cbcNextV, 0, cbcNextV.Length);

            cipher.Reset();
        }

        /**
        * Do the appropriate chaining step for CBC mode encryption.
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
            byte[]      input,
            int         inOff,
            byte[]      outBytes,
            int         outOff)
        {
            if ((inOff + blockSize) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            /*
            * XOR the cbcV and the input,
            * then encrypt the cbcV
            */
            for (int i = 0; i < blockSize; i++)
            {
                cbcV[i] ^= input[inOff + i];
            }

            int length = cipher.ProcessBlock(cbcV, 0, outBytes, outOff);

            /*
            * copy ciphertext to cbcV
            */
            Array.Copy(outBytes, outOff, cbcV, 0, cbcV.Length);

            return length;
        }

        /**
        * Do the appropriate chaining step for CBC mode decryption.
        *
        * @param in the array containing the data to be decrypted.
        * @param inOff offset into the in array the data starts at.
        * @param out the array the decrypted data will be copied into.
        * @param outOff the offset into the out array the output will start at.
        * @exception DataLengthException if there isn't enough data in in, or
        * space in out.
        * @exception InvalidOperationException if the cipher isn't initialised.
        * @return the number of bytes processed and produced.
        */
        private int DecryptBlock(
            byte[]      input,
            int         inOff,
            byte[]      outBytes,
            int         outOff)
        {
            if ((inOff + blockSize) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            Array.Copy(input, inOff, cbcNextV, 0, blockSize);

            int length = cipher.ProcessBlock(input, inOff, outBytes, outOff);

            /*
            * XOR the cbcV and the output
            */
            for (int i = 0; i < blockSize; i++)
            {
                outBytes[outOff + i] ^= cbcV[i];
            }

            /*
            * swap the back up buffer into next position
            */
            byte[]  tmp;

            tmp = cbcV;
            cbcV = cbcNextV;
            cbcNextV = tmp;

            return length;
        }
    }

}
