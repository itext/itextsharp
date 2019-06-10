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
	public class RsassaPssParameters
		: Asn1Encodable
	{
		private AlgorithmIdentifier hashAlgorithm;
		private AlgorithmIdentifier maskGenAlgorithm;
		private DerInteger saltLength;
		private DerInteger trailerField;

		public readonly static AlgorithmIdentifier DefaultHashAlgorithm = new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1, DerNull.Instance);
		public readonly static AlgorithmIdentifier DefaultMaskGenFunction = new AlgorithmIdentifier(PkcsObjectIdentifiers.IdMgf1, DefaultHashAlgorithm);
		public readonly static DerInteger DefaultSaltLength = new DerInteger(20);
		public readonly static DerInteger DefaultTrailerField = new DerInteger(1);

		public static RsassaPssParameters GetInstance(
			object obj)
		{
			if (obj == null || obj is RsassaPssParameters)
			{
				return (RsassaPssParameters)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new RsassaPssParameters((Asn1Sequence)obj);
			}

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
		}

		/**
		 * The default version
		 */
		public RsassaPssParameters()
		{
			hashAlgorithm = DefaultHashAlgorithm;
			maskGenAlgorithm = DefaultMaskGenFunction;
			saltLength = DefaultSaltLength;
			trailerField = DefaultTrailerField;
		}

		public RsassaPssParameters(
			AlgorithmIdentifier hashAlgorithm,
			AlgorithmIdentifier maskGenAlgorithm,
			DerInteger saltLength,
			DerInteger trailerField)
		{
			this.hashAlgorithm = hashAlgorithm;
			this.maskGenAlgorithm = maskGenAlgorithm;
			this.saltLength = saltLength;
			this.trailerField = trailerField;
		}

		public RsassaPssParameters(
			Asn1Sequence seq)
		{
			hashAlgorithm = DefaultHashAlgorithm;
			maskGenAlgorithm = DefaultMaskGenFunction;
			saltLength = DefaultSaltLength;
			trailerField = DefaultTrailerField;

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
						saltLength = DerInteger.GetInstance(o, true);
						break;
					case 3:
						trailerField = DerInteger.GetInstance(o, true);
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

		public DerInteger SaltLength
		{
			get { return saltLength; }
		}

		public DerInteger TrailerField
		{
			get { return trailerField; }
		}

		/**
		 * <pre>
		 * RSASSA-PSS-params ::= SEQUENCE {
		 *   hashAlgorithm      [0] OAEP-PSSDigestAlgorithms  DEFAULT sha1,
		 *    maskGenAlgorithm   [1] PKCS1MGFAlgorithms  DEFAULT mgf1SHA1,
		 *    saltLength         [2] INTEGER  DEFAULT 20,
		 *    trailerField       [3] TrailerField  DEFAULT trailerFieldBC
		 *  }
		 *
		 * OAEP-PSSDigestAlgorithms    ALGORITHM-IDENTIFIER ::= {
		 *    { OID id-sha1 PARAMETERS NULL   }|
		 *    { OID id-sha256 PARAMETERS NULL }|
		 *    { OID id-sha384 PARAMETERS NULL }|
		 *    { OID id-sha512 PARAMETERS NULL },
		 *    ...  -- Allows for future expansion --
		 * }
		 *
		 * PKCS1MGFAlgorithms    ALGORITHM-IDENTIFIER ::= {
		 *   { OID id-mgf1 PARAMETERS OAEP-PSSDigestAlgorithms },
		 *    ...  -- Allows for future expansion --
		 * }
		 *
		 * TrailerField ::= INTEGER { trailerFieldBC(1) }
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

			if (!saltLength.Equals(DefaultSaltLength))
			{
				v.Add(new DerTaggedObject(true, 2, saltLength));
			}

			if (!trailerField.Equals(DefaultTrailerField))
			{
				v.Add(new DerTaggedObject(true, 3, trailerField));
			}

			return new DerSequence(v);
		}
	}
}
