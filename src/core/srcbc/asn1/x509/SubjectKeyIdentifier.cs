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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The SubjectKeyIdentifier object.
     * <pre>
     * SubjectKeyIdentifier::= OCTET STRING
     * </pre>
     */
    public class SubjectKeyIdentifier
        : Asn1Encodable
    {
        private readonly byte[] keyIdentifier;

		public static SubjectKeyIdentifier GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1OctetString.GetInstance(obj, explicitly));
        }

		public static SubjectKeyIdentifier GetInstance(
            object obj)
        {
            if (obj is SubjectKeyIdentifier)
            {
                return (SubjectKeyIdentifier) obj;
            }

			if (obj is SubjectPublicKeyInfo)
            {
                return new SubjectKeyIdentifier((SubjectPublicKeyInfo) obj);
            }

			if (obj is Asn1OctetString)
            {
                return new SubjectKeyIdentifier((Asn1OctetString) obj);
            }

			if (obj is X509Extension)
			{
				return GetInstance(X509Extension.ConvertValueToObject((X509Extension) obj));
			}

			throw new ArgumentException("Invalid SubjectKeyIdentifier: " + obj.GetType().Name);
        }

		public SubjectKeyIdentifier(
            byte[] keyID)
        {
			if (keyID == null)
				throw new ArgumentNullException("keyID");

			this.keyIdentifier = keyID;
        }

		public SubjectKeyIdentifier(
            Asn1OctetString keyID)
        {
            this.keyIdentifier = keyID.GetOctets();
        }

		/**
		 * Calculates the keyIdentifier using a SHA1 hash over the BIT STRING
		 * from SubjectPublicKeyInfo as defined in RFC3280.
		 *
		 * @param spki the subject public key info.
		 */
		public SubjectKeyIdentifier(
			SubjectPublicKeyInfo spki)
		{
			this.keyIdentifier = GetDigest(spki);
		}

		public byte[] GetKeyIdentifier()
		{
			return keyIdentifier;
		}

		public override Asn1Object ToAsn1Object()
		{
			return new DerOctetString(keyIdentifier);
		}

		/**
		 * Return a RFC 3280 type 1 key identifier. As in:
		 * <pre>
		 * (1) The keyIdentifier is composed of the 160-bit SHA-1 hash of the
		 * value of the BIT STRING subjectPublicKey (excluding the tag,
		 * length, and number of unused bits).
		 * </pre>
		 * @param keyInfo the key info object containing the subjectPublicKey field.
		 * @return the key identifier.
		 */
		public static SubjectKeyIdentifier CreateSha1KeyIdentifier(
			SubjectPublicKeyInfo keyInfo)
		{
			return new SubjectKeyIdentifier(keyInfo);
		}

		/**
		 * Return a RFC 3280 type 2 key identifier. As in:
		 * <pre>
		 * (2) The keyIdentifier is composed of a four bit type field with
		 * the value 0100 followed by the least significant 60 bits of the
		 * SHA-1 hash of the value of the BIT STRING subjectPublicKey.
		 * </pre>
		 * @param keyInfo the key info object containing the subjectPublicKey field.
		 * @return the key identifier.
		 */
		public static SubjectKeyIdentifier CreateTruncatedSha1KeyIdentifier(
			SubjectPublicKeyInfo keyInfo)
		{
			byte[] dig = GetDigest(keyInfo);
			byte[] id = new byte[8];

			Array.Copy(dig, dig.Length - 8, id, 0, id.Length);

			id[0] &= 0x0f;
			id[0] |= 0x40;

			return new SubjectKeyIdentifier(id);
		}

		private static byte[] GetDigest(
			SubjectPublicKeyInfo spki)
		{
            IDigest digest = new Sha1Digest();
            byte[] resBuf = new byte[digest.GetDigestSize()];

			byte[] bytes = spki.PublicKeyData.GetBytes();
            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(resBuf, 0);
            return resBuf;
		}
	}
}
