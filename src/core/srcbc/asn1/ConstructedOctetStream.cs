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
using System.IO;

using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Asn1
{
	internal class ConstructedOctetStream
		: BaseInputStream
	{
		private readonly Asn1StreamParser _parser;

		private bool _first = true;
		private Stream _currentStream;

		internal ConstructedOctetStream(
			Asn1StreamParser parser)
		{
			_parser = parser;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_currentStream == null)
			{
				if (!_first)
					return 0;

				Asn1OctetStringParser s = (Asn1OctetStringParser)_parser.ReadObject();

				if (s == null)
					return 0;

				_first = false;
				_currentStream = s.GetOctetStream();
			}

			int totalRead = 0;

			for (;;)
			{
				int numRead = _currentStream.Read(buffer, offset + totalRead, count - totalRead);

				if (numRead > 0)
				{
					totalRead += numRead;

					if (totalRead == count)
						return totalRead;
				}
				else
				{
					Asn1OctetStringParser aos = (Asn1OctetStringParser)_parser.ReadObject();

					if (aos == null)
					{
						_currentStream = null;
						return totalRead;
					}

					_currentStream = aos.GetOctetStream();
				}
			}
		}

		public override int ReadByte()
		{
			if (_currentStream == null)
			{
				if (!_first)
					return 0;

				Asn1OctetStringParser s = (Asn1OctetStringParser)_parser.ReadObject();

				if (s == null)
					return 0;

				_first = false;
				_currentStream = s.GetOctetStream();
			}

			for (;;)
			{
				int b = _currentStream.ReadByte();

				if (b >= 0)
				{
					return b;
				}

				Asn1OctetStringParser aos = (Asn1OctetStringParser)_parser.ReadObject();

				if (aos == null)
				{
					_currentStream = null;
					return -1;
				}

				_currentStream = aos.GetOctetStream();
			}
		}
	}
}
