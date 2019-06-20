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
using System.Text;

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Class for producing literal data packets.</remarks>
    public class PgpLiteralDataGenerator
		: IStreamGenerator
	{
        public const char Binary = PgpLiteralData.Binary;
        public const char Text = PgpLiteralData.Text;
		public const char Utf8 = PgpLiteralData.Utf8;

		/// <summary>The special name indicating a "for your eyes only" packet.</summary>
        public const string Console = PgpLiteralData.Console;

		private BcpgOutputStream pkOut;
        private bool oldFormat;

		public PgpLiteralDataGenerator()
        {
        }

		/// <summary>
		/// Generates literal data objects in the old format.
		/// This is important if you need compatibility with PGP 2.6.x.
		/// </summary>
		/// <param name="oldFormat">If true, uses old format.</param>
        public PgpLiteralDataGenerator(
            bool oldFormat)
        {
            this.oldFormat = oldFormat;
        }

		private void WriteHeader(
            BcpgOutputStream	outStr,
            char				format,
            byte[]				encName,
            long				modificationTime)
        {
			outStr.Write(
				(byte) format,
				(byte) encName.Length);

			outStr.Write(encName);

			long modDate = modificationTime / 1000L;

			outStr.Write(
				(byte)(modDate >> 24),
				(byte)(modDate >> 16),
				(byte)(modDate >> 8),
				(byte)modDate);
        }

		/// <summary>
		/// <p>
		/// Open a literal data packet, returning a stream to store the data inside the packet.
		/// </p>
		/// <p>
		/// The stream created can be closed off by either calling Close()
		/// on the stream or Close() on the generator. Closing the returned
		/// stream does not close off the Stream parameter <c>outStr</c>.
		/// </p>
		/// </summary>
		/// <param name="outStr">The stream we want the packet in.</param>
		/// <param name="format">The format we are using.</param>
		/// <param name="name">The name of the 'file'.</param>
		/// <param name="length">The length of the data we will write.</param>
		/// <param name="modificationTime">The time of last modification we want stored.</param>
        public Stream Open(
            Stream		outStr,
            char		format,
            string		name,
            long		length,
            DateTime	modificationTime)
        {
			if (pkOut != null)
				throw new InvalidOperationException("generator already in open state");
			if (outStr == null)
				throw new ArgumentNullException("outStr");

			// Do this first, since it might throw an exception
			long unixMs = DateTimeUtilities.DateTimeToUnixMs(modificationTime);

            byte[] encName = Strings.ToUtf8ByteArray(name);

            pkOut = new BcpgOutputStream(outStr, PacketTag.LiteralData,
				length + 2 + encName.Length + 4, oldFormat);

			WriteHeader(pkOut, format, encName, unixMs);

			return new WrappedGeneratorStream(this, pkOut);
        }

        /// <summary>
		/// <p>
		/// Open a literal data packet, returning a stream to store the data inside the packet,
		/// as an indefinite length stream. The stream is written out as a series of partial
		/// packets with a chunk size determined by the size of the passed in buffer.
		/// </p>
		/// <p>
		/// The stream created can be closed off by either calling Close()
		/// on the stream or Close() on the generator. Closing the returned
		/// stream does not close off the Stream parameter <c>outStr</c>.
		/// </p>
		/// <p>
		/// <b>Note</b>: if the buffer is not a power of 2 in length only the largest power of 2
		/// bytes worth of the buffer will be used.</p>
		/// </summary>
		/// <param name="outStr">The stream we want the packet in.</param>
		/// <param name="format">The format we are using.</param>
		/// <param name="name">The name of the 'file'.</param>
		/// <param name="modificationTime">The time of last modification we want stored.</param>
		/// <param name="buffer">The buffer to use for collecting data to put into chunks.</param>
        public Stream Open(
            Stream		outStr,
            char		format,
            string		name,
            DateTime	modificationTime,
            byte[]		buffer)
        {
			if (pkOut != null)
				throw new InvalidOperationException("generator already in open state");
			if (outStr == null)
				throw new ArgumentNullException("outStr");

			// Do this first, since it might throw an exception
			long unixMs = DateTimeUtilities.DateTimeToUnixMs(modificationTime);

            byte[] encName = Strings.ToUtf8ByteArray(name);

			pkOut = new BcpgOutputStream(outStr, PacketTag.LiteralData, buffer);

            WriteHeader(pkOut, format, encName, unixMs);

			return new WrappedGeneratorStream(this, pkOut);
		}

		/// <summary>
		/// <p>
		/// Open a literal data packet for the passed in <c>FileInfo</c> object, returning
		/// an output stream for saving the file contents.
		/// </p>
		/// <p>
		/// The stream created can be closed off by either calling Close()
		/// on the stream or Close() on the generator. Closing the returned
		/// stream does not close off the Stream parameter <c>outStr</c>.
		/// </p>
		/// </summary>
		/// <param name="outStr">The stream we want the packet in.</param>
		/// <param name="format">The format we are using.</param>
		/// <param name="file">The <c>FileInfo</c> object containg the packet details.</param>
		public Stream Open(
            Stream		outStr,
            char		format,
            FileInfo	file)
        {
			return Open(outStr, format, file.Name, file.Length, file.LastWriteTime);
        }

		/// <summary>
		/// Close the literal data packet - this is equivalent to calling Close()
		/// on the stream returned by the Open() method.
		/// </summary>
        public void Close()
        {
			if (pkOut != null)
			{
				pkOut.Finish();
				pkOut.Flush();
				pkOut = null;
			}
		}
	}
}
