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

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for a PGP secret key.</remarks>
    public class SecretKeyPacket
        : ContainedPacket //, PublicKeyAlgorithmTag
    {
		public const int UsageNone = 0x00;
		public const int UsageChecksum = 0xff;
		public const int UsageSha1 = 0xfe;

		private PublicKeyPacket pubKeyPacket;
        private readonly byte[] secKeyData;
		private int s2kUsage;
		private SymmetricKeyAlgorithmTag encAlgorithm;
        private S2k s2k;
        private byte[] iv;

		internal SecretKeyPacket(
            BcpgInputStream bcpgIn)
        {
			if (this is SecretSubkeyPacket)
			{
				pubKeyPacket = new PublicSubkeyPacket(bcpgIn);
			}
			else
			{
				pubKeyPacket = new PublicKeyPacket(bcpgIn);
			}

			s2kUsage = bcpgIn.ReadByte();

			if (s2kUsage == UsageChecksum || s2kUsage == UsageSha1)
            {
                encAlgorithm = (SymmetricKeyAlgorithmTag) bcpgIn.ReadByte();
                s2k = new S2k(bcpgIn);
            }
            else
            {
                encAlgorithm = (SymmetricKeyAlgorithmTag) s2kUsage;
			}

			if (!(s2k != null && s2k.Type == S2k.GnuDummyS2K && s2k.ProtectionMode == 0x01))
            {
				if (s2kUsage != 0)
				{
                    if (((int) encAlgorithm) < 7)
                    {
                        iv = new byte[8];
                    }
                    else
                    {
                        iv = new byte[16];
                    }
                    bcpgIn.ReadFully(iv);
                }
            }

			secKeyData = bcpgIn.ReadAll();
        }

		public SecretKeyPacket(
            PublicKeyPacket				pubKeyPacket,
            SymmetricKeyAlgorithmTag	encAlgorithm,
            S2k							s2k,
            byte[]						iv,
            byte[]						secKeyData)
        {
            this.pubKeyPacket = pubKeyPacket;
            this.encAlgorithm = encAlgorithm;

			if (encAlgorithm != SymmetricKeyAlgorithmTag.Null)
			{
				this.s2kUsage = UsageChecksum;
			}
			else
			{
				this.s2kUsage = UsageNone;
			}

			this.s2k = s2k;
			this.iv = Arrays.Clone(iv);
			this.secKeyData = secKeyData;
        }

		public SecretKeyPacket(
			PublicKeyPacket				pubKeyPacket,
			SymmetricKeyAlgorithmTag	encAlgorithm,
			int							s2kUsage,
			S2k							s2k,
			byte[]						iv,
			byte[]						secKeyData)
		{
			this.pubKeyPacket = pubKeyPacket;
			this.encAlgorithm = encAlgorithm;
			this.s2kUsage = s2kUsage;
			this.s2k = s2k;
			this.iv = Arrays.Clone(iv);
			this.secKeyData = secKeyData;
		}

		public SymmetricKeyAlgorithmTag EncAlgorithm
        {
			get { return encAlgorithm; }
        }

		public int S2kUsage
		{
			get { return s2kUsage; }
		}

		public byte[] GetIV()
        {
            return Arrays.Clone(iv);
        }

		public S2k S2k
        {
			get { return s2k; }
        }

		public PublicKeyPacket PublicKeyPacket
        {
			get { return pubKeyPacket; }
        }

		public byte[] GetSecretKeyData()
        {
            return secKeyData;
        }

		public byte[] GetEncodedContents()
        {
            MemoryStream bOut = new MemoryStream();
            BcpgOutputStream pOut = new BcpgOutputStream(bOut);

            pOut.Write(pubKeyPacket.GetEncodedContents());

			pOut.WriteByte((byte) s2kUsage);

			if (s2kUsage == UsageChecksum || s2kUsage == UsageSha1)
            {
                pOut.WriteByte((byte) encAlgorithm);
                pOut.WriteObject(s2k);
            }

			if (iv != null)
            {
                pOut.Write(iv);
            }

            if (secKeyData != null && secKeyData.Length > 0)
            {
                pOut.Write(secKeyData);
            }

            return bOut.ToArray();
        }

        public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(PacketTag.SecretKey, GetEncodedContents(), true);
        }
    }
}
