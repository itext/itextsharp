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

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
    class DefiniteLengthInputStream
        : LimitedInputStream
    {
		private static readonly byte[] EmptyBytes = new byte[0];

		private readonly int _originalLength;
		private int _remaining;

        internal DefiniteLengthInputStream(
            Stream	inStream,
            int		length)
            : base(inStream, length)
        {
			if (length < 0)
				throw new ArgumentException("negative lengths not allowed", "length");

			this._originalLength = length;
			this._remaining = length;

			if (length == 0)
			{
				SetParentEofDetect(true);
			}
        }

		internal int Remaining
		{
			get { return _remaining; }
		}

		public override int ReadByte()
        {
			if (_remaining == 0)
				return -1;

			int b = _in.ReadByte();

			if (b < 0)
				throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);

			if (--_remaining == 0)
			{
				SetParentEofDetect(true);
			}

			return b;
        }

		public override int Read(
			byte[]	buf,
			int		off,
			int		len)
		{
			if (_remaining == 0)
				return 0;

			int toRead = System.Math.Min(len, _remaining);
			int numRead = _in.Read(buf, off, toRead);

			if (numRead < 1)
				throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);

			if ((_remaining -= numRead) == 0)
			{
				SetParentEofDetect(true);
			}

			return numRead;
		}

        internal void ReadAllIntoByteArray(byte[] buf)
        {
            if (_remaining != buf.Length)
                throw new ArgumentException("buffer length not right for data");

            if ((_remaining -= Streams.ReadFully(_in, buf)) != 0)
                throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
            SetParentEofDetect(true);
        }

        internal byte[] ToArray()
		{
			if (_remaining == 0)
				return EmptyBytes;

			byte[] bytes = new byte[_remaining];
			if ((_remaining -= Streams.ReadFully(_in, bytes)) != 0)
				throw new EndOfStreamException("DEF length " + _originalLength + " object truncated by " + _remaining);
			SetParentEofDetect(true);
			return bytes;
		}
    }
}
