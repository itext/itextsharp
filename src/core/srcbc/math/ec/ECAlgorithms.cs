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

namespace Org.BouncyCastle.Math.EC
{
	public class ECAlgorithms
	{
		public static ECPoint SumOfTwoMultiplies(ECPoint P, BigInteger a,
			ECPoint Q, BigInteger b)
		{
			ECCurve c = P.Curve;
			if (!c.Equals(Q.Curve))
				throw new ArgumentException("P and Q must be on same curve");

			// Point multiplication for Koblitz curves (using WTNAF) beats Shamir's trick
			if (c is F2mCurve)
			{
				F2mCurve f2mCurve = (F2mCurve) c;
				if (f2mCurve.IsKoblitz)
				{
					return P.Multiply(a).Add(Q.Multiply(b));
				}
			}

			return ImplShamirsTrick(P, a, Q, b);
		}

		/*
		* "Shamir's Trick", originally due to E. G. Straus
		* (Addition chains of vectors. American Mathematical Monthly,
		* 71(7):806-808, Aug./Sept. 1964)
		*  
		* Input: The points P, Q, scalar k = (km?, ... , k1, k0)
		* and scalar l = (lm?, ... , l1, l0).
		* Output: R = k * P + l * Q.
		* 1: Z <- P + Q
		* 2: R <- O
		* 3: for i from m-1 down to 0 do
		* 4:        R <- R + R        {point doubling}
		* 5:        if (ki = 1) and (li = 0) then R <- R + P end if
		* 6:        if (ki = 0) and (li = 1) then R <- R + Q end if
		* 7:        if (ki = 1) and (li = 1) then R <- R + Z end if
		* 8: end for
		* 9: return R
		*/
		public static ECPoint ShamirsTrick(
			ECPoint		P,
			BigInteger	k,
			ECPoint		Q,
			BigInteger	l)
		{
			if (!P.Curve.Equals(Q.Curve))
				throw new ArgumentException("P and Q must be on same curve");

			return ImplShamirsTrick(P, k, Q, l);
		}

		private static ECPoint ImplShamirsTrick(ECPoint P, BigInteger k,
			ECPoint Q, BigInteger l)
		{
			int m = System.Math.Max(k.BitLength, l.BitLength);
			ECPoint Z = P.Add(Q);
			ECPoint R = P.Curve.Infinity;

			for (int i = m - 1; i >= 0; --i)
			{
				R = R.Twice();

				if (k.TestBit(i))
				{
					if (l.TestBit(i))
					{
						R = R.Add(Z);
					}
					else
					{
						R = R.Add(P);
					}
				}
				else
				{
					if (l.TestBit(i))
					{
						R = R.Add(Q);
					}
				}
			}

			return R;
		}
	}
}
