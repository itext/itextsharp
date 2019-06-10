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

namespace Org.BouncyCastle.Crypto.IO
{
	public class SignerStream
		: Stream
	{
		protected readonly Stream stream;
		protected readonly ISigner inSigner;
		protected readonly ISigner outSigner;

		public SignerStream(
			Stream	stream,
			ISigner	readSigner,
			ISigner	writeSigner)
		{
			this.stream = stream;
			this.inSigner = readSigner;
			this.outSigner = writeSigner;
		}

		public virtual ISigner ReadSigner()
		{
			return inSigner;
		}

		public virtual ISigner WriteSigner()
		{
			return outSigner;
		}

		public override int Read(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			int n = stream.Read(buffer, offset, count);
			if (inSigner != null)
			{
				if (n > 0)
				{
					inSigner.BlockUpdate(buffer, offset, n);
				}
			}
			return n;
		}

		public override int ReadByte()
		{
			int b = stream.ReadByte();
			if (inSigner != null)
			{
				if (b >= 0)
				{
					inSigner.Update((byte)b);
				}
			}
			return b;
		}

		public override void Write(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			if (outSigner != null)
			{
				if (count > 0)
				{
					outSigner.BlockUpdate(buffer, offset, count);
				}
			}
			stream.Write(buffer, offset, count);
		}

		public override void WriteByte(
			byte b)
		{
			if (outSigner != null)
			{
				outSigner.Update(b);
			}
			stream.WriteByte(b);
		}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override void Close()
		{
			stream.Close();
		}

		public override  void Flush()
		{
			stream.Flush();
		}

		public override long Seek(
			long		offset,
			SeekOrigin	origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(
			long length)
		{
			stream.SetLength(length);
		}
	}
}

