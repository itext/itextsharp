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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Reader for PGP objects.</remarks>
    public class BcpgInputStream
        : BaseInputStream
    {
        private Stream m_in;
        private bool next = false;
        private int nextB;

        internal static BcpgInputStream Wrap(
			Stream inStr)
        {
            if (inStr is BcpgInputStream)
            {
                return (BcpgInputStream) inStr;
            }

            return new BcpgInputStream(inStr);
        }

        private BcpgInputStream(
			Stream inputStream)
        {
            this.m_in = inputStream;
        }

        public override int ReadByte()
        {
            if (next)
            {
                next = false;
                return nextB;
            }

            return m_in.ReadByte();
        }

        public override int Read(
			byte[]	buffer,
			int		offset,
			int		count)
        {
			// Strangely, when count == 0, we should still attempt to read a byte
//			if (count == 0)
//				return 0;

			if (!next)
				return m_in.Read(buffer, offset, count);

			// We have next byte waiting, so return it

			if (nextB < 0)
				return 0; // EndOfStream

			if (buffer == null)
				throw new ArgumentNullException("buffer");

			buffer[offset] = (byte) nextB;
			next = false;

			return 1;
        }

		public byte[] ReadAll()
        {
			return Streams.ReadAll(this);
		}

		public void ReadFully(
            byte[]	buffer,
            int		off,
            int		len)
        {
			if (Streams.ReadFully(this, buffer, off, len) < len)
				throw new EndOfStreamException();
        }

		public void ReadFully(
            byte[] buffer)
        {
            ReadFully(buffer, 0, buffer.Length);
        }

		/// <summary>Returns the next packet tag in the stream.</summary>
        public PacketTag NextPacketTag()
        {
            if (!next)
            {
                try
                {
                    nextB = m_in.ReadByte();
                }
                catch (EndOfStreamException)
                {
                    nextB = -1;
                }

                next = true;
            }

            if (nextB >= 0)
            {
                if ((nextB & 0x40) != 0)    // new
                {
                    return (PacketTag)(nextB & 0x3f);
                }
                else    // old
                {
                    return (PacketTag)((nextB & 0x3f) >> 2);
                }
            }

            return (PacketTag) nextB;
        }

        public Packet ReadPacket()
        {
            int hdr = this.ReadByte();

            if (hdr < 0)
            {
                return null;
            }

            if ((hdr & 0x80) == 0)
            {
                throw new IOException("invalid header encountered");
            }

            bool newPacket = (hdr & 0x40) != 0;
            PacketTag tag = 0;
            int bodyLen = 0;
            bool partial = false;

            if (newPacket)
            {
                tag = (PacketTag)(hdr & 0x3f);

                int l = this.ReadByte();

                if (l < 192)
                {
                    bodyLen = l;
                }
                else if (l <= 223)
                {
                    int b = m_in.ReadByte();
                    bodyLen = ((l - 192) << 8) + (b) + 192;
                }
                else if (l == 255)
                {
                    bodyLen = (m_in.ReadByte() << 24) | (m_in.ReadByte() << 16)
                        |  (m_in.ReadByte() << 8)  | m_in.ReadByte();
                }
                else
                {
                    partial = true;
                    bodyLen = 1 << (l & 0x1f);
                }
            }
            else
            {
                int lengthType = hdr & 0x3;

                tag = (PacketTag)((hdr & 0x3f) >> 2);

                switch (lengthType)
                {
                    case 0:
                        bodyLen = this.ReadByte();
                        break;
                    case 1:
                        bodyLen = (this.ReadByte() << 8) | this.ReadByte();
                        break;
                    case 2:
                        bodyLen = (this.ReadByte() << 24) | (this.ReadByte() << 16)
                            | (this.ReadByte() << 8) | this.ReadByte();
                        break;
                    case 3:
                        partial = true;
                        break;
                    default:
                        throw new IOException("unknown length type encountered");
                }
            }

            BcpgInputStream objStream;
            if (bodyLen == 0 && partial)
            {
                objStream = this;
            }
            else
            {
                PartialInputStream pis = new PartialInputStream(this, partial, bodyLen);
                objStream = new BcpgInputStream(pis);
            }

            switch (tag)
            {
                case PacketTag.Reserved:
                    return new InputStreamPacket(objStream);
                case PacketTag.PublicKeyEncryptedSession:
                    return new PublicKeyEncSessionPacket(objStream);
                case PacketTag.Signature:
                    return new SignaturePacket(objStream);
                case PacketTag.SymmetricKeyEncryptedSessionKey:
                    return new SymmetricKeyEncSessionPacket(objStream);
                case PacketTag.OnePassSignature:
                    return new OnePassSignaturePacket(objStream);
                case PacketTag.SecretKey:
                    return new SecretKeyPacket(objStream);
                case PacketTag.PublicKey:
                    return new PublicKeyPacket(objStream);
                case PacketTag.SecretSubkey:
                    return new SecretSubkeyPacket(objStream);
                case PacketTag.CompressedData:
                    return new CompressedDataPacket(objStream);
                case PacketTag.SymmetricKeyEncrypted:
                    return new SymmetricEncDataPacket(objStream);
                case PacketTag.Marker:
                    return new MarkerPacket(objStream);
                case PacketTag.LiteralData:
                    return new LiteralDataPacket(objStream);
                case PacketTag.Trust:
                    return new TrustPacket(objStream);
                case PacketTag.UserId:
                    return new UserIdPacket(objStream);
                case PacketTag.UserAttribute:
                    return new UserAttributePacket(objStream);
                case PacketTag.PublicSubkey:
                    return new PublicSubkeyPacket(objStream);
                case PacketTag.SymmetricEncryptedIntegrityProtected:
                    return new SymmetricEncIntegrityPacket(objStream);
                case PacketTag.ModificationDetectionCode:
                    return new ModDetectionCodePacket(objStream);
                case PacketTag.Experimental1:
                case PacketTag.Experimental2:
                case PacketTag.Experimental3:
                case PacketTag.Experimental4:
                    return new ExperimentalPacket(tag, objStream);
                default:
                    throw new IOException("unknown packet type encountered: " + tag);
            }
        }

		public override void Close()
		{
			m_in.Close();
			base.Close();
		}

		/// <summary>
		/// A stream that overlays our input stream, allowing the user to only read a segment of it.
		/// NB: dataLength will be negative if the segment length is in the upper range above 2**31.
		/// </summary>
		private class PartialInputStream
            : BaseInputStream
        {
            private BcpgInputStream m_in;
            private bool partial;
            private int dataLength;

            internal PartialInputStream(
                BcpgInputStream	bcpgIn,
                bool			partial,
                int				dataLength)
            {
                this.m_in = bcpgIn;
                this.partial = partial;
                this.dataLength = dataLength;
            }

			public override int ReadByte()
			{
				do
				{
					if (dataLength != 0)
					{
						int ch = m_in.ReadByte();
						if (ch < 0)
						{
							throw new EndOfStreamException("Premature end of stream in PartialInputStream");
						}
						dataLength--;
						return ch;
					}
				}
				while (partial && ReadPartialDataLength() >= 0);

				return -1;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				do
				{
					if (dataLength != 0)
					{
						int readLen = (dataLength > count || dataLength < 0) ? count : dataLength;
						int len = m_in.Read(buffer, offset, readLen);
						if (len < 1)
						{
							throw new EndOfStreamException("Premature end of stream in PartialInputStream");
						}
						dataLength -= len;
						return len;
					}
				}
				while (partial && ReadPartialDataLength() >= 0);

				return 0;
			}

            private int ReadPartialDataLength()
            {
                int l = m_in.ReadByte();

				if (l < 0)
                {
                    return -1;
                }

				partial = false;

				if (l < 192)
                {
                    dataLength = l;
                }
                else if (l <= 223)
                {
                    dataLength = ((l - 192) << 8) + (m_in.ReadByte()) + 192;
                }
                else if (l == 255)
                {
                    dataLength = (m_in.ReadByte() << 24) | (m_in.ReadByte() << 16)
                        |  (m_in.ReadByte() << 8)  | m_in.ReadByte();
                }
                else
                {
                    partial = true;
                    dataLength = 1 << (l & 0x1f);
                }

                return 0;
            }
        }
    }
}
