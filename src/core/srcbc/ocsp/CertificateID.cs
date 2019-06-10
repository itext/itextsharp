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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Ocsp
{
	public class CertificateID
	{
		public const string HashSha1 = "1.3.14.3.2.26";

		private readonly CertID id;

		public CertificateID(
			CertID id)
		{
			if (id == null)
				throw new ArgumentNullException("id");

			this.id = id;
		}

		/**
		 * create from an issuer certificate and the serial number of the
		 * certificate it signed.
		 * @exception OcspException if any problems occur creating the id fields.
		 */
		public CertificateID(
			string			hashAlgorithm,
			X509Certificate	issuerCert,
			BigInteger		serialNumber)
		{
			AlgorithmIdentifier hashAlg = new AlgorithmIdentifier(
				new DerObjectIdentifier(hashAlgorithm), DerNull.Instance);

			this.id = CreateCertID(hashAlg, issuerCert, new DerInteger(serialNumber));
		}

		public string HashAlgOid
		{
			get { return id.HashAlgorithm.ObjectID.Id; }
		}

		public byte[] GetIssuerNameHash()
		{
			return id.IssuerNameHash.GetOctets();
		}

		public byte[] GetIssuerKeyHash()
		{
			return id.IssuerKeyHash.GetOctets();
		}

		/**
		 * return the serial number for the certificate associated
		 * with this request.
		 */
		public BigInteger SerialNumber
		{
			get { return id.SerialNumber.Value; }
		}

		public bool MatchesIssuer(
			X509Certificate	issuerCert)
		{
			return CreateCertID(id.HashAlgorithm, issuerCert, id.SerialNumber).Equals(id);
		}

		public CertID ToAsn1Object()
		{
			return id;
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			CertificateID other = obj as CertificateID;

			if (other == null)
				return false;

			return id.ToAsn1Object().Equals(other.id.ToAsn1Object());
		}

		public override int GetHashCode()
		{
			return id.ToAsn1Object().GetHashCode();
		}


		/**
		 * Create a new CertificateID for a new serial number derived from a previous one
		 * calculated for the same CA certificate.
		 *
		 * @param original the previously calculated CertificateID for the CA.
		 * @param newSerialNumber the serial number for the new certificate of interest.
		 *
		 * @return a new CertificateID for newSerialNumber
		 */
		public static CertificateID DeriveCertificateID(CertificateID original, BigInteger newSerialNumber)
		{
			return new CertificateID(new CertID(original.id.HashAlgorithm, original.id.IssuerNameHash,
				original.id.IssuerKeyHash, new DerInteger(newSerialNumber)));
		}

        private static CertID CreateCertID(
			AlgorithmIdentifier	hashAlg,
			X509Certificate		issuerCert,
			DerInteger			serialNumber)
		{
			try
			{
				String hashAlgorithm = hashAlg.ObjectID.Id;

				X509Name issuerName = PrincipalUtilities.GetSubjectX509Principal(issuerCert);
				byte[] issuerNameHash = DigestUtilities.CalculateDigest(
					hashAlgorithm, issuerName.GetEncoded());

				AsymmetricKeyParameter issuerKey = issuerCert.GetPublicKey();
				SubjectPublicKeyInfo info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(issuerKey);
				byte[] issuerKeyHash = DigestUtilities.CalculateDigest(
					hashAlgorithm, info.PublicKeyData.GetBytes());

				return new CertID(hashAlg, new DerOctetString(issuerNameHash),
					new DerOctetString(issuerKeyHash), serialNumber);
			}
			catch (Exception e)
			{
				throw new OcspException("problem creating ID: " + e, e);
			}
		}
	}
}
