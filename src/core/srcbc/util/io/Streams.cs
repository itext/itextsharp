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

namespace Org.BouncyCastle.Utilities.IO
{
	public sealed class Streams
	{
		private const int BufferSize = 512;

		private Streams()
		{
		}

		public static void Drain(Stream inStr)
		{
			byte[] bs = new byte[BufferSize];
			while (inStr.Read(bs, 0, bs.Length) > 0)
			{
			}
		}

		public static byte[] ReadAll(Stream inStr)
		{
			MemoryStream buf = new MemoryStream();
			PipeAll(inStr, buf);
			return buf.ToArray();
		}

		public static byte[] ReadAllLimited(Stream inStr, int limit)
		{
			MemoryStream buf = new MemoryStream();
			PipeAllLimited(inStr, limit, buf);
			return buf.ToArray();
		}

		public static int ReadFully(Stream inStr, byte[] buf)
		{
			return ReadFully(inStr, buf, 0, buf.Length);
		}

		public static int ReadFully(Stream inStr, byte[] buf, int off, int len)
		{
			int totalRead = 0;
			while (totalRead < len)
			{
				int numRead = inStr.Read(buf, off + totalRead, len - totalRead);
				if (numRead < 1)
					break;
				totalRead += numRead;
			}
			return totalRead;
		}

		public static void PipeAll(Stream inStr, Stream outStr)
		{
			byte[] bs = new byte[BufferSize];
			int numRead;
			while ((numRead = inStr.Read(bs, 0, bs.Length)) > 0)
			{
				outStr.Write(bs, 0, numRead);
			}
		}

		/// <summary>
		/// Pipe all bytes from <c>inStr</c> to <c>outStr</c>, throwing <c>StreamFlowException</c> if greater
		/// than <c>limit</c> bytes in <c>inStr</c>.
		/// </summary>
		/// <param name="inStr">
		/// A <see cref="Stream"/>
		/// </param>
		/// <param name="limit">
		/// A <see cref="System.Int64"/>
		/// </param>
		/// <param name="outStr">
		/// A <see cref="Stream"/>
		/// </param>
		/// <returns>The number of bytes actually transferred, if not greater than <c>limit</c></returns>
		/// <exception cref="IOException"></exception>
		public static long PipeAllLimited(Stream inStr, long limit, Stream outStr)
		{
			byte[] bs = new byte[BufferSize];
			long total = 0;
			int numRead;
			while ((numRead = inStr.Read(bs, 0, bs.Length)) > 0)
			{
				total += numRead;
				if (total > limit)
					throw new StreamOverflowException("Data Overflow");
				outStr.Write(bs, 0, numRead);
			}
			return total;
		}
	}
}
