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

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* The no-op engine that just copies bytes through, irrespective of whether encrypting and decrypting.
	* Provided for the sake of completeness.
	*/
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class NullEngine
		: IBlockCipher
	{
		private bool initialised;
		private const int BlockSize = 1;

		public NullEngine()
		{
		}

		public void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			// we don't mind any parameters that may come in
			initialised = true;
		}

		public string AlgorithmName
		{
			get { return "Null"; }
		}

		public bool IsPartialBlockOkay
		{
			get { return true; }
		}

		public int GetBlockSize()
		{
			return BlockSize;
		}

		public int ProcessBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff)
		{
			if (!initialised)
				throw new InvalidOperationException("Null engine not initialised");
			if ((inOff + BlockSize) > input.Length)
				throw new DataLengthException("input buffer too short");
			if ((outOff + BlockSize) > output.Length)
				throw new DataLengthException("output buffer too short");

			for (int i = 0; i < BlockSize; ++i)
			{
				output[outOff + i] = input[inOff + i];
			}

			return BlockSize;
		}

		public void Reset()
		{
			// nothing needs to be done
		}
	}
}
