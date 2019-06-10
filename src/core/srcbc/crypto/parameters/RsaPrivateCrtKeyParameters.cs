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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class RsaPrivateCrtKeyParameters
		: RsaKeyParameters
    {
        private readonly BigInteger e, p, q, dP, dQ, qInv;

		public RsaPrivateCrtKeyParameters(
            BigInteger	modulus,
            BigInteger	publicExponent,
            BigInteger	privateExponent,
            BigInteger	p,
            BigInteger	q,
            BigInteger	dP,
            BigInteger	dQ,
            BigInteger	qInv)
			: base(true, modulus, privateExponent)
        {
			ValidateValue(publicExponent, "publicExponent", "exponent");
			ValidateValue(p, "p", "P value");
			ValidateValue(q, "q", "Q value");
			ValidateValue(dP, "dP", "DP value");
			ValidateValue(dQ, "dQ", "DQ value");
			ValidateValue(qInv, "qInv", "InverseQ value");

			this.e = publicExponent;
            this.p = p;
            this.q = q;
            this.dP = dP;
            this.dQ = dQ;
            this.qInv = qInv;
        }

		public BigInteger PublicExponent
        {
            get { return e; }
		}

		public BigInteger P
		{
			get { return p; }
		}

		public BigInteger Q
		{
			get { return q; }
		}

		public BigInteger DP
		{
			get { return dP; }
		}

		public BigInteger DQ
		{
			get { return dQ; }
		}

		public BigInteger QInv
		{
			get { return qInv; }
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			RsaPrivateCrtKeyParameters kp = obj as RsaPrivateCrtKeyParameters;

			if (kp == null)
				return false;

			return kp.DP.Equals(dP)
				&& kp.DQ.Equals(dQ)
				&& kp.Exponent.Equals(this.Exponent)
				&& kp.Modulus.Equals(this.Modulus)
				&& kp.P.Equals(p)
				&& kp.Q.Equals(q)
				&& kp.PublicExponent.Equals(e)
				&& kp.QInv.Equals(qInv);
		}

		public override int GetHashCode()
		{
			return DP.GetHashCode() ^ DQ.GetHashCode() ^ Exponent.GetHashCode() ^ Modulus.GetHashCode()
				^ P.GetHashCode() ^ Q.GetHashCode() ^ PublicExponent.GetHashCode() ^ QInv.GetHashCode();
		}

		private static void ValidateValue(BigInteger x, string name, string desc)
		{
			if (x == null)
				throw new ArgumentNullException(name);
			if (x.SignValue <= 0)
				throw new ArgumentException("Not a valid RSA " + desc, name);
		}
	}
}
