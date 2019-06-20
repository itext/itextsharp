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

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class CertStatus
		: Asn1Encodable
	{
		private readonly Asn1OctetString certHash;
		private readonly DerInteger certReqId;
		private readonly PkiStatusInfo statusInfo;

		private CertStatus(Asn1Sequence seq)
		{
			certHash = Asn1OctetString.GetInstance(seq[0]);
			certReqId = DerInteger.GetInstance(seq[1]);

			if (seq.Count > 2)
			{
				statusInfo = PkiStatusInfo.GetInstance(seq[2]);
			}
		}

		public CertStatus(byte[] certHash, BigInteger certReqId)
		{
			this.certHash = new DerOctetString(certHash);
			this.certReqId = new DerInteger(certReqId);
		}

		public CertStatus(byte[] certHash, BigInteger certReqId, PkiStatusInfo statusInfo)
		{
			this.certHash = new DerOctetString(certHash);
			this.certReqId = new DerInteger(certReqId);
			this.statusInfo = statusInfo;
		}

		public static CertStatus GetInstance(object obj)
		{
			if (obj is CertStatus)
				return (CertStatus)obj;

			if (obj is Asn1Sequence)
				return new CertStatus((Asn1Sequence)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public virtual Asn1OctetString CertHash
		{
			get { return certHash; }
		}

		public virtual DerInteger CertReqID
		{
			get { return certReqId; }
		}

		public virtual PkiStatusInfo StatusInfo
		{
			get { return statusInfo; }
		}

		/**
		 * <pre>
		 * CertStatus ::= SEQUENCE {
		 *                   certHash    OCTET STRING,
		 *                   -- the hash of the certificate, using the same hash algorithm
		 *                   -- as is used to create and verify the certificate signature
		 *                   certReqId   INTEGER,
		 *                   -- to match this confirmation with the corresponding req/rep
		 *                   statusInfo  PKIStatusInfo OPTIONAL
		 * }
		 * </pre>
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(certHash, certReqId);
			v.AddOptional(statusInfo);
			return new DerSequence(v);
		}
	}
}
