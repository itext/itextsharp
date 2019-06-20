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

using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	 * an implementation of the RFC 3211 Key Wrap
	 * Specification.
	 */
	public class Rfc3211WrapEngine
		: IWrapper
	{
		private CbcBlockCipher		engine;
		private ParametersWithIV	param;
		private bool				forWrapping;
		private SecureRandom		rand;

		public Rfc3211WrapEngine(
			IBlockCipher engine)
		{
			this.engine = new CbcBlockCipher(engine);
		}

		public void Init(
			bool				forWrapping,
			ICipherParameters	param)
		{
			this.forWrapping = forWrapping;

			if (param is ParametersWithRandom)
			{
				ParametersWithRandom p = (ParametersWithRandom) param;

				this.rand = p.Random;
				this.param = (ParametersWithIV) p.Parameters;
			}
			else
			{
				if (forWrapping)
				{
					rand = new SecureRandom();
				}

				this.param = (ParametersWithIV) param;
			}
		}

		public string AlgorithmName
		{
			get { return engine.GetUnderlyingCipher().AlgorithmName + "/RFC3211Wrap"; }
		}

		public byte[] Wrap(
			byte[]	inBytes,
			int		inOff,
			int		inLen)
		{
			if (!forWrapping)
			{
				throw new InvalidOperationException("not set for wrapping");
			}

			engine.Init(true, param);

			int blockSize = engine.GetBlockSize();
			byte[] cekBlock;

			if (inLen + 4 < blockSize * 2)
			{
				cekBlock = new byte[blockSize * 2];
			}
			else
			{
				cekBlock = new byte[(inLen + 4) % blockSize == 0 ? inLen + 4 : ((inLen + 4) / blockSize + 1) * blockSize];
			}

			cekBlock[0] = (byte)inLen;
			cekBlock[1] = (byte)~inBytes[inOff];
			cekBlock[2] = (byte)~inBytes[inOff + 1];
			cekBlock[3] = (byte)~inBytes[inOff + 2];

			Array.Copy(inBytes, inOff, cekBlock, 4, inLen);

			rand.NextBytes(cekBlock, inLen + 4, cekBlock.Length - inLen - 4);

			for (int i = 0; i < cekBlock.Length; i += blockSize)
			{
				engine.ProcessBlock(cekBlock, i, cekBlock, i);
			}

			for (int i = 0; i < cekBlock.Length; i += blockSize)
			{
				engine.ProcessBlock(cekBlock, i, cekBlock, i);
			}

			return cekBlock;
		}

		public byte[] Unwrap(
			byte[]	inBytes,
			int		inOff,
			int		inLen)
		{
			if (forWrapping)
			{
				throw new InvalidOperationException("not set for unwrapping");
			}

			int blockSize = engine.GetBlockSize();

			if (inLen < 2 * blockSize)
			{
				throw new InvalidCipherTextException("input too short");
			}

			byte[] cekBlock = new byte[inLen];
			byte[] iv = new byte[blockSize];

			Array.Copy(inBytes, inOff, cekBlock, 0, inLen);
			Array.Copy(inBytes, inOff, iv, 0, iv.Length);

			engine.Init(false, new ParametersWithIV(param.Parameters, iv));

			for (int i = blockSize; i < cekBlock.Length; i += blockSize)
			{
				engine.ProcessBlock(cekBlock, i, cekBlock, i);    
			}

			Array.Copy(cekBlock, cekBlock.Length - iv.Length, iv, 0, iv.Length);

			engine.Init(false, new ParametersWithIV(param.Parameters, iv));

			engine.ProcessBlock(cekBlock, 0, cekBlock, 0);

			engine.Init(false, param);

			for (int i = 0; i < cekBlock.Length; i += blockSize)
			{
				engine.ProcessBlock(cekBlock, i, cekBlock, i);
			}

			if ((cekBlock[0] & 0xff) > cekBlock.Length - 4)
			{
				throw new InvalidCipherTextException("wrapped key corrupted");
			}

			byte[] key = new byte[cekBlock[0] & 0xff];

			Array.Copy(cekBlock, 4, key, 0, cekBlock[0]);

			// Note: Using constant time comparison
			int nonEqual = 0;
			for (int i = 0; i != 3; i++)
			{
				byte check = (byte)~cekBlock[1 + i];
				nonEqual |= (check ^ key[i]);
			}

			if (nonEqual != 0)
				throw new InvalidCipherTextException("wrapped key fails checksum");

			return key;
		}
	}
}
