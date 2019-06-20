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
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X9
{
    /**
     * ASN.1 def for Elliptic-Curve Field ID structure. See
     * X9.62, for further details.
     */
    public class X9FieldID
        : Asn1Encodable
    {
        private readonly DerObjectIdentifier	id;
        private readonly Asn1Object parameters;

		/**
		 * Constructor for elliptic curves over prime fields
		 * <code>F<sub>2</sub></code>.
		 * @param primeP The prime <code>p</code> defining the prime field.
		 */
		public X9FieldID(
			BigInteger primeP)
		{
			this.id = X9ObjectIdentifiers.PrimeField;
			this.parameters = new DerInteger(primeP);
		}

		/**
		 * Constructor for elliptic curves over binary fields
		 * <code>F<sub>2<sup>m</sup></sub></code>.
		 * @param m  The exponent <code>m</code> of
		 * <code>F<sub>2<sup>m</sup></sub></code>.
		 * @param k1 The integer <code>k1</code> where <code>x<sup>m</sup> +
		 * x<sup>k3</sup> + x<sup>k2</sup> + x<sup>k1</sup> + 1</code>
		 * represents the reduction polynomial <code>f(z)</code>.
		 * @param k2 The integer <code>k2</code> where <code>x<sup>m</sup> +
		 * x<sup>k3</sup> + x<sup>k2</sup> + x<sup>k1</sup> + 1</code>
		 * represents the reduction polynomial <code>f(z)</code>.
		 * @param k3 The integer <code>k3</code> where <code>x<sup>m</sup> +
		 * x<sup>k3</sup> + x<sup>k2</sup> + x<sup>k1</sup> + 1</code>
		 * represents the reduction polynomial <code>f(z)</code>..
		 */
		public X9FieldID(
			int m,
			int k1,
			int k2,
			int k3)
		{
			this.id = X9ObjectIdentifiers.CharacteristicTwoField;

			Asn1EncodableVector fieldIdParams = new Asn1EncodableVector(new DerInteger(m));

			if (k2 == 0)
			{
				fieldIdParams.Add(
					X9ObjectIdentifiers.TPBasis,
					new DerInteger(k1));
			}
			else
			{
				fieldIdParams.Add(
					X9ObjectIdentifiers.PPBasis,
					new DerSequence(
						new DerInteger(k1),
						new DerInteger(k2),
						new DerInteger(k3)));
			}

			this.parameters = new DerSequence(fieldIdParams);
		}

		internal X9FieldID(
			Asn1Sequence seq)
		{
			this.id = (DerObjectIdentifier) seq[0];
			this.parameters = (Asn1Object) seq[1];
		}

		public DerObjectIdentifier Identifier
        {
            get { return id; }
        }

		public Asn1Object Parameters
        {
            get { return parameters; }
        }

		/**
         * Produce a Der encoding of the following structure.
         * <pre>
         *  FieldID ::= Sequence {
         *      fieldType       FIELD-ID.&amp;id({IOSet}),
         *      parameters      FIELD-ID.&amp;Type({IOSet}{&#64;fieldType})
         *  }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(id, parameters);
        }
    }
}
