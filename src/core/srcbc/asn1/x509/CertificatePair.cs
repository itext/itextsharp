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

namespace Org.BouncyCastle.Asn1.X509
{
	/**
	* This class helps to support crossCerfificatePairs in a LDAP directory
	* according RFC 2587
	*
	* <pre>
	*     crossCertificatePairATTRIBUTE::={
	*       WITH SYNTAX   CertificatePair
	*       EQUALITY MATCHING RULE certificatePairExactMatch
	*       ID joint-iso-ccitt(2) ds(5) attributeType(4) crossCertificatePair(40)}
	* </pre>
	*
	* <blockquote> The forward elements of the crossCertificatePair attribute of a
	* CA's directory entry shall be used to store all, except self-issued
	* certificates issued to this CA. Optionally, the reverse elements of the
	* crossCertificatePair attribute, of a CA's directory entry may contain a
	* subset of certificates issued by this CA to other CAs. When both the forward
	* and the reverse elements are present in a single attribute value, issuer name
	* in one certificate shall match the subject name in the other and vice versa,
	* and the subject public key in one certificate shall be capable of verifying
	* the digital signature on the other certificate and vice versa.
	*
	* When a reverse element is present, the forward element value and the reverse
	* element value need not be stored in the same attribute value; in other words,
	* they can be stored in either a single attribute value or two attribute
	* values. </blockquote>
	*
	* <pre>
	*       CertificatePair ::= SEQUENCE {
	*         forward		[0]	Certificate OPTIONAL,
	*         reverse		[1]	Certificate OPTIONAL,
	*         -- at least one of the pair shall be present -- }
	* </pre>
	*/
	public class CertificatePair
		: Asn1Encodable
	{
		private X509CertificateStructure forward, reverse;

		public static CertificatePair GetInstance(
			object obj)
		{
			if (obj == null || obj is CertificatePair)
			{
				return (CertificatePair) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new CertificatePair((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type CertificatePair:
		* <p/>
		* <pre>
		*       CertificatePair ::= SEQUENCE {
		*         forward		[0]	Certificate OPTIONAL,
		*         reverse		[1]	Certificate OPTIONAL,
		*         -- at least one of the pair shall be present -- }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private CertificatePair(
			Asn1Sequence seq)
		{
			if (seq.Count != 1 && seq.Count != 2)
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");
			}

			foreach (object obj in seq)
			{
				Asn1TaggedObject o = Asn1TaggedObject.GetInstance(obj);
				if (o.TagNo == 0)
				{
					forward = X509CertificateStructure.GetInstance(o, true);
				}
				else if (o.TagNo == 1)
				{
					reverse = X509CertificateStructure.GetInstance(o, true);
				}
				else
				{
					throw new ArgumentException("Bad tag number: " + o.TagNo);
				}
			}
		}

		/**
		* Constructor from a given details.
		*
		* @param forward Certificates issued to this CA.
		* @param reverse Certificates issued by this CA to other CAs.
		*/
		public CertificatePair(
			X509CertificateStructure	forward,
			X509CertificateStructure	reverse)
		{
			this.forward = forward;
			this.reverse = reverse;
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*       CertificatePair ::= SEQUENCE {
		*         forward		[0]	Certificate OPTIONAL,
		*         reverse		[1]	Certificate OPTIONAL,
		*         -- at least one of the pair shall be present -- }
		* </pre>
		*
		* @return a DERObject
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector vec = new Asn1EncodableVector();

			if (forward != null)
			{
				vec.Add(new DerTaggedObject(0, forward));
			}

			if (reverse != null)
			{
				vec.Add(new DerTaggedObject(1, reverse));
			}

			return new DerSequence(vec);
		}

		/**
		* @return Returns the forward.
		*/
		public X509CertificateStructure Forward
		{
			get { return forward; }
		}

		/**
		* @return Returns the reverse.
		*/
		public X509CertificateStructure Reverse
		{
			get { return reverse; }
		}
	}
}
