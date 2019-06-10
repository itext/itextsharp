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
	public abstract class BufferedCipherBase
		: IBufferedCipher
	{
		protected static readonly byte[] EmptyBuffer = new byte[0];

		public abstract string AlgorithmName { get; }

		public abstract void Init(bool forEncryption, ICipherParameters parameters);

		public abstract int GetBlockSize();

		public abstract int GetOutputSize(int inputLen);
		public abstract int GetUpdateOutputSize(int inputLen);

		public abstract byte[] ProcessByte(byte input);

		public virtual int ProcessByte(
			byte	input,
			byte[]	output,
			int		outOff)
		{
			byte[] outBytes = ProcessByte(input);
			if (outBytes == null)
				return 0;
			if (outOff + outBytes.Length > output.Length)
				throw new DataLengthException("output buffer too short");
			outBytes.CopyTo(output, outOff);
			return outBytes.Length;
		}

		public virtual byte[] ProcessBytes(
			byte[] input)
		{
			return ProcessBytes(input, 0, input.Length);
		}

		public abstract byte[] ProcessBytes(byte[] input, int inOff, int length);

		public virtual int ProcessBytes(
			byte[]	input,
			byte[]	output,
			int		outOff)
		{
			return ProcessBytes(input, 0, input.Length, output, outOff);
		}

		public virtual int ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length,
			byte[]	output,
			int		outOff)
		{
			byte[] outBytes = ProcessBytes(input, inOff, length);
			if (outBytes == null)
				return 0;
			if (outOff + outBytes.Length > output.Length)
				throw new DataLengthException("output buffer too short");
			outBytes.CopyTo(output, outOff);
			return outBytes.Length;
		}

		public abstract byte[] DoFinal();

		public virtual byte[] DoFinal(
			byte[] input)
		{
			return DoFinal(input, 0, input.Length);
		}

		public abstract byte[] DoFinal(
			byte[]	input,
			int		inOff,
			int		length);

		public virtual int DoFinal(
			byte[]	output,
			int		outOff)
		{
			byte[] outBytes = DoFinal();
			if (outOff + outBytes.Length > output.Length)
				throw new DataLengthException("output buffer too short");
			outBytes.CopyTo(output, outOff);
			return outBytes.Length;
		}

		public virtual int DoFinal(
			byte[]	input,
			byte[]	output,
			int		outOff)
		{
			return DoFinal(input, 0, input.Length, output, outOff);
		}

		public virtual int DoFinal(
			byte[]	input,
			int		inOff,
			int		length,
			byte[]	output,
			int		outOff)
		{
			int len = ProcessBytes(input, inOff, length, output, outOff);
			len += DoFinal(output, outOff + len);
			return len;
		}

		public abstract void Reset();
	}
}
