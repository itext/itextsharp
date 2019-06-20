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

using Org.BouncyCastle.Asn1.Crmf;

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class CertifiedKeyPair
		: Asn1Encodable
	{
		private readonly CertOrEncCert certOrEncCert;
		private readonly EncryptedValue privateKey;
		private readonly PkiPublicationInfo publicationInfo;

		private CertifiedKeyPair(Asn1Sequence seq)
		{
			certOrEncCert = CertOrEncCert.GetInstance(seq[0]);

			if (seq.Count >= 2)
			{
				if (seq.Count == 2)
				{
					Asn1TaggedObject tagged = Asn1TaggedObject.GetInstance(seq[1]);
					if (tagged.TagNo == 0)
					{
						privateKey = EncryptedValue.GetInstance(tagged.GetObject());
					}
					else
					{
						publicationInfo = PkiPublicationInfo.GetInstance(tagged.GetObject());
					}
				}
				else
				{
					privateKey = EncryptedValue.GetInstance(Asn1TaggedObject.GetInstance(seq[1]));
					publicationInfo = PkiPublicationInfo.GetInstance(Asn1TaggedObject.GetInstance(seq[2]));
				}
			}
		}

		public static CertifiedKeyPair GetInstance(object obj)
		{
			if (obj is CertifiedKeyPair)
				return (CertifiedKeyPair)obj;

			if (obj is Asn1Sequence)
				return new CertifiedKeyPair((Asn1Sequence)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public CertifiedKeyPair(
			CertOrEncCert certOrEncCert)
			: this(certOrEncCert, null, null)
		{
		}

		public CertifiedKeyPair(
			CertOrEncCert		certOrEncCert,
			EncryptedValue		privateKey,
			PkiPublicationInfo	publicationInfo
		)
		{
			if (certOrEncCert == null)
				throw new ArgumentNullException("certOrEncCert");

			this.certOrEncCert = certOrEncCert;
			this.privateKey = privateKey;
			this.publicationInfo = publicationInfo;
		}

		public virtual CertOrEncCert CertOrEncCert
		{
			get { return certOrEncCert; }
		}

		public virtual EncryptedValue PrivateKey
		{
			get { return privateKey; }
		}

		public virtual PkiPublicationInfo PublicationInfo
		{
			get { return publicationInfo; }
		}

		/**
		 * <pre>
		 * CertifiedKeyPair ::= SEQUENCE {
		 *                                  certOrEncCert       CertOrEncCert,
		 *                                  privateKey      [0] EncryptedValue      OPTIONAL,
		 *                                  -- see [CRMF] for comment on encoding
		 *                                  publicationInfo [1] PKIPublicationInfo  OPTIONAL
		 *       }
		 * </pre>
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(certOrEncCert);

			if (privateKey != null)
			{
				v.Add(new DerTaggedObject(true, 0, privateKey));
			}

			if (publicationInfo != null)
			{
				v.Add(new DerTaggedObject(true, 1, publicationInfo));
			}

			return new DerSequence(v);
		}
	}
}
