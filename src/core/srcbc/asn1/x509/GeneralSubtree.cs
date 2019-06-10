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

namespace Org.BouncyCastle.Asn1.X509
{
	/**
	 * Class for containing a restriction object subtrees in NameConstraints. See
	 * RFC 3280.
	 *
	 * <pre>
	 *
	 *       GeneralSubtree ::= SEQUENCE
	 *       {
	 *         baseName                    GeneralName,
	 *         minimum         [0]     BaseDistance DEFAULT 0,
	 *         maximum         [1]     BaseDistance OPTIONAL
	 *       }
	 * </pre>
	 *
	 * @see org.bouncycastle.asn1.x509.NameConstraints
	 *
	 */
	public class GeneralSubtree
		: Asn1Encodable
	{
		private readonly GeneralName	baseName;
		private readonly DerInteger		minimum;
		private readonly DerInteger		maximum;

		private GeneralSubtree(
			Asn1Sequence seq)
		{
			baseName = GeneralName.GetInstance(seq[0]);

			switch (seq.Count)
			{
				case 1:
					break;
				case 2:
				{
					Asn1TaggedObject o = Asn1TaggedObject.GetInstance(seq[1]);
					switch (o.TagNo)
					{
						case 0:
							minimum = DerInteger.GetInstance(o, false);
							break;
						case 1:
							maximum = DerInteger.GetInstance(o, false);
							break;
						default:
							throw new ArgumentException("Bad tag number: " + o.TagNo);
					}
					break;
				}
				case 3:
				{
					{
						Asn1TaggedObject oMin = Asn1TaggedObject.GetInstance(seq[1]);
						if (oMin.TagNo != 0)
							throw new ArgumentException("Bad tag number for 'minimum': " + oMin.TagNo);
						minimum = DerInteger.GetInstance(oMin, false);
					}

					{
						Asn1TaggedObject oMax = Asn1TaggedObject.GetInstance(seq[2]);
						if (oMax.TagNo != 1)
							throw new ArgumentException("Bad tag number for 'maximum': " + oMax.TagNo);
						maximum = DerInteger.GetInstance(oMax, false);
					}

					break;
				}
				default:
					throw new ArgumentException("Bad sequence size: " + seq.Count);
			}
		}

		/**
		 * Constructor from a given details.
		 *
		 * According RFC 3280, the minimum and maximum fields are not used with any
		 * name forms, thus minimum MUST be zero, and maximum MUST be absent.
		 * <p>
		 * If minimum is <code>null</code>, zero is assumed, if
		 * maximum is <code>null</code>, maximum is absent.</p>
		 *
		 * @param baseName
		 *            A restriction.
		 * @param minimum
		 *            Minimum
		 *
		 * @param maximum
		 *            Maximum
		 */
		public GeneralSubtree(
			GeneralName	baseName,
			BigInteger	minimum,
			BigInteger	maximum)
		{
			this.baseName = baseName;
			if (minimum != null)
			{
				this.minimum = new DerInteger(minimum);
			}
			if (maximum != null)
			{
				this.maximum = new DerInteger(maximum);
			}
		}

		public GeneralSubtree(
			GeneralName baseName)
			: this(baseName, null, null)
		{
		}

		public static GeneralSubtree GetInstance(
			Asn1TaggedObject	o,
			bool				isExplicit)
		{
			return new GeneralSubtree(Asn1Sequence.GetInstance(o, isExplicit));
		}

		public static GeneralSubtree GetInstance(
			object obj)
		{
			if (obj == null)
			{
				return null;
			}

			if (obj is GeneralSubtree)
			{
				return (GeneralSubtree) obj;
			}

			return new GeneralSubtree(Asn1Sequence.GetInstance(obj));
		}

		public GeneralName Base
		{
			get { return baseName; }
		}

		public BigInteger Minimum
		{
			get { return minimum == null ? BigInteger.Zero : minimum.Value; }
		}

		public BigInteger Maximum
		{
			get { return maximum == null ? null : maximum.Value; }
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 *
		 * Returns:
		 *
		 * <pre>
		 *       GeneralSubtree ::= SEQUENCE
		 *       {
		 *         baseName                    GeneralName,
		 *         minimum         [0]     BaseDistance DEFAULT 0,
		 *         maximum         [1]     BaseDistance OPTIONAL
		 *       }
		 * </pre>
		 *
		 * @return a DERObject
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(baseName);

			if (minimum != null && minimum.Value.SignValue != 0)
			{
				v.Add(new DerTaggedObject(false, 0, minimum));
			}

			if (maximum != null)
			{
				v.Add(new DerTaggedObject(false, 1, maximum));
			}

			return new DerSequence(v);
		}
	}
}
