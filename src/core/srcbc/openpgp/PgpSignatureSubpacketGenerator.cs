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
	/// <remarks>Generator for signature subpackets.</remarks>
    public class PgpSignatureSubpacketGenerator
    {
        private IList list = Platform.CreateArrayList();

		public void SetRevocable(
            bool	isCritical,
            bool	isRevocable)
        {
            list.Add(new Revocable(isCritical, isRevocable));
        }

		public void SetExportable(
            bool	isCritical,
            bool	isExportable)
        {
            list.Add(new Exportable(isCritical, isExportable));
        }

		/// <summary>
		/// Add a TrustSignature packet to the signature. The values for depth and trust are largely
		/// installation dependent but there are some guidelines in RFC 4880 - 5.2.3.13.
		/// </summary>
		/// <param name="isCritical">true if the packet is critical.</param>
		/// <param name="depth">depth level.</param>
		/// <param name="trustAmount">trust amount.</param>
		public void SetTrust(
            bool	isCritical,
            int		depth,
            int		trustAmount)
        {
            list.Add(new TrustSignature(isCritical, depth, trustAmount));
        }

		/// <summary>
		/// Set the number of seconds a key is valid for after the time of its creation.
		/// A value of zero means the key never expires.
		/// </summary>
		/// <param name="isCritical">True, if should be treated as critical, false otherwise.</param>
		/// <param name="seconds">The number of seconds the key is valid, or zero if no expiry.</param>
        public void SetKeyExpirationTime(
            bool	isCritical,
            long	seconds)
        {
            list.Add(new KeyExpirationTime(isCritical, seconds));
        }

		/// <summary>
		/// Set the number of seconds a signature is valid for after the time of its creation.
		/// A value of zero means the signature never expires.
		/// </summary>
		/// <param name="isCritical">True, if should be treated as critical, false otherwise.</param>
		/// <param name="seconds">The number of seconds the signature is valid, or zero if no expiry.</param>
        public void SetSignatureExpirationTime(
            bool	isCritical,
            long	seconds)
        {
            list.Add(new SignatureExpirationTime(isCritical, seconds));
        }

		/// <summary>
		/// Set the creation time for the signature.
		/// <p>
		/// Note: this overrides the generation of a creation time when the signature
		/// is generated.</p>
		/// </summary>
		public void SetSignatureCreationTime(
			bool		isCritical,
			DateTime	date)
		{
			list.Add(new SignatureCreationTime(isCritical, date));
		}

		public void SetPreferredHashAlgorithms(
            bool	isCritical,
            int[]	algorithms)
        {
            list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredHashAlgorithms, isCritical, algorithms));
        }

		public void SetPreferredSymmetricAlgorithms(
            bool	isCritical,
            int[]	algorithms)
        {
            list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredSymmetricAlgorithms, isCritical, algorithms));
        }

		public void SetPreferredCompressionAlgorithms(
            bool	isCritical,
            int[]	algorithms)
        {
            list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredCompressionAlgorithms, isCritical, algorithms));
        }

		public void SetKeyFlags(
            bool	isCritical,
            int		flags)
        {
            list.Add(new KeyFlags(isCritical, flags));
        }

		public void SetSignerUserId(
            bool	isCritical,
            string	userId)
        {
            if (userId == null)
                throw new ArgumentNullException("userId");

			list.Add(new SignerUserId(isCritical, userId));
        }

		public void SetEmbeddedSignature(
			bool			isCritical,
			PgpSignature	pgpSignature)
		{
			byte[] sig = pgpSignature.GetEncoded();
			byte[] data;

			// TODO Should be >= ?
			if (sig.Length - 1 > 256)
			{
				data = new byte[sig.Length - 3];
			}
			else
			{
				data = new byte[sig.Length - 2];
			}

			Array.Copy(sig, sig.Length - data.Length, data, 0, data.Length);

			list.Add(new EmbeddedSignature(isCritical, data));
		}

		public void SetPrimaryUserId(
            bool	isCritical,
            bool	isPrimaryUserId)
        {
            list.Add(new PrimaryUserId(isCritical, isPrimaryUserId));
        }

		public void SetNotationData(
			bool	isCritical,
			bool	isHumanReadable,
			string	notationName,
			string	notationValue)
		{
			list.Add(new NotationData(isCritical, isHumanReadable, notationName, notationValue));
		}

		/// <summary>
		/// Sets revocation reason sub packet
		/// </summary>	    
		public void SetRevocationReason(bool isCritical, RevocationReasonTag reason,
			string description)
		{
			list.Add(new RevocationReason(isCritical, reason, description));
		}

		/// <summary>
		/// Sets revocation key sub packet
		/// </summary>	
		public void SetRevocationKey(bool isCritical, PublicKeyAlgorithmTag keyAlgorithm, byte[] fingerprint)
		{
			list.Add(new RevocationKey(isCritical, RevocationKeyTag.ClassDefault, keyAlgorithm, fingerprint));
		}

		/// <summary>
		/// Sets issuer key sub packet
		/// </summary>	
		public void SetIssuerKeyID(bool isCritical, long keyID)
		{
			list.Add(new IssuerKeyId(isCritical, keyID));
		}    

		public PgpSignatureSubpacketVector Generate()
        {
            SignatureSubpacket[] a = new SignatureSubpacket[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                a[i] = (SignatureSubpacket)list[i];
            }
            return new PgpSignatureSubpacketVector(a);
        }
    }
}
