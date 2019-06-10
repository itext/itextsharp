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

    /// <summary> A padder that adds Trailing-Bit-Compliment padding to a block.
    /// <p>
    /// This padding pads the block out compliment of the last bit
    /// of the plain text.
    /// </p>
    /// </summary>
    public class TbcPadding
		: IBlockCipherPadding
    {
        /// <summary> Return the name of the algorithm the cipher implements.</summary>
        /// <returns> the name of the algorithm the cipher implements.
        /// </returns>
        public string PaddingName
        {
            get { return "TBC"; }
        }

		/// <summary> Initialise the padder.</summary>
        /// <param name="random">- a SecureRandom if available.
        /// </param>
        public virtual void Init(SecureRandom random)
        {
            // nothing to do.
        }

        /// <summary> add the pad bytes to the passed in block, returning the
        /// number of bytes added.
        /// <p>
        /// Note: this assumes that the last block of plain text is always
        /// passed to it inside in. i.e. if inOff is zero, indicating the
        /// entire block is to be overwritten with padding the value of in
        /// should be the same as the last block of plain text.
        /// </p>
        /// </summary>
        public virtual int AddPadding(byte[] input, int inOff)
        {
            int count = input.Length - inOff;
            byte code;

            if (inOff > 0)
            {
                code = (byte)((input[inOff - 1] & 0x01) == 0?0xff:0x00);
            }
            else
            {
                code = (byte)((input[input.Length - 1] & 0x01) == 0?0xff:0x00);
            }

            while (inOff < input.Length)
            {
                input[inOff] = code;
                inOff++;
            }

            return count;
        }

        /// <summary> return the number of pad bytes present in the block.</summary>
        public virtual int PadCount(byte[] input)
        {
            byte code = input[input.Length - 1];

            int index = input.Length - 1;
            while (index > 0 && input[index - 1] == code)
            {
                index--;
            }

            return input.Length - index;
        }
    }
}
