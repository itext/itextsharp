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
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Modes
{
	/// <summary>
	/// A block cipher mode that includes authenticated encryption with a streaming mode
	/// and optional associated data.</summary>
	/// <see cref="AeadParameters"/>
	public interface IAeadBlockCipher
	{
		/// <summary>The name of the algorithm this cipher implements.</summary>
		string AlgorithmName { get; }

		/// <summary>Initialise the cipher.</summary>
		/// <remarks>Parameter can either be an AeadParameters or a ParametersWithIV object.</remarks>
		/// <param name="forEncryption">Initialise for encryption if true, for decryption if false.</param>
		/// <param name="parameters">The key or other data required by the cipher.</param>
		void Init(bool forEncryption, ICipherParameters parameters);

		/// <returns>The block size for this cipher, in bytes.</returns>
		int GetBlockSize();

        /// <summary>Add a single byte to the associated data check.</summary>
		/// <remarks>If the implementation supports it, this will be an online operation and will not retain the associated data.</remarks>
        /// <param name="input">The byte to be processed.</param>
        void ProcessAadByte(byte input);

        /// <summary>Add a sequence of bytes to the associated data check.</summary>
		/// <remarks>If the implementation supports it, this will be an online operation and will not retain the associated data.</remarks>
        /// <param name="inBytes">The input byte array.</param>
        /// <param name="inOff">The offset into the input array where the data to be processed starts.</param>
        /// <param name="len">The number of bytes to be processed.</param>
        void ProcessAadBytes(byte[] inBytes, int inOff, int len);

        /**
		* Encrypt/decrypt a single byte.
		*
		* @param input the byte to be processed.
		* @param outBytes the output buffer the processed byte goes into.
		* @param outOff the offset into the output byte array the processed data starts at.
		* @return the number of bytes written to out.
		* @exception DataLengthException if the output buffer is too small.
		*/
		int ProcessByte(byte input, byte[] outBytes, int outOff);

		/**
		* Process a block of bytes from in putting the result into out.
		*
		* @param inBytes the input byte array.
		* @param inOff the offset into the in array where the data to be processed starts.
		* @param len the number of bytes to be processed.
		* @param outBytes the output buffer the processed bytes go into.
		* @param outOff the offset into the output byte array the processed data starts at.
		* @return the number of bytes written to out.
		* @exception DataLengthException if the output buffer is too small.
		*/
		int ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff);

		/**
		* Finish the operation either appending or verifying the MAC at the end of the data.
		*
		* @param outBytes space for any resulting output data.
		* @param outOff offset into out to start copying the data at.
		* @return number of bytes written into out.
		* @throws InvalidOperationException if the cipher is in an inappropriate state.
		* @throws InvalidCipherTextException if the MAC fails to match.
		*/
		int DoFinal(byte[] outBytes, int outOff);

		/**
		* Return the value of the MAC associated with the last stream processed.
		*
		* @return MAC for plaintext data.
		*/
		byte[] GetMac();

		/**
		* Return the size of the output buffer required for a ProcessBytes
		* an input of len bytes.
		*
		* @param len the length of the input.
		* @return the space required to accommodate a call to ProcessBytes
		* with len bytes of input.
		*/
		int GetUpdateOutputSize(int len);

		/**
		* Return the size of the output buffer required for a ProcessBytes plus a
		* DoFinal with an input of len bytes.
		*
		* @param len the length of the input.
		* @return the space required to accommodate a call to ProcessBytes and DoFinal
		* with len bytes of input.
		*/
		int GetOutputSize(int len);

		/// <summary>
		/// Reset the cipher to the same state as it was after the last init (if there was one).
		/// </summary>
		void Reset();
	}
}
