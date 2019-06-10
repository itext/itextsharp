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

using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Pkcs
{
	public class RsaesOaepParameters
		: Asn1Encodable
	{
		private AlgorithmIdentifier hashAlgorithm;
		private AlgorithmIdentifier maskGenAlgorithm;
		private AlgorithmIdentifier pSourceAlgorithm;

		public readonly static AlgorithmIdentifier DefaultHashAlgorithm = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
		public readonly static AlgorithmIdentifier DefaultMaskGenFunction = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, DefaultHashAlgorithm);
		public readonly static AlgorithmIdentifier DefaultPSourceAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdPSpecified, new DerOctetString(new byte[0]));

		public static RsaesOaepParameters GetInstance(
			object obj)
		{
			if (obj is RsaesOaepParameters)
			{
				return (RsaesOaepParameters)obj;
			}
			else if (obj is Asn1Sequence)
			{
				return new RsaesOaepParameters((Asn1Sequence)obj);
			}

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
		}

		/**
		 * The default version
		 */
		public RsaesOaepParameters()
		{
			hashAlgorithm = DefaultHashAlgorithm;
			maskGenAlgorithm = DefaultMaskGenFunction;
			pSourceAlgorithm = DefaultPSourceAlgorithm;
		}

		public RsaesOaepParameters(
			AlgorithmIdentifier hashAlgorithm,
			AlgorithmIdentifier maskGenAlgorithm,
			AlgorithmIdentifier pSourceAlgorithm)
		{
			this.hashAlgorithm = hashAlgorithm;
			this.maskGenAlgorithm = maskGenAlgorithm;
			this.pSourceAlgorithm = pSourceAlgorithm;
		}

		public RsaesOaepParameters(
			Asn1Sequence seq)
		{
			hashAlgorithm = DefaultHashAlgorithm;
			maskGenAlgorithm = DefaultMaskGenFunction;
			pSourceAlgorithm = DefaultPSourceAlgorithm;

			for (int i = 0; i != seq.Count; i++)
			{
				Asn1TaggedObject o = (Asn1TaggedObject)seq[i];

				switch (o.TagNo)
				{
					case 0:
						hashAlgorithm = AlgorithmIdentifier.GetInstance(o, true);
						break;
					case 1:
						maskGenAlgorithm = AlgorithmIdentifier.GetInstance(o, true);
						break;
					case 2:
						pSourceAlgorithm = AlgorithmIdentifier.GetInstance(o, true);
						break;
					default:
						throw new ArgumentException("unknown tag");
				}
			}
		}

		public AlgorithmIdentifier HashAlgorithm
		{
			get { return hashAlgorithm; }
		}

		public AlgorithmIdentifier MaskGenAlgorithm
		{
			get { return maskGenAlgorithm; }
		}

		public AlgorithmIdentifier PSourceAlgorithm
		{
			get { return pSourceAlgorithm; }
		}

		/**
		 * <pre>
		 *  RSAES-OAEP-params ::= SEQUENCE {
		 *     hashAlgorithm      [0] OAEP-PSSDigestAlgorithms     DEFAULT sha1,
		 *     maskGenAlgorithm   [1] PKCS1MGFAlgorithms  DEFAULT mgf1SHA1,
		 *     pSourceAlgorithm   [2] PKCS1PSourceAlgorithms  DEFAULT pSpecifiedEmpty
		 *   }
		 *
		 *   OAEP-PSSDigestAlgorithms    ALGORITHM-IDENTIFIER ::= {
		 *     { OID id-sha1 PARAMETERS NULL   }|
		 *     { OID id-sha256 PARAMETERS NULL }|
		 *     { OID id-sha384 PARAMETERS NULL }|
		 *     { OID id-sha512 PARAMETERS NULL },
		 *     ...  -- Allows for future expansion --
		 *   }
		 *   PKCS1MGFAlgorithms    ALGORITHM-IDENTIFIER ::= {
		 *     { OID id-mgf1 PARAMETERS OAEP-PSSDigestAlgorithms },
		 *    ...  -- Allows for future expansion --
		 *   }
		 *   PKCS1PSourceAlgorithms    ALGORITHM-IDENTIFIER ::= {
		 *     { OID id-pSpecified PARAMETERS OCTET STRING },
		 *     ...  -- Allows for future expansion --
		 *  }
		 * </pre>
		 * @return the asn1 primitive representing the parameters.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			if (!hashAlgorithm.Equals(DefaultHashAlgorithm))
			{
				v.Add(new DerTaggedObject(true, 0, hashAlgorithm));
			}

			if (!maskGenAlgorithm.Equals(DefaultMaskGenFunction))
			{
				v.Add(new DerTaggedObject(true, 1, maskGenAlgorithm));
			}

			if (!pSourceAlgorithm.Equals(DefaultPSourceAlgorithm))
			{
				v.Add(new DerTaggedObject(true, 2, pSourceAlgorithm));
			}

			return new DerSequence(v);
		}
	}
}
