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
	public class BufferedStreamCipher
		: BufferedCipherBase
	{
		private readonly IStreamCipher cipher;

		public BufferedStreamCipher(
			IStreamCipher cipher)
		{
			if (cipher == null)
				throw new ArgumentNullException("cipher");

			this.cipher = cipher;
		}

		public override string AlgorithmName
		{
			get { return cipher.AlgorithmName; }
		}

		public override void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom) parameters).Parameters;
			}

			cipher.Init(forEncryption, parameters);
		}

		public override int GetBlockSize()
		{
			return 0;
		}

		public override int GetOutputSize(
			int inputLen)
		{
			return inputLen;
		}

		public override int GetUpdateOutputSize(
			int inputLen)
		{
			return inputLen;
		}

		public override byte[] ProcessByte(
			byte input)
		{
			return new byte[]{ cipher.ReturnByte(input) };
		}

		public override int ProcessByte(
			byte	input,
			byte[]	output,
			int		outOff)
		{
			if (outOff >= output.Length)
				throw new DataLengthException("output buffer too short");

			output[outOff] = cipher.ReturnByte(input);
			return 1;
		}

		public override byte[] ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length)
		{
			if (length < 1)
				return null;

			byte[] output = new byte[length];
			cipher.ProcessBytes(input, inOff, length, output, 0);
			return output;
		}

		public override int ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length,
			byte[]	output,
			int		outOff)
		{
			if (length < 1)
				return 0;

			if (length > 0)
			{
				cipher.ProcessBytes(input, inOff, length, output, outOff);
			}

			return length;
		}

		public override byte[] DoFinal()
		{
			Reset();

			return EmptyBuffer;
		}

		public override byte[] DoFinal(
			byte[]	input,
			int		inOff,
			int		length)
		{
			if (length < 1)
				return EmptyBuffer;

			byte[] output = ProcessBytes(input, inOff, length);

			Reset();

			return output;
		}

		public override void Reset()
		{
			cipher.Reset();
		}
	}
}
