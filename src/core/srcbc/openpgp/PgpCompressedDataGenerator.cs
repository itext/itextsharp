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

using Org.BouncyCastle.Apache.Bzip2;
using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Class for producing compressed data packets.</remarks>
	public class PgpCompressedDataGenerator
		: IStreamGenerator
	{
		private readonly CompressionAlgorithmTag algorithm;
		private readonly int compression;

		private Stream dOut;
		private BcpgOutputStream pkOut;

		public PgpCompressedDataGenerator(
			CompressionAlgorithmTag algorithm)
			: this(algorithm, JZlib.Z_DEFAULT_COMPRESSION)
		{
		}

		public PgpCompressedDataGenerator(
			CompressionAlgorithmTag	algorithm,
			int						compression)
		{
			switch (algorithm)
			{
				case CompressionAlgorithmTag.Uncompressed:
				case CompressionAlgorithmTag.Zip:
				case CompressionAlgorithmTag.ZLib:
				case CompressionAlgorithmTag.BZip2:
					break;
				default:
					throw new ArgumentException("unknown compression algorithm", "algorithm");
			}

			if (compression != JZlib.Z_DEFAULT_COMPRESSION)
			{
				if ((compression < JZlib.Z_NO_COMPRESSION) || (compression > JZlib.Z_BEST_COMPRESSION))
				{
					throw new ArgumentException("unknown compression level: " + compression);
				}
			}

			this.algorithm = algorithm;
			this.compression = compression;
		}

		/// <summary>
		/// <p>
		/// Return an output stream which will save the data being written to
		/// the compressed object.
		/// </p>
		/// <p>
		/// The stream created can be closed off by either calling Close()
		/// on the stream or Close() on the generator. Closing the returned
		/// stream does not close off the Stream parameter <c>outStr</c>.
		/// </p>
		/// </summary>
		/// <param name="outStr">Stream to be used for output.</param>
		/// <returns>A Stream for output of the compressed data.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="IOException"></exception>
		public Stream Open(
			Stream outStr)
		{
			if (dOut != null)
				throw new InvalidOperationException("generator already in open state");
			if (outStr == null)
				throw new ArgumentNullException("outStr");

			this.pkOut = new BcpgOutputStream(outStr, PacketTag.CompressedData);

			doOpen();

			return new WrappedGeneratorStream(this, dOut);
		}

		/// <summary>
		/// <p>
		/// Return an output stream which will compress the data as it is written to it.
		/// The stream will be written out in chunks according to the size of the passed in buffer.
		/// </p>
		/// <p>
		/// The stream created can be closed off by either calling Close()
		/// on the stream or Close() on the generator. Closing the returned
		/// stream does not close off the Stream parameter <c>outStr</c>.
		/// </p>
		/// <p>
		/// <b>Note</b>: if the buffer is not a power of 2 in length only the largest power of 2
		/// bytes worth of the buffer will be used.
		/// </p>
		/// <p>
		/// <b>Note</b>: using this may break compatibility with RFC 1991 compliant tools.
		/// Only recent OpenPGP implementations are capable of accepting these streams.
		/// </p>
		/// </summary>
		/// <param name="outStr">Stream to be used for output.</param>
		/// <param name="buffer">The buffer to use.</param>
		/// <returns>A Stream for output of the compressed data.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PgpException"></exception>
		public Stream Open(
			Stream	outStr,
			byte[]	buffer)
		{
			if (dOut != null)
				throw new InvalidOperationException("generator already in open state");
			if (outStr == null)
				throw new ArgumentNullException("outStr");
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			this.pkOut = new BcpgOutputStream(outStr, PacketTag.CompressedData, buffer);

			doOpen();

			return new WrappedGeneratorStream(this, dOut);
		}

		private void doOpen()
		{
			pkOut.WriteByte((byte) algorithm);

			switch (algorithm)
			{
				case CompressionAlgorithmTag.Uncompressed:
					dOut = pkOut;
					break;
				case CompressionAlgorithmTag.Zip:
					dOut = new SafeZOutputStream(pkOut, compression, true);
					break;
				case CompressionAlgorithmTag.ZLib:
					dOut = new SafeZOutputStream(pkOut, compression, false);
					break;
				case CompressionAlgorithmTag.BZip2:
					dOut = new SafeCBZip2OutputStream(pkOut);
					break;
				default:
					// Constructor should guard against this possibility
					throw new InvalidOperationException();
			}
		}

		/// <summary>Close the compressed object.</summary>summary>
		public void Close()
		{
			if (dOut != null)
			{
				if (dOut != pkOut)
				{
					dOut.Close();
					dOut.Flush();
				}

				dOut = null;

				pkOut.Finish();
				pkOut.Flush();
				pkOut = null;
			}
		}

		private class SafeCBZip2OutputStream : CBZip2OutputStream
		{
			public SafeCBZip2OutputStream(Stream output)
				: base(output)
			{
			}

			public override void Close()
			{
				Finish();
			}
		}

		private class SafeZOutputStream : ZOutputStream
		{
			public SafeZOutputStream(Stream output, int level, bool nowrap)
				: base(output, level, nowrap)
			{
			}

			public override void Close()
			{
				Finish();
				End();
			}
		}
	}
}
