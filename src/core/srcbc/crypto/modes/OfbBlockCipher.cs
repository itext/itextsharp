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
    * implements a Output-FeedBack (OFB) mode on top of a simple cipher.
    */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class OfbBlockCipher
		: IBlockCipher
    {
        private byte[]	IV;
        private byte[]	ofbV;
        private byte[]	ofbOutV;

        private readonly int			blockSize;
        private readonly IBlockCipher	cipher;

        /**
        * Basic constructor.
        *
        * @param cipher the block cipher to be used as the basis of the
        * feedback mode.
        * @param blockSize the block size in bits (note: a multiple of 8)
        */
        public OfbBlockCipher(
            IBlockCipher cipher,
            int         blockSize)
        {
            this.cipher = cipher;
            this.blockSize = blockSize / 8;

            this.IV = new byte[cipher.GetBlockSize()];
            this.ofbV = new byte[cipher.GetBlockSize()];
            this.ofbOutV = new byte[cipher.GetBlockSize()];
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
        * An IV which is too short is handled in FIPS compliant fashion.
        *
        * @param forEncryption if true the cipher is initialised for
        *  encryption, if false for decryption.
        * @param param the key and other data required by the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public void Init(
            bool				forEncryption, //ignored by this OFB mode
            ICipherParameters	parameters)
        {
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

            // if it's null, key is to be reused.
            if (parameters != null)
            {
                cipher.Init(true, parameters);
            }
        }

        /**
        * return the algorithm name and mode.
        *
        * @return the name of the underlying algorithm followed by "/OFB"
        * and the block size in bits
        */
        public string AlgorithmName
        {
            get { return cipher.AlgorithmName + "/OFB" + (blockSize * 8); }
        }

		public bool IsPartialBlockOkay
		{
			get { return true; }
		}

		/**
        * return the block size we are operating at (in bytes).
        *
        * @return the block size we are operating at (in bytes).
        */
        public int GetBlockSize()
        {
            return blockSize;
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
            if ((inOff + blockSize) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + blockSize) > output.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            cipher.ProcessBlock(ofbV, 0, ofbOutV, 0);

            //
            // XOR the ofbV with the plaintext producing the cipher text (and
            // the next input block).
            //
            for (int i = 0; i < blockSize; i++)
            {
                output[outOff + i] = (byte)(ofbOutV[i] ^ input[inOff + i]);
            }

            //
            // change over the input block.
            //
            Array.Copy(ofbV, blockSize, ofbV, 0, ofbV.Length - blockSize);
            Array.Copy(ofbOutV, 0, ofbV, ofbV.Length - blockSize, blockSize);

            return blockSize;
        }

        /**
        * reset the feedback vector back to the IV and reset the underlying
        * cipher.
        */
        public void Reset()
        {
            Array.Copy(IV, 0, ofbV, 0, IV.Length);

            cipher.Reset();
        }
    }

}
