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

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class KeyRecRepContent
		: Asn1Encodable
	{
		private readonly PkiStatusInfo status;
		private readonly CmpCertificate newSigCert;
		private readonly Asn1Sequence caCerts;
		private readonly Asn1Sequence keyPairHist;

		private KeyRecRepContent(Asn1Sequence seq)
		{
			status = PkiStatusInfo.GetInstance(seq[0]);

			for (int pos = 1; pos < seq.Count; ++pos)
			{
				Asn1TaggedObject tObj = Asn1TaggedObject.GetInstance(seq[pos]);

				switch (tObj.TagNo)
				{
					case 0:
						newSigCert = CmpCertificate.GetInstance(tObj.GetObject());
						break;
					case 1:
						caCerts = Asn1Sequence.GetInstance(tObj.GetObject());
						break;
					case 2:
						keyPairHist = Asn1Sequence.GetInstance(tObj.GetObject());
						break;
					default:
						throw new ArgumentException("unknown tag number: " + tObj.TagNo, "seq");
				}
			}
		}

		public static KeyRecRepContent GetInstance(object obj)
		{
			if (obj is KeyRecRepContent)
				return (KeyRecRepContent)obj;

			if (obj is Asn1Sequence)
				return new KeyRecRepContent((Asn1Sequence)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public virtual PkiStatusInfo Status
		{
			get { return status; }
		}

		public virtual CmpCertificate NewSigCert
		{
			get { return newSigCert; }
		}

		public virtual CmpCertificate[] GetCACerts()
		{
			if (caCerts == null)
				return null;

			CmpCertificate[] results = new CmpCertificate[caCerts.Count];
			for (int i = 0; i != results.Length; ++i)
			{
				results[i] = CmpCertificate.GetInstance(caCerts[i]);
			}
			return results;
		}

		public virtual CertifiedKeyPair[] GetKeyPairHist()
		{
			if (keyPairHist == null)
				return null;

			CertifiedKeyPair[] results = new CertifiedKeyPair[keyPairHist.Count];
			for (int i = 0; i != results.Length; ++i)
			{
				results[i] = CertifiedKeyPair.GetInstance(keyPairHist[i]);
			}
			return results;
		}

		/**
		 * <pre>
		 * KeyRecRepContent ::= SEQUENCE {
		 *                         status                  PKIStatusInfo,
		 *                         newSigCert          [0] CMPCertificate OPTIONAL,
		 *                         caCerts             [1] SEQUENCE SIZE (1..MAX) OF
		 *                                                           CMPCertificate OPTIONAL,
		 *                         keyPairHist         [2] SEQUENCE SIZE (1..MAX) OF
		 *                                                           CertifiedKeyPair OPTIONAL
		 *              }
		 * </pre> 
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(status);
			AddOptional(v, 0, newSigCert);
			AddOptional(v, 1, caCerts);
			AddOptional(v, 2, keyPairHist);
			return new DerSequence(v);
		}

		private void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
		{
			if (obj != null)
			{
				v.Add(new DerTaggedObject(true, tagNo, obj));
			}
		}
	}
}
