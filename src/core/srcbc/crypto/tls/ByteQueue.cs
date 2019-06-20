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

namespace Org.BouncyCastle.Crypto.Tls
{
    /// <remarks>
    /// A queue for bytes.
    /// <p>
    /// This file could be more optimized.
    /// </p>
    /// </remarks>
    public class ByteQueue
    {
        /// <returns>The smallest number which can be written as 2^x which is bigger than i.</returns>
        public static int NextTwoPow(
            int i)
        {
            /*
            * This code is based of a lot of code I found on the Internet
            * which mostly referenced a book called "Hacking delight".
            *
            */
            i |= (i >> 1);
            i |= (i >> 2);
            i |= (i >> 4);
            i |= (i >> 8);
            i |= (i >> 16);
            return i + 1;
        }

        /**
         * The initial size for our buffer.
         */
        private const int DefaultCapacity = 1024;

        /**
         * The buffer where we store our data.
         */
        private byte[] databuf;

        /**
         * How many bytes at the beginning of the buffer are skipped.
         */
        private int skipped = 0;

        /**
         * How many bytes in the buffer are valid data.
         */
        private int available = 0;

        public ByteQueue()
            : this(DefaultCapacity)
        {
        }

        public ByteQueue(int capacity)
        {
            this.databuf = new byte[capacity];
        }

        /// <summary>Read data from the buffer.</summary>
        /// <param name="buf">The buffer where the read data will be copied to.</param>
        /// <param name="offset">How many bytes to skip at the beginning of buf.</param>
        /// <param name="len">How many bytes to read at all.</param>
        /// <param name="skip">How many bytes from our data to skip.</param>
        public void Read(
            byte[]	buf,
            int		offset,
            int		len,
            int		skip)
        {
            if ((available - skip) < len)
            {
                throw new TlsException("Not enough data to read");
            }
            if ((buf.Length - offset) < len)
            {
                throw new TlsException("Buffer size of " + buf.Length + " is too small for a read of " + len + " bytes");
            }
            Array.Copy(databuf, skipped + skip, buf, offset, len);
        }

        /// <summary>Add some data to our buffer.</summary>
        /// <param name="data">A byte-array to read data from.</param>
        /// <param name="offset">How many bytes to skip at the beginning of the array.</param>
        /// <param name="len">How many bytes to read from the array.</param>
        public void AddData(
            byte[]	data,
            int		offset,
            int		len)
        {
            if ((skipped + available + len) > databuf.Length)
            {
                int desiredSize = ByteQueue.NextTwoPow(available + len);
                if (desiredSize > databuf.Length)
                {
                    byte[] tmp = new byte[desiredSize];
                    Array.Copy(databuf, skipped, tmp, 0, available);
                    databuf = tmp;
                }
                else
                {
                    Array.Copy(databuf, skipped, databuf, 0, available);
                }
                skipped = 0;
            }

            Array.Copy(data, offset, databuf, skipped + available, len);
            available += len;
        }

        /// <summary>Remove some bytes from our data from the beginning.</summary>
        /// <param name="i">How many bytes to remove.</param>
        public void RemoveData(
            int i)
        {
            if (i > available)
            {
                throw new TlsException("Cannot remove " + i + " bytes, only got " + available);
            }

            /*
            * Skip the data.
            */
            available -= i;
            skipped += i;
        }

        public void RemoveData(byte[] buf, int off, int len, int skip)
        {
            Read(buf, off, len, skip);
            RemoveData(skip + len);
        }

        public byte[] RemoveData(int len, int skip)
        {
            byte[] buf = new byte[len];
            RemoveData(buf, 0, len, skip);
            return buf;
        }

        /// <summary>The number of bytes which are available in this buffer.</summary>
        public int Available
        {
            get { return available; }
        }
    }
}
