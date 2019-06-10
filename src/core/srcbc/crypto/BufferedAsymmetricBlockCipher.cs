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
using System.Diagnostics;

using Org.BouncyCastle.Crypto.Engines;

namespace Org.BouncyCastle.Crypto
{
    /**
    * a buffer wrapper for an asymmetric block cipher, allowing input
    * to be accumulated in a piecemeal fashion until final processing.
    */
    public class BufferedAsymmetricBlockCipher
		: BufferedCipherBase
    {
		private readonly IAsymmetricBlockCipher cipher;

		private byte[] buffer;
		private int bufOff;

		/**
        * base constructor.
        *
        * @param cipher the cipher this buffering object wraps.
        */
        public BufferedAsymmetricBlockCipher(
            IAsymmetricBlockCipher cipher)
        {
            this.cipher = cipher;
		}

		/**
        * return the amount of data sitting in the buffer.
        *
        * @return the amount of data sitting in the buffer.
        */
        internal int GetBufferPosition()
        {
            return bufOff;
        }

		public override string AlgorithmName
        {
            get { return cipher.AlgorithmName; }
        }

		public override int GetBlockSize()
        {
			return cipher.GetInputBlockSize();
        }

		public override int GetOutputSize(
			int length)
		{
			return cipher.GetOutputBlockSize();
		}

		public override int GetUpdateOutputSize(
			int length)
		{
			return 0;
		}

		/**
        * initialise the buffer and the underlying cipher.
        *
        * @param forEncryption if true the cipher is initialised for
        *  encryption, if false for decryption.
        * @param param the key and other data required by the cipher.
        */
        public override void Init(
            bool				forEncryption,
            ICipherParameters	parameters)
        {
			Reset();

			cipher.Init(forEncryption, parameters);

			//
			// we allow for an extra byte where people are using their own padding
			// mechanisms on a raw cipher.
			//
			this.buffer = new byte[cipher.GetInputBlockSize() + (forEncryption ? 1 : 0)];
			this.bufOff = 0;
        }

		public override byte[] ProcessByte(
			byte input)
		{
			if (bufOff >= buffer.Length)
				throw new DataLengthException("attempt to process message to long for cipher");

			buffer[bufOff++] = input;
			return null;
		}

		public override byte[] ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length)
		{
			if (length < 1)
				return null;

			if (input == null)
				throw new ArgumentNullException("input");
			if (bufOff + length > buffer.Length)
				throw new DataLengthException("attempt to process message to long for cipher");

			Array.Copy(input, inOff, buffer, bufOff, length);
			bufOff += length;
			return null;
		}

		/**
        * process the contents of the buffer using the underlying
        * cipher.
        *
        * @return the result of the encryption/decryption process on the
        * buffer.
        * @exception InvalidCipherTextException if we are given a garbage block.
        */
        public override byte[] DoFinal()
        {
			byte[] outBytes = bufOff > 0
				?	cipher.ProcessBlock(buffer, 0, bufOff)
				:	EmptyBuffer;

			Reset();

			return outBytes;
        }

		public override byte[] DoFinal(
			byte[]	input,
			int		inOff,
			int		length)
		{
			ProcessBytes(input, inOff, length);
			return DoFinal();
		}

		/// <summary>Reset the buffer</summary>
        public override void Reset()
        {
			if (buffer != null)
			{
				Array.Clear(buffer, 0, buffer.Length);
				bufOff = 0;
			}
        }
    }
}
