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
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DHParameters
		: ICipherParameters
    {
		private const int DefaultMinimumLength = 160;

		private readonly BigInteger p, g, q, j;
		private readonly int m, l;
		private readonly DHValidationParameters validation;

		private static int GetDefaultMParam(
			int lParam)
		{
			if (lParam == 0)
				return DefaultMinimumLength;

			return System.Math.Min(lParam, DefaultMinimumLength);
		}

		public DHParameters(
			BigInteger	p,
			BigInteger	g)
			: this(p, g, null, 0)
		{
		}

		public DHParameters(
			BigInteger	p,
			BigInteger	g,
			BigInteger	q)
			: this(p, g, q, 0)
		{
		}

		public DHParameters(
			BigInteger	p,
			BigInteger	g,
			BigInteger	q,
			int			l)
			: this(p, g, q, GetDefaultMParam(l), l, null, null)
		{
		}

		public DHParameters(
			BigInteger  p,
			BigInteger  g,
			BigInteger  q,
			int         m,
			int         l)
			: this(p, g, q, m, l, null, null)
		{
		}

		public DHParameters(
			BigInteger				p,
			BigInteger				g,
			BigInteger				q,
			BigInteger				j,
			DHValidationParameters	validation)
			: this(p, g, q,  DefaultMinimumLength, 0, j, validation)
		{
		}

		public DHParameters(
			BigInteger				p,
			BigInteger				g,
			BigInteger				q,
			int						m,
			int						l,
			BigInteger				j,
			DHValidationParameters	validation)
		{
			if (p == null)
				throw new ArgumentNullException("p");
			if (g == null)
				throw new ArgumentNullException("g");
			if (!p.TestBit(0))
				throw new ArgumentException("field must be an odd prime", "p");
			if (g.CompareTo(BigInteger.Two) < 0
				|| g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
				throw new ArgumentException("generator must in the range [2, p - 2]", "g");
			if (q != null && q.BitLength >= p.BitLength)
				throw new ArgumentException("q too big to be a factor of (p-1)", "q");
			if (m >= p.BitLength)
				throw new ArgumentException("m value must be < bitlength of p", "m");
			if (l != 0)
			{ 
	            if (l >= p.BitLength)
                	throw new ArgumentException("when l value specified, it must be less than bitlength(p)", "l");
				if (l < m)
					throw new ArgumentException("when l value specified, it may not be less than m value", "l");
			}
			if (j != null && j.CompareTo(BigInteger.Two) < 0)
				throw new ArgumentException("subgroup factor must be >= 2", "j");

			// TODO If q, j both provided, validate p = jq + 1 ?

			this.p = p;
			this.g = g;
			this.q = q;
			this.m = m;
			this.l = l;
			this.j = j;
			this.validation = validation;
        }

		public BigInteger P
        {
            get { return p; }
        }

		public BigInteger G
        {
            get { return g; }
        }

		public BigInteger Q
        {
            get { return q; }
        }

		public BigInteger J
        {
            get { return j; }
        }

		/// <summary>The minimum bitlength of the private value.</summary>
		public int M
		{
			get { return m; }
		}

		/// <summary>The bitlength of the private value.</summary>
		public int L
		{
			get { return l; }
		}

		public DHValidationParameters ValidationParameters
        {
			get { return validation; }
        }

		public override bool Equals(
			object obj)
        {
			if (obj == this)
				return true;

			DHParameters other = obj as DHParameters;

			if (other == null)
				return false;

			return Equals(other);
		}

		protected bool Equals(
			DHParameters other)
		{
			return p.Equals(other.p)
				&& g.Equals(other.g)
				&& Platform.Equals(q, other.q);
		}

		public override int GetHashCode()
        {
			int hc = p.GetHashCode() ^ g.GetHashCode();

			if (q != null)
			{
				hc ^= q.GetHashCode();
			}

			return hc;
        }
    }
}
