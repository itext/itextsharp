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

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class Challenge
		: Asn1Encodable
	{
		private readonly AlgorithmIdentifier owf;
		private readonly Asn1OctetString witness;
		private readonly Asn1OctetString challenge;

		private Challenge(Asn1Sequence seq)
		{
			int index = 0;

			if (seq.Count == 3)
			{
				owf = AlgorithmIdentifier.GetInstance(seq[index++]);
			}

			witness = Asn1OctetString.GetInstance(seq[index++]);
			challenge = Asn1OctetString.GetInstance(seq[index]);
		}

		public static Challenge GetInstance(object obj)
		{
			if (obj is Challenge)
				return (Challenge)obj;

			if (obj is Asn1Sequence)
				return new Challenge((Asn1Sequence)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public virtual AlgorithmIdentifier Owf
		{
			get { return owf; }
		}

		/**
		 * <pre>
		 * Challenge ::= SEQUENCE {
		 *                 owf                 AlgorithmIdentifier  OPTIONAL,
		 *
		 *                 -- MUST be present in the first Challenge; MAY be omitted in
		 *                 -- any subsequent Challenge in POPODecKeyChallContent (if
		 *                 -- omitted, then the owf used in the immediately preceding
		 *                 -- Challenge is to be used).
		 *
		 *                 witness             OCTET STRING,
		 *                 -- the result of applying the one-way function (owf) to a
		 *                 -- randomly-generated INTEGER, A.  [Note that a different
		 *                 -- INTEGER MUST be used for each Challenge.]
		 *                 challenge           OCTET STRING
		 *                 -- the encryption (under the public key for which the cert.
		 *                 -- request is being made) of Rand, where Rand is specified as
		 *                 --   Rand ::= SEQUENCE {
		 *                 --      int      INTEGER,
		 *                 --       - the randomly-generated INTEGER A (above)
		 *                 --      sender   GeneralName
		 *                 --       - the sender's name (as included in PKIHeader)
		 *                 --   }
		 *      }
		 * </pre>
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();
			v.AddOptional(owf);
			v.Add(witness);
			v.Add(challenge);
			return new DerSequence(v);
		}
	}
}
