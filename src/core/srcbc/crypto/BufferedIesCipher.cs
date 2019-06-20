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
using System.IO;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto
{
	public class BufferedIesCipher
		: BufferedCipherBase
	{
		private readonly IesEngine engine;
		private bool forEncryption;
		private MemoryStream buffer = new MemoryStream();

		public BufferedIesCipher(
			IesEngine engine)
		{
			if (engine == null)
				throw new ArgumentNullException("engine");

			this.engine = engine;
		}

		public override string AlgorithmName
		{
			// TODO Create IESEngine.AlgorithmName
			get { return "IES"; }
		}

		public override void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			this.forEncryption = forEncryption;

			// TODO
			throw Platform.CreateNotImplementedException("IES");
		}

		public override int GetBlockSize()
		{
			return 0;
		}

		public override int GetOutputSize(
			int inputLen)
		{
			if (engine == null)
				throw new InvalidOperationException("cipher not initialised");

			int baseLen = inputLen + (int) buffer.Length;
			return forEncryption
				?	baseLen + 20
				:	baseLen - 20;
		}

		public override int GetUpdateOutputSize(
			int inputLen)
		{
			return 0;
		}

		public override byte[] ProcessByte(
			byte input)
		{
			buffer.WriteByte(input);
			return null;
		}

		public override byte[] ProcessBytes(
			byte[]	input,
			int		inOff,
			int		length)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			if (inOff < 0)
				throw new ArgumentException("inOff");
			if (length < 0)
				throw new ArgumentException("length");
			if (inOff + length > input.Length)
				throw new ArgumentException("invalid offset/length specified for input array");

			buffer.Write(input, inOff, length);
			return null;
		}

		public override byte[] DoFinal()
		{
			byte[] buf = buffer.ToArray();

			Reset();

			return engine.ProcessBlock(buf, 0, buf.Length);
		}

		public override byte[] DoFinal(
			byte[]	input,
			int		inOff,
			int		length)
		{
			ProcessBytes(input, inOff, length);
			return DoFinal();
		}

		public override void Reset()
		{
			buffer.SetLength(0);
		}
	}
}
