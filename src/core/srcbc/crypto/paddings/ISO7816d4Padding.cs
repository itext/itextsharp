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
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Paddings
{
	/**
	 * A padder that adds the padding according to the scheme referenced in
	 * ISO 7814-4 - scheme 2 from ISO 9797-1. The first byte is 0x80, rest is 0x00
	 */
	public class ISO7816d4Padding
		: IBlockCipherPadding
	{
		/**
		 * Initialise the padder.
		 *
		 * @param random - a SecureRandom if available.
		 */
		public void Init(
			SecureRandom random)
		{
			// nothing to do.
		}

		/**
		 * Return the name of the algorithm the padder implements.
		 *
		 * @return the name of the algorithm the padder implements.
		 */
		public string PaddingName
		{
			get { return "ISO7816-4"; }
		}

		/**
		 * add the pad bytes to the passed in block, returning the
		 * number of bytes added.
		 */
		public int AddPadding(
			byte[]	input,
			int		inOff)
		{
			int added = (input.Length - inOff);

			input[inOff]= (byte) 0x80;
			inOff ++;

			while (inOff < input.Length)
			{
				input[inOff] = (byte) 0;
				inOff++;
			}

			return added;
		}

		/**
		 * return the number of pad bytes present in the block.
		 */
		public int PadCount(
			byte[] input)
		{
			int count = input.Length - 1;

			while (count > 0 && input[count] == 0)
			{
				count--;
			}

			if (input[count] != (byte)0x80)
			{
				throw new InvalidCipherTextException("pad block corrupted");
			}

			return input.Length - count;
		}
	}
}
