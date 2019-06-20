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

namespace Org.BouncyCastle.Crypto
{
	/// <summary>The interface stream ciphers conform to.</summary>
    public interface IStreamCipher
    {
		/// <summary>The name of the algorithm this cipher implements.</summary>
		string AlgorithmName { get; }

		/// <summary>Initialise the cipher.</summary>
		/// <param name="forEncryption">If true the cipher is initialised for encryption,
		/// if false for decryption.</param>
		/// <param name="parameters">The key and other data required by the cipher.</param>
		/// <exception cref="ArgumentException">
		/// If the parameters argument is inappropriate.
		/// </exception>
        void Init(bool forEncryption, ICipherParameters parameters);

		/// <summary>encrypt/decrypt a single byte returning the result.</summary>
		/// <param name="input">the byte to be processed.</param>
		/// <returns>the result of processing the input byte.</returns>
        byte ReturnByte(byte input);

		/// <summary>
		/// Process a block of bytes from <c>input</c> putting the result into <c>output</c>.
		/// </summary>
		/// <param name="input">The input byte array.</param>
		/// <param name="inOff">
		/// The offset into <c>input</c> where the data to be processed starts.
		/// </param>
		/// <param name="length">The number of bytes to be processed.</param>
		/// <param name="output">The output buffer the processed bytes go into.</param>
		/// <param name="outOff">
		/// The offset into <c>output</c> the processed data starts at.
		/// </param>
		/// <exception cref="DataLengthException">If the output buffer is too small.</exception>
        void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff);

		/// <summary>
		/// Reset the cipher to the same state as it was after the last init (if there was one).
		/// </summary>
		void Reset();
    }
}
