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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.IsisMtt.Ocsp
{
	/**
	* ISIS-MTT PROFILE: The responder may include this extension in a response to
	* send the hash of the requested certificate to the responder. This hash is
	* cryptographically bound to the certificate and serves as evidence that the
	* certificate is known to the responder (i.e. it has been issued and is present
	* in the directory). Hence, this extension is a means to provide a positive
	* statement of availability as described in T8.[8]. As explained in T13.[1],
	* clients may rely on this information to be able to validate signatures after
	* the expiry of the corresponding certificate. Hence, clients MUST support this
	* extension. If a positive statement of availability is to be delivered, this
	* extension syntax and OID MUST be used.
	* <p/>
	* <p/>
	* <pre>
	*     CertHash ::= SEQUENCE {
	*       hashAlgorithm AlgorithmIdentifier,
	*       certificateHash OCTET STRING
	*     }
	* </pre>
	*/
	public class CertHash
		: Asn1Encodable
	{
		private readonly AlgorithmIdentifier	hashAlgorithm;
		private readonly byte[]					certificateHash;

		public static CertHash GetInstance(
			object obj)
		{
			if (obj == null || obj is CertHash)
			{
				return (CertHash) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new CertHash((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type CertHash:
		* <p/>
		* <pre>
		*     CertHash ::= SEQUENCE {
		*       hashAlgorithm AlgorithmIdentifier,
		*       certificateHash OCTET STRING
		*     }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private CertHash(
			Asn1Sequence seq)
		{
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			this.hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[0]);
			this.certificateHash = DerOctetString.GetInstance(seq[1]).GetOctets();
		}

		/**
		* Constructor from a given details.
		*
		* @param hashAlgorithm   The hash algorithm identifier.
		* @param certificateHash The hash of the whole DER encoding of the certificate.
		*/
		public CertHash(
			AlgorithmIdentifier	hashAlgorithm,
			byte[]				certificateHash)
		{
			if (hashAlgorithm == null)
				throw new ArgumentNullException("hashAlgorithm");
			if (certificateHash == null)
				throw new ArgumentNullException("certificateHash");

			this.hashAlgorithm = hashAlgorithm;
			this.certificateHash = (byte[]) certificateHash.Clone();
		}

		public AlgorithmIdentifier HashAlgorithm
		{
			get { return hashAlgorithm; }
		}

		public byte[] CertificateHash
		{
			get { return (byte[]) certificateHash.Clone(); }
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*     CertHash ::= SEQUENCE {
		*       hashAlgorithm AlgorithmIdentifier,
		*       certificateHash OCTET STRING
		*     }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			return new DerSequence(hashAlgorithm, new DerOctetString(certificateHash));
		}
	}
}
