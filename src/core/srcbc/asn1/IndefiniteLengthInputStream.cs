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

namespace Org.BouncyCastle.Asn1
{
	class IndefiniteLengthInputStream
		: LimitedInputStream
	{
        private int _lookAhead;
        private bool _eofOn00 = true;

		internal IndefiniteLengthInputStream(
			Stream	inStream,
			int		limit)
			: base(inStream, limit)
		{
            _lookAhead = RequireByte();
            CheckForEof();
		}

		internal void SetEofOn00(
			bool eofOn00)
		{
			_eofOn00 = eofOn00;
            if (_eofOn00)
            {
                CheckForEof();
            }
        }

        private bool CheckForEof()
		{
            if (_lookAhead == 0x00)
            {
                int extra = RequireByte();
                if (extra != 0)
                {
                    throw new IOException("malformed end-of-contents marker");
                }

                _lookAhead = -1;
                SetParentEofDetect(true);
                return true;
            }
            return _lookAhead < 0;
        }

		public override int Read(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			// Only use this optimisation if we aren't checking for 00
			if (_eofOn00 || count <= 1)
				return base.Read(buffer, offset, count);

			if (_lookAhead < 0)
				return 0;

			int numRead = _in.Read(buffer, offset + 1, count - 1);

			if (numRead <= 0)
			{
				// Corrupted stream
				throw new EndOfStreamException();
			}

			buffer[offset] = (byte)_lookAhead;
			_lookAhead = RequireByte();

			return numRead + 1;
		}

		public override int ReadByte()
		{
            if (_eofOn00 && CheckForEof())
				return -1;

            int result = _lookAhead;
            _lookAhead = RequireByte();
            return result;
		}

        private int RequireByte()
        {
            int b = _in.ReadByte();
            if (b < 0)
            {
                // Corrupted stream
                throw new EndOfStreamException();
            }
            return b;
        }
	}
}

//using System;
//using System.IO;

//namespace Org.BouncyCastle.Asn1
//{
//    class IndefiniteLengthInputStream
//        : LimitedInputStream
//    {
//        private bool _eofReached = false;
//        private bool _eofOn00 = true;

//        internal IndefiniteLengthInputStream(
//            Stream	inStream,
//            int		limit)
//            : base(inStream, limit)
//        {
//        }

//        internal void SetEofOn00(
//            bool eofOn00)
//        {
//            _eofOn00 = eofOn00;
//        }

//        public override int Read(
//            byte[]	buffer,
//            int		offset,
//            int		count)
//        {
//            if (_eofReached)
//                return 0;

//            if (_eofOn00)
//                return base.Read(buffer, offset, count);

//            int numRead = _in.Read(buffer, offset, count);

//            if (numRead <= 0)
//                throw new EndOfStreamException();

//            return numRead;
//        }

//        public override int ReadByte()
//        {
//            if (_eofReached)
//                return -1;

//            int b1 = _in.ReadByte();

//            if (b1 < 0)
//                throw new EndOfStreamException();

//            if (b1 == 0 && _eofOn00)
//            {
//                int b2 = _in.ReadByte();

//                if (b2 < 0)
//                    throw new EndOfStreamException();

//                if (b2 == 0)
//                {
//                    _eofReached = true;
//                    SetParentEofDetect(true);
//                    return -1;
//                }

//                throw new InvalidDataException();
//            }

//            return b1;
//        }
//    }
//}
