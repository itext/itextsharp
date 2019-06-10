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

namespace Org.BouncyCastle.Crypto
{
	/**
	 * a wrapper for block ciphers with a single byte block size, so that they
	 * can be treated like stream ciphers.
	 */
	public class StreamBlockCipher
		: IStreamCipher
	{
		private readonly IBlockCipher cipher;
		private readonly byte[] oneByte = new byte[1];

		/**
		 * basic constructor.
		 *
		 * @param cipher the block cipher to be wrapped.
		 * @exception ArgumentException if the cipher has a block size other than
		 * one.
		 */
		public StreamBlockCipher(
			IBlockCipher cipher)
		{
			if (cipher == null)
				throw new ArgumentNullException("cipher");
			if (cipher.GetBlockSize() != 1)
				throw new ArgumentException("block cipher block size != 1.", "cipher");

			this.cipher = cipher;
		}

		/**
		 * initialise the underlying cipher.
		 *
		 * @param forEncryption true if we are setting up for encryption, false otherwise.
		 * @param param the necessary parameters for the underlying cipher to be initialised.
		 */
		public void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			cipher.Init(forEncryption, parameters);
		}

		/**
		* return the name of the algorithm we are wrapping.
		*
		* @return the name of the algorithm we are wrapping.
		*/
		public string AlgorithmName
		{
			get { return cipher.AlgorithmName; }
		}

		/**
		* encrypt/decrypt a single byte returning the result.
		*
		* @param in the byte to be processed.
		* @return the result of processing the input byte.
		*/
		public byte ReturnByte(
			byte input)
		{
			oneByte[0] = input;

			cipher.ProcessBlock(oneByte, 0, oneByte, 0);

			return oneByte[0];
		}

		/**
		* process a block of bytes from in putting the result into out.
		*
		* @param in the input byte array.
		* @param inOff the offset into the in array where the data to be processed starts.
		* @param len the number of bytes to be processed.
		* @param out the output buffer the processed bytes go into.
		* @param outOff the offset into the output byte array the processed data stars at.
		* @exception DataLengthException if the output buffer is too small.
		*/
		public void ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length,
			byte[]	output,
			int		outOff)
		{
			if (outOff + length > output.Length)
				throw new DataLengthException("output buffer too small in ProcessBytes()");

			for (int i = 0; i != length; i++)
			{
				cipher.ProcessBlock(input, inOff + i, output, outOff + i);
			}
		}

		/**
		* reset the underlying cipher. This leaves it in the same state
		* it was at after the last init (if there was one).
		*/
		public void Reset()
		{
			cipher.Reset();
		}
	}
}
