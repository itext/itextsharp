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

using Org.BouncyCastle.Utilities.Date;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for a PGP public key.</remarks>
    public class PublicKeyPacket
        : ContainedPacket //, PublicKeyAlgorithmTag
    {
		private int version;
        private long time;
        private int validDays;
        private PublicKeyAlgorithmTag algorithm;
        private IBcpgKey key;

        internal PublicKeyPacket(
            BcpgInputStream bcpgIn)
        {
            version = bcpgIn.ReadByte();

            time = ((uint)bcpgIn.ReadByte() << 24) | ((uint)bcpgIn.ReadByte() << 16)
                | ((uint)bcpgIn.ReadByte() << 8) | (uint)bcpgIn.ReadByte();

            if (version <= 3)
            {
                validDays = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
            }

            algorithm = (PublicKeyAlgorithmTag) bcpgIn.ReadByte();

            switch ((PublicKeyAlgorithmTag) algorithm)
            {
                case PublicKeyAlgorithmTag.RsaEncrypt:
                case PublicKeyAlgorithmTag.RsaGeneral:
                case PublicKeyAlgorithmTag.RsaSign:
                    key = new RsaPublicBcpgKey(bcpgIn);
                    break;
                case PublicKeyAlgorithmTag.Dsa:
                    key = new DsaPublicBcpgKey(bcpgIn);
                    break;
                case PublicKeyAlgorithmTag.ElGamalEncrypt:
                case PublicKeyAlgorithmTag.ElGamalGeneral:
                    key = new ElGamalPublicBcpgKey(bcpgIn);
                    break;
                default:
                    throw new IOException("unknown PGP public key algorithm encountered");
            }
        }

		/// <summary>Construct a version 4 public key packet.</summary>
        public PublicKeyPacket(
            PublicKeyAlgorithmTag	algorithm,
            DateTime				time,
            IBcpgKey				key)
        {
			this.version = 4;
            this.time = DateTimeUtilities.DateTimeToUnixMs(time) / 1000L;
            this.algorithm = algorithm;
            this.key = key;
        }

        public int Version
        {
			get { return version; }
        }

        public PublicKeyAlgorithmTag Algorithm
        {
			get { return algorithm; }
        }

        public int ValidDays
        {
			get { return validDays; }
        }

		public DateTime GetTime()
        {
            return DateTimeUtilities.UnixMsToDateTime(time * 1000L);
        }

        public IBcpgKey Key
        {
			get { return key; }
        }

        public byte[] GetEncodedContents()
        {
            MemoryStream bOut = new MemoryStream();
            BcpgOutputStream pOut = new BcpgOutputStream(bOut);

            pOut.WriteByte((byte) version);
            pOut.WriteInt((int) time);

			if (version <= 3)
            {
                pOut.WriteShort((short) validDays);
            }

			pOut.WriteByte((byte) algorithm);

			pOut.WriteObject((BcpgObject)key);

			return bOut.ToArray();
        }

		public override void Encode(
			BcpgOutputStream bcpgOut)
		{
			bcpgOut.WritePacket(PacketTag.PublicKey, GetEncodedContents(), true);
		}
	}
}
