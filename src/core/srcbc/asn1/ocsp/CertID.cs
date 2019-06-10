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
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Ocsp
{
    public class CertID
        : Asn1Encodable
    {
        private readonly AlgorithmIdentifier    hashAlgorithm;
        private readonly Asn1OctetString        issuerNameHash;
        private readonly Asn1OctetString        issuerKeyHash;
        private readonly DerInteger             serialNumber;

		public static CertID GetInstance(
			Asn1TaggedObject	obj,
			bool				explicitly)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
		}

		public static CertID GetInstance(
			object obj)
		{
			if (obj == null || obj is CertID)
			{
				return (CertID)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new CertID((Asn1Sequence)obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public CertID(
            AlgorithmIdentifier hashAlgorithm,
            Asn1OctetString     issuerNameHash,
            Asn1OctetString     issuerKeyHash,
            DerInteger          serialNumber)
        {
            this.hashAlgorithm = hashAlgorithm;
            this.issuerNameHash = issuerNameHash;
            this.issuerKeyHash = issuerKeyHash;
            this.serialNumber = serialNumber;
        }

		private CertID(
            Asn1Sequence seq)
        {
			if (seq.Count != 4)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			this.hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[0]);
            this.issuerNameHash = Asn1OctetString.GetInstance(seq[1]);
            this.issuerKeyHash = Asn1OctetString.GetInstance(seq[2]);
            this.serialNumber = DerInteger.GetInstance(seq[3]);
        }

		public AlgorithmIdentifier HashAlgorithm
		{
			get { return hashAlgorithm; }
		}

		public Asn1OctetString IssuerNameHash
		{
			get { return issuerNameHash; }
		}

		public Asn1OctetString IssuerKeyHash
		{
			get { return issuerKeyHash; }
		}

		public DerInteger SerialNumber
		{
			get { return serialNumber; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * CertID          ::=     Sequence {
         *     hashAlgorithm       AlgorithmIdentifier,
         *     issuerNameHash      OCTET STRING, -- Hash of Issuer's DN
         *     issuerKeyHash       OCTET STRING, -- Hash of Issuers public key
         *     serialNumber        CertificateSerialNumber }
         * </pre>
         */
		public override Asn1Object ToAsn1Object()
		{
			return new DerSequence(hashAlgorithm, issuerNameHash, issuerKeyHash, serialNumber);
		}
	}
}
