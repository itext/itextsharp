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
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class ECDomainParameters
    {
        internal ECCurve     curve;
        internal byte[]      seed;
        internal ECPoint     g;
        internal BigInteger  n;
        internal BigInteger  h;

		public ECDomainParameters(
            ECCurve     curve,
            ECPoint     g,
            BigInteger  n)
			: this(curve, g, n, BigInteger.One)
        {
        }

        public ECDomainParameters(
            ECCurve     curve,
            ECPoint     g,
            BigInteger  n,
            BigInteger  h)
			: this(curve, g, n, h, null)
		{
        }

		public ECDomainParameters(
            ECCurve     curve,
            ECPoint     g,
            BigInteger  n,
            BigInteger  h,
            byte[]      seed)
        {
			if (curve == null)
				throw new ArgumentNullException("curve");
			if (g == null)
				throw new ArgumentNullException("g");
			if (n == null)
				throw new ArgumentNullException("n");
			if (h == null)
				throw new ArgumentNullException("h");

			this.curve = curve;
            this.g = g;
            this.n = n;
            this.h = h;
            this.seed = Arrays.Clone(seed);
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
            get { return h; }
        }

        public byte[] GetSeed()
        {
			return Arrays.Clone(seed);
        }

		public override bool Equals(
			object obj)
        {
			if (obj == this)
				return true;

			ECDomainParameters other = obj as ECDomainParameters;

			if (other == null)
				return false;

			return Equals(other);
        }

		protected bool Equals(
			ECDomainParameters other)
		{
			return curve.Equals(other.curve)
				&&	g.Equals(other.g)
				&&	n.Equals(other.n)
				&&	h.Equals(other.h)
				&&	Arrays.AreEqual(seed, other.seed);
		}

		public override int GetHashCode()
        {
            return curve.GetHashCode()
				^	g.GetHashCode()
				^	n.GetHashCode()
				^	h.GetHashCode()
				^	Arrays.GetHashCode(seed);
        }
    }

}
