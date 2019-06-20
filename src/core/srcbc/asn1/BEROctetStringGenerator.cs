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
	public class BerOctetStringGenerator
		: BerGenerator
	{
		public BerOctetStringGenerator(Stream outStream)
			: base(outStream)
		{
			WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.OctetString);
		}

		public BerOctetStringGenerator(
			Stream	outStream,
			int		tagNo,
			bool	isExplicit)
			: base(outStream, tagNo, isExplicit)
		{
			WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.OctetString);
		}

		public Stream GetOctetOutputStream()
		{
			return GetOctetOutputStream(new byte[1000]); // limit for CER encoding.
		}

		public Stream GetOctetOutputStream(
			int bufSize)
		{
			return bufSize < 1
				?	GetOctetOutputStream()
				:	GetOctetOutputStream(new byte[bufSize]);
		}

		public Stream GetOctetOutputStream(
			byte[] buf)
		{
			return new BufferedBerOctetStream(this, buf);
		}

		private class BufferedBerOctetStream
			: BaseOutputStream
		{
			private byte[] _buf;
			private int    _off;
			private readonly BerOctetStringGenerator _gen;
			private readonly DerOutputStream _derOut;

			internal BufferedBerOctetStream(
				BerOctetStringGenerator	gen,
				byte[]					buf)
			{
				_gen = gen;
				_buf = buf;
				_off = 0;
				_derOut = new DerOutputStream(_gen.Out);
			}

			public override void WriteByte(
				byte b)
			{
				_buf[_off++] = b;

				if (_off == _buf.Length)
				{
					DerOctetString.Encode(_derOut, _buf, 0, _off);
					_off = 0;
				}
			}

			public override void Write(
				byte[] buf,
				int    offset,
				int    len)
			{
				while (len > 0)
				{
					int numToCopy = System.Math.Min(len, _buf.Length - _off);

					if (numToCopy == _buf.Length)
					{
						DerOctetString.Encode(_derOut, buf, offset, numToCopy);
					}
					else
					{
						Array.Copy(buf, offset, _buf, _off, numToCopy);

						_off += numToCopy;
						if (_off < _buf.Length)
							break;

						DerOctetString.Encode(_derOut, _buf, 0, _off);
						_off = 0;
					}

					offset += numToCopy;
					len -= numToCopy;
				}
			}

			public override void Close()
			{
				if (_off != 0)
				{
					DerOctetString.Encode(_derOut, _buf, 0, _off);
				}

				_gen.WriteBerEnd();
				base.Close();
			}
		}
	}
}
