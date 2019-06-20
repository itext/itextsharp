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

namespace Org.BouncyCastle.Utilities.Encoders
{
    /// <summary>
    /// A hex translator.
    /// </summary>
    public class HexTranslator : ITranslator
    {
        private static readonly byte[]   hexTable =
            {
                (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f'
            };

        /// <summary>
        /// Return encoded block size.
        /// </summary>
        /// <returns>2</returns>
        public int GetEncodedBlockSize()
        {
            return 2;
        }

        /// <summary>
        /// Encode some data.
        /// </summary>
        /// <param name="input">Input data array.</param>
        /// <param name="inOff">Start position within input data array.</param>
        /// <param name="length">The amount of data to process.</param>
        /// <param name="outBytes">The output data array.</param>
        /// <param name="outOff">The offset within the output data array to start writing from.</param>
        /// <returns>Amount of data encoded.</returns>
        public int Encode(
            byte[]  input,
            int     inOff,
            int     length,
            byte[]  outBytes,
            int     outOff)
        {
            for (int i = 0, j = 0; i < length; i++, j += 2)
            {
                outBytes[outOff + j] = hexTable[(input[inOff] >> 4) & 0x0f];
                outBytes[outOff + j + 1] = hexTable[input[inOff] & 0x0f];

                inOff++;
            }

            return length * 2;
        }

        /// <summary>
        /// Returns the decoded block size.
        /// </summary>
        /// <returns>1</returns>
        public int GetDecodedBlockSize()
        {
            return 1;
        }

        /// <summary>
        /// Decode data from a byte array.
        /// </summary>
        /// <param name="input">The input data array.</param>
        /// <param name="inOff">Start position within input data array.</param>
        /// <param name="length">The amounty of data to process.</param>
        /// <param name="outBytes">The output data array.</param>
        /// <param name="outOff">The position within the output data array to start writing from.</param>
        /// <returns>The amount of data written.</returns>
        public int Decode(
            byte[]  input,
            int     inOff,
            int     length,
            byte[]  outBytes,
            int     outOff)
        {
            int halfLength = length / 2;
            byte left, right;
            for (int i = 0; i < halfLength; i++)
            {
                left  = input[inOff + i * 2];
                right = input[inOff + i * 2 + 1];

                if (left < (byte)'a')
                {
                    outBytes[outOff] = (byte)((left - '0') << 4);
                }
                else
                {
                    outBytes[outOff] = (byte)((left - 'a' + 10) << 4);
                }
                if (right < (byte)'a')
                {
                    outBytes[outOff] += (byte)(right - '0');
                }
                else
                {
                    outBytes[outOff] += (byte)(right - 'a' + 10);
                }

                outOff++;
            }

            return halfLength;
        }
    }

}
