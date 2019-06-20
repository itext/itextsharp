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
using System.Diagnostics;
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
    public abstract class PgpEncryptedData
    {
		internal class TruncatedStream
			: BaseInputStream
		{
			private const int LookAheadSize = 22;
			private const int LookAheadBufSize = 512;
			private const int LookAheadBufLimit = LookAheadBufSize - LookAheadSize;

			private readonly Stream inStr;
			private readonly byte[] lookAhead = new byte[LookAheadBufSize];
			private int bufStart, bufEnd;

			internal TruncatedStream(
				Stream inStr)
			{
				int numRead = Streams.ReadFully(inStr, lookAhead, 0, lookAhead.Length);

				if (numRead < LookAheadSize)
					throw new EndOfStreamException();

				this.inStr = inStr;
				this.bufStart = 0;
				this.bufEnd = numRead - LookAheadSize;
			}

			private int FillBuffer()
			{
				if (bufEnd < LookAheadBufLimit)
					return 0;

				Debug.Assert(bufStart == LookAheadBufLimit);
				Debug.Assert(bufEnd == LookAheadBufLimit);

				Array.Copy(lookAhead, LookAheadBufLimit, lookAhead, 0, LookAheadSize);
				bufEnd = Streams.ReadFully(inStr, lookAhead, LookAheadSize, LookAheadBufLimit);
				bufStart = 0;
				return bufEnd;
			}

			public override int ReadByte()
			{
				if (bufStart < bufEnd)
					return lookAhead[bufStart++];

				if (FillBuffer() < 1)
					return -1;

				return lookAhead[bufStart++];
			}

			public override int Read(byte[] buf, int off, int len)
			{
				int avail = bufEnd - bufStart;

				int pos = off;
				while (len > avail)
				{
					Array.Copy(lookAhead, bufStart, buf, pos, avail);

					bufStart += avail;
					pos += avail;
					len -= avail;

					if ((avail = FillBuffer()) < 1)
						return pos - off;
				}

				Array.Copy(lookAhead, bufStart, buf, pos, len);
				bufStart += len;

				return pos + len - off;;
			}

			internal byte[] GetLookAhead()
			{
				byte[] temp = new byte[LookAheadSize];
				Array.Copy(lookAhead, bufStart, temp, 0, LookAheadSize);
				return temp;
			}
		}

		internal InputStreamPacket	encData;
        internal Stream				encStream;
        internal TruncatedStream	truncStream;

		internal PgpEncryptedData(
            InputStreamPacket encData)
        {
            this.encData = encData;
        }

		/// <summary>Return the raw input stream for the data stream.</summary>
        public virtual Stream GetInputStream()
        {
            return encData.GetInputStream();
        }

		/// <summary>Return true if the message is integrity protected.</summary>
		/// <returns>True, if there is a modification detection code namespace associated
		/// with this stream.</returns>
        public bool IsIntegrityProtected()
        {
			return encData is SymmetricEncIntegrityPacket;
        }

		/// <summary>Note: This can only be called after the message has been read.</summary>
		/// <returns>True, if the message verifies, false otherwise</returns>
        public bool Verify()
        {
            if (!IsIntegrityProtected())
                throw new PgpException("data not integrity protected.");

			DigestStream dIn = (DigestStream) encStream;

			//
            // make sure we are at the end.
            //
            while (encStream.ReadByte() >= 0)
            {
				// do nothing
            }

			//
            // process the MDC packet
            //
			byte[] lookAhead = truncStream.GetLookAhead();

			IDigest hash = dIn.ReadDigest();
			hash.BlockUpdate(lookAhead, 0, 2);
			byte[] digest = DigestUtilities.DoFinal(hash);

			byte[] streamDigest = new byte[digest.Length];
			Array.Copy(lookAhead, 2, streamDigest, 0, streamDigest.Length);

			return Arrays.ConstantTimeAreEqual(digest, streamDigest);
        }
    }
}
