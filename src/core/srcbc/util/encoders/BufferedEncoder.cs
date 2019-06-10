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
    /// A class that allows encoding of data using a specific encoder to be processed in chunks.
    /// </summary>
    public class BufferedEncoder
    {
        internal byte[]        Buffer;
        internal int           bufOff;

        internal ITranslator   translator;


        /// <summary>
        /// Create.
        /// </summary>
        /// <param name="translator">The translator to use.</param>
        /// <param name="bufferSize">Size of the chunks.</param>
        public BufferedEncoder(
            ITranslator translator,
            int         bufferSize)
        {
            this.translator = translator;

            if ((bufferSize % translator.GetEncodedBlockSize()) != 0)
            {
                throw new ArgumentException("buffer size not multiple of input block size");
            }

            Buffer = new byte[bufferSize];
//            bufOff = 0;
        }


        /// <summary>
        /// Process one byte of data.
        /// </summary>
        /// <param name="input">The byte.</param>
        /// <param name="outBytes">An array to store output in.</param>
        /// <param name="outOff">Offset within output array to start writing from.</param>
        /// <returns></returns>
        public int ProcessByte(
            byte        input,
            byte[]      outBytes,
            int         outOff)
        {
            int         resultLen = 0;

            Buffer[bufOff++] = input;

            if (bufOff == Buffer.Length)
            {
                resultLen = translator.Encode(Buffer, 0, Buffer.Length, outBytes, outOff);
                bufOff = 0;
            }

            return resultLen;
        }

        /// <summary>
        /// Process data from a byte array.
        /// </summary>
        /// <param name="input">Input data Byte array containing data to be processed.</param>
        /// <param name="inOff">Start position within input data array.</param>
        /// <param name="len">Amount of input data to be processed.</param>
        /// <param name="outBytes">Output data array.</param>
        /// <param name="outOff">Offset within output data array to start writing to.</param>
        /// <returns>The amount of data written.</returns>
        public int ProcessBytes(
            byte[]      input,
            int         inOff,
            int         len,
            byte[]      outBytes,
            int         outOff)
        {
            if (len < 0)
            {
            throw new ArgumentException("Can't have a negative input length!");
            }

            int resultLen = 0;
            int gapLen = Buffer.Length - bufOff;

            if (len > gapLen)
            {
                Array.Copy(input, inOff, Buffer, bufOff, gapLen);

                resultLen += translator.Encode(Buffer, 0, Buffer.Length, outBytes, outOff);

                bufOff = 0;

                len -= gapLen;
                inOff += gapLen;
                outOff += resultLen;

                int chunkSize = len - (len % Buffer.Length);

                resultLen += translator.Encode(input, inOff, chunkSize, outBytes, outOff);

                len -= chunkSize;
                inOff += chunkSize;
            }

            if (len != 0)
            {
                Array.Copy(input, inOff, Buffer, bufOff, len);

                bufOff += len;
            }

            return resultLen;
        }
    }

}
