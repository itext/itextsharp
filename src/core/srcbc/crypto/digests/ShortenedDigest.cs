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
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.Crypto.Digests
{
	/**
	* Wrapper class that reduces the output length of a particular digest to
	* only the first n bytes of the digest function.
	*/
	public class ShortenedDigest
		: IDigest
	{
		private IDigest	baseDigest;
		private int		length;

		/**
		* Base constructor.
		*
		* @param baseDigest underlying digest to use.
		* @param length length in bytes of the output of doFinal.
		* @exception ArgumentException if baseDigest is null, or length is greater than baseDigest.GetDigestSize().
		*/
		public ShortenedDigest(
			IDigest	baseDigest,
			int		length)
		{
			if (baseDigest == null)
			{
				throw new ArgumentNullException("baseDigest");
			}

			if (length > baseDigest.GetDigestSize())
			{
				throw new ArgumentException("baseDigest output not large enough to support length");
			}

			this.baseDigest = baseDigest;
			this.length = length;
		}

		public string AlgorithmName
		{
			get { return baseDigest.AlgorithmName + "(" + length * 8 + ")"; }
		}

		public int GetDigestSize()
		{
			return length;
		}

		public void Update(byte input)
		{
			baseDigest.Update(input);
		}

		public void BlockUpdate(byte[] input, int inOff, int length)
		{
			baseDigest.BlockUpdate(input, inOff, length);
		}

		public int DoFinal(byte[] output, int outOff)
		{
			byte[] tmp = new byte[baseDigest.GetDigestSize()];

			baseDigest.DoFinal(tmp, 0);

	        Array.Copy(tmp, 0, output, outOff, length);

			return length;
		}

		public void Reset()
		{
			baseDigest.Reset();
		}

		public int GetByteLength()
		{
			return baseDigest.GetByteLength();
		}
	}
}
