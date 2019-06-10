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
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Modes
{
	/**
	* Implements the Segmented Integer Counter (SIC) mode on top of a simple
	* block cipher.
	*/
	public class SicBlockCipher
		: IBlockCipher
	{
		private readonly IBlockCipher cipher;
		private readonly int blockSize;
		private readonly byte[] IV;
		private readonly byte[] counter;
		private readonly byte[] counterOut;

		/**
		* Basic constructor.
		*
		* @param c the block cipher to be used.
		*/
		public SicBlockCipher(IBlockCipher cipher)
		{
			this.cipher = cipher;
			this.blockSize = cipher.GetBlockSize();
			this.IV = new byte[blockSize];
			this.counter = new byte[blockSize];
			this.counterOut = new byte[blockSize];
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

		public void Init(
			bool				forEncryption, //ignored by this CTR mode
			ICipherParameters	parameters)
		{
			if (parameters is ParametersWithIV)
			{
				ParametersWithIV ivParam = (ParametersWithIV) parameters;
				byte[] iv = ivParam.GetIV();
				Array.Copy(iv, 0, IV, 0, IV.Length);

                Reset();

                // if null it's an IV changed only.
                if (ivParam.Parameters != null)
                {
                    cipher.Init(true, ivParam.Parameters);
                }
			}
	        else
	        {
	            throw new ArgumentException("SIC mode requires ParametersWithIV", "parameters");
	        }
		}

		public string AlgorithmName
		{
			get { return cipher.AlgorithmName + "/SIC"; }
		}

		public bool IsPartialBlockOkay
		{
			get { return true; }
		}

		public int GetBlockSize()
		{
			return cipher.GetBlockSize();
		}

		public int ProcessBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff)
		{
			cipher.ProcessBlock(counter, 0, counterOut, 0);

			//
			// XOR the counterOut with the plaintext producing the cipher text
			//
			for (int i = 0; i < counterOut.Length; i++)
			{
				output[outOff + i] = (byte)(counterOut[i] ^ input[inOff + i]);
			}

			// Increment the counter
			int j = counter.Length;
			while (--j >= 0 && ++counter[j] == 0)
			{
			}

			return counter.Length;
		}

		public void Reset()
		{
			Array.Copy(IV, 0, counter, 0, counter.Length);
			cipher.Reset();
		}
	}
}
