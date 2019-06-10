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
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/// <remarks>
	/// An implementation of the AES Key Wrapper from the NIST Key Wrap
	/// Specification as described in RFC 3394.
	/// <p/>
	/// For further details see: <a href="http://www.ietf.org/rfc/rfc3394.txt">http://www.ietf.org/rfc/rfc3394.txt</a>
	/// and  <a href="http://csrc.nist.gov/encryption/kms/key-wrap.pdf">http://csrc.nist.gov/encryption/kms/key-wrap.pdf</a>.
	/// </remarks>
	public class Rfc3394WrapEngine
		: IWrapper
	{
		private readonly IBlockCipher engine;

		private KeyParameter	param;
		private bool			forWrapping;

		private byte[] iv =
		{
			0xa6, 0xa6, 0xa6, 0xa6,
			0xa6, 0xa6, 0xa6, 0xa6
		};

		public Rfc3394WrapEngine(
			IBlockCipher engine)
		{
			this.engine = engine;
		}

		public void Init(
			bool				forWrapping,
			ICipherParameters	parameters)
		{
			this.forWrapping = forWrapping;

			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom) parameters).Parameters;
			}

			if (parameters is KeyParameter)
			{
				this.param = (KeyParameter) parameters;
			}
			else if (parameters is ParametersWithIV)
			{
				ParametersWithIV pIV = (ParametersWithIV) parameters;
				byte[] iv = pIV.GetIV();

				if (iv.Length != 8)
					throw new ArgumentException("IV length not equal to 8", "parameters");

				this.iv = iv;
				this.param = (KeyParameter) pIV.Parameters;
			}
			else
			{
				// TODO Throw an exception for bad parameters?
			}
		}

		public string AlgorithmName
		{
			get { return engine.AlgorithmName; }
		}

		public byte[] Wrap(
			byte[]	input,
			int		inOff,
			int		inLen)
		{
			if (!forWrapping)
			{
				throw new InvalidOperationException("not set for wrapping");
			}

			int n = inLen / 8;

			if ((n * 8) != inLen)
			{
				throw new DataLengthException("wrap data must be a multiple of 8 bytes");
			}

			byte[] block = new byte[inLen + iv.Length];
			byte[] buf = new byte[8 + iv.Length];

			Array.Copy(iv, 0, block, 0, iv.Length);
			Array.Copy(input, 0, block, iv.Length, inLen);

			engine.Init(true, param);

			for (int j = 0; j != 6; j++)
			{
				for (int i = 1; i <= n; i++)
				{
					Array.Copy(block, 0, buf, 0, iv.Length);
					Array.Copy(block, 8 * i, buf, iv.Length, 8);
					engine.ProcessBlock(buf, 0, buf, 0);

					int t = n * j + i;
					for (int k = 1; t != 0; k++)
					{
						byte v = (byte)t;

						buf[iv.Length - k] ^= v;
						t = (int) ((uint)t >> 8);
					}

					Array.Copy(buf, 0, block, 0, 8);
					Array.Copy(buf, 8, block, 8 * i, 8);
				}
			}

			return block;
		}

		public byte[] Unwrap(
			byte[]  input,
			int     inOff,
			int     inLen)
		{
			if (forWrapping)
			{
				throw new InvalidOperationException("not set for unwrapping");
			}

			int n = inLen / 8;

			if ((n * 8) != inLen)
			{
				throw new InvalidCipherTextException("unwrap data must be a multiple of 8 bytes");
			}

			byte[]  block = new byte[inLen - iv.Length];
			byte[]  a = new byte[iv.Length];
			byte[]  buf = new byte[8 + iv.Length];

			Array.Copy(input, 0, a, 0, iv.Length);
			Array.Copy(input, iv.Length, block, 0, inLen - iv.Length);

			engine.Init(false, param);

			n = n - 1;

			for (int j = 5; j >= 0; j--)
			{
				for (int i = n; i >= 1; i--)
				{
					Array.Copy(a, 0, buf, 0, iv.Length);
					Array.Copy(block, 8 * (i - 1), buf, iv.Length, 8);

					int t = n * j + i;
					for (int k = 1; t != 0; k++)
					{
						byte v = (byte)t;

						buf[iv.Length - k] ^= v;
						t = (int) ((uint)t >> 8);
					}

					engine.ProcessBlock(buf, 0, buf, 0);
					Array.Copy(buf, 0, a, 0, 8);
					Array.Copy(buf, 8, block, 8 * (i - 1), 8);
				}
			}

			if (!Arrays.ConstantTimeAreEqual(a, iv))
				throw new InvalidCipherTextException("checksum failed");

			return block;
		}
	}
}
