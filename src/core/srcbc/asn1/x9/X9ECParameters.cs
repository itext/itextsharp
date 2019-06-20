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
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Asn1.X9
{
    /**
     * ASN.1 def for Elliptic-Curve ECParameters structure. See
     * X9.62, for further details.
     */
    public class X9ECParameters
        : Asn1Encodable
    {
        private X9FieldID	fieldID;
        private ECCurve		curve;
        private ECPoint		g;
        private BigInteger	n;
        private BigInteger	h;
        private byte[]		seed;

		public X9ECParameters(
            Asn1Sequence seq)
        {
            if (!(seq[0] is DerInteger)
               || !((DerInteger) seq[0]).Value.Equals(BigInteger.One))
            {
                throw new ArgumentException("bad version in X9ECParameters");
            }

			X9Curve x9c = null;
            if (seq[2] is X9Curve)
            {
                x9c = (X9Curve) seq[2];
            }
            else
            {
                x9c = new X9Curve(
					new X9FieldID(
						(Asn1Sequence) seq[1]),
						(Asn1Sequence) seq[2]);
            }

			this.curve = x9c.Curve;

			if (seq[3] is X9ECPoint)
            {
                this.g = ((X9ECPoint) seq[3]).Point;
            }
            else
            {
                this.g = new X9ECPoint(curve, (Asn1OctetString) seq[3]).Point;
            }

			this.n = ((DerInteger) seq[4]).Value;
            this.seed = x9c.GetSeed();

			if (seq.Count == 6)
            {
                this.h = ((DerInteger) seq[5]).Value;
            }
        }

		public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n)
            : this(curve, g, n, BigInteger.One, null)
        {
        }

		public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n,
            BigInteger	h)
            : this(curve, g, n, h, null)
        {
        }

		public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n,
            BigInteger	h,
            byte[]		seed)
        {
            this.curve = curve;
            this.g = g;
            this.n = n;
            this.h = h;
            this.seed = seed;

			if (curve is FpCurve)
			{
				this.fieldID = new X9FieldID(((FpCurve) curve).Q);
			}
			else if (curve is F2mCurve)
			{
				F2mCurve curveF2m = (F2mCurve) curve;
				this.fieldID = new X9FieldID(curveF2m.M, curveF2m.K1,
					curveF2m.K2, curveF2m.K3);
			}
        }

		public ECCurve Curve
        {
			get { return curve; }
        }

		public ECPoint G
        {
            get { return g; }
        }

		public BigInteger N
        {
            get { return n; }
        }

		public BigInteger H
        {
            get
			{
				if (h == null)
				{
					// TODO - this should be calculated, it will cause issues with custom curves.
					return BigInteger.One;
				}

				return h;
			}
        }

		public byte[] GetSeed()
        {
            return seed;
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *  ECParameters ::= Sequence {
         *      version         Integer { ecpVer1(1) } (ecpVer1),
         *      fieldID         FieldID {{FieldTypes}},
         *      curve           X9Curve,
         *      base            X9ECPoint,
         *      order           Integer,
         *      cofactor        Integer OPTIONAL
         *  }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				new DerInteger(1),
				fieldID,
				new X9Curve(curve, seed),
				new X9ECPoint(g),
				new DerInteger(n));

			if (h != null)
            {
                v.Add(new DerInteger(h));
            }

			return new DerSequence(v);
        }
    }
}
