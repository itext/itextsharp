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
using System.Collections;

using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Container for a list of signature subpackets.</remarks>
    public class PgpSignatureSubpacketVector
    {
        private readonly SignatureSubpacket[] packets;

		internal PgpSignatureSubpacketVector(
            SignatureSubpacket[] packets)
        {
            this.packets = packets;
        }

		public SignatureSubpacket GetSubpacket(
            SignatureSubpacketTag type)
        {
            for (int i = 0; i != packets.Length; i++)
            {
                if (packets[i].SubpacketType == type)
                {
                    return packets[i];
                }
            }

			return null;
        }

		/**
		 * Return true if a particular subpacket type exists.
		 *
		 * @param type type to look for.
		 * @return true if present, false otherwise.
		 */
		public bool HasSubpacket(
			SignatureSubpacketTag type)
		{
			return GetSubpacket(type) != null;
		}

		/**
		 * Return all signature subpackets of the passed in type.
		 * @param type subpacket type code
		 * @return an array of zero or more matching subpackets.
		 */
		public SignatureSubpacket[] GetSubpackets(
			SignatureSubpacketTag type)
		{
            int count = 0;
            for (int i = 0; i < packets.Length; ++i)
            {
                if (packets[i].SubpacketType == type)
                {
                    ++count;
                }
            }

            SignatureSubpacket[] result = new SignatureSubpacket[count];

            int pos = 0;
            for (int i = 0; i < packets.Length; ++i)
            {
                if (packets[i].SubpacketType == type)
                {
                    result[pos++] = packets[i];
                }
            }

            return result;
        }

        public NotationData[] GetNotationDataOccurences()
		{
			SignatureSubpacket[] notations = GetSubpackets(SignatureSubpacketTag.NotationData);
			NotationData[] vals = new NotationData[notations.Length];

			for (int i = 0; i < notations.Length; i++)
			{
				vals[i] = (NotationData) notations[i];
			}

			return vals;
		}

		public long GetIssuerKeyId()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.IssuerKeyId);

            return p == null ? 0 : ((IssuerKeyId) p).KeyId;
        }

		public bool HasSignatureCreationTime()
		{
			return GetSubpacket(SignatureSubpacketTag.CreationTime) != null;
		}

		public DateTime GetSignatureCreationTime()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.CreationTime);

            if (p == null)
            {
                throw new PgpException("SignatureCreationTime not available");
            }

            return ((SignatureCreationTime)p).GetTime();
        }

		/// <summary>
		/// Return the number of seconds a signature is valid for after its creation date.
		/// A value of zero means the signature never expires.
		/// </summary>
		/// <returns>Seconds a signature is valid for.</returns>
        public long GetSignatureExpirationTime()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.ExpireTime);

			return p == null ? 0 : ((SignatureExpirationTime) p).Time;
        }

		/// <summary>
		/// Return the number of seconds a key is valid for after its creation date.
		/// A value of zero means the key never expires.
		/// </summary>
		/// <returns>Seconds a signature is valid for.</returns>
        public long GetKeyExpirationTime()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.KeyExpireTime);

			return p == null ? 0 : ((KeyExpirationTime) p).Time;
        }

		public int[] GetPreferredHashAlgorithms()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.PreferredHashAlgorithms);

			return p == null ? null : ((PreferredAlgorithms) p).GetPreferences();
        }

		public int[] GetPreferredSymmetricAlgorithms()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.PreferredSymmetricAlgorithms);

            return p == null ? null : ((PreferredAlgorithms) p).GetPreferences();
        }

		public int[] GetPreferredCompressionAlgorithms()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.PreferredCompressionAlgorithms);

            return p == null ? null : ((PreferredAlgorithms) p).GetPreferences();
        }

		public int GetKeyFlags()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.KeyFlags);

            return p == null ? 0 : ((KeyFlags) p).Flags;
        }

		public string GetSignerUserId()
        {
            SignatureSubpacket p = GetSubpacket(SignatureSubpacketTag.SignerUserId);

			return p == null ? null : ((SignerUserId) p).GetId();
        }

		public bool IsPrimaryUserId()
		{
			PrimaryUserId primaryId = (PrimaryUserId)
				this.GetSubpacket(SignatureSubpacketTag.PrimaryUserId);

			if (primaryId != null)
			{
				return primaryId.IsPrimaryUserId();
			}

			return false;
		}

		public SignatureSubpacketTag[] GetCriticalTags()
        {
            int count = 0;
            for (int i = 0; i != packets.Length; i++)
            {
                if (packets[i].IsCritical())
                {
                    count++;
                }
            }

			SignatureSubpacketTag[] list = new SignatureSubpacketTag[count];

			count = 0;

			for (int i = 0; i != packets.Length; i++)
            {
                if (packets[i].IsCritical())
                {
                    list[count++] = packets[i].SubpacketType;
                }
            }

			return list;
        }

		[Obsolete("Use 'Count' property instead")]
		public int Size
		{
			get { return packets.Length; }
		}

		/// <summary>Return the number of packets this vector contains.</summary>
		public int Count
		{
			get { return packets.Length; }
		}

		internal SignatureSubpacket[] ToSubpacketArray()
        {
            return packets;
        }
    }
}
