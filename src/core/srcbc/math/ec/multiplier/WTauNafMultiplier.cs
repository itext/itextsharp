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

using Org.BouncyCastle.Math.EC.Abc;

namespace Org.BouncyCastle.Math.EC.Multiplier
{
	/**
	* Class implementing the WTNAF (Window
	* <code>&#964;</code>-adic Non-Adjacent Form) algorithm.
	*/
	internal class WTauNafMultiplier
		: ECMultiplier
	{
		/**
		* Multiplies a {@link org.bouncycastle.math.ec.F2mPoint F2mPoint}
		* by <code>k</code> using the reduced <code>&#964;</code>-adic NAF (RTNAF)
		* method.
		* @param p The F2mPoint to multiply.
		* @param k The integer by which to multiply <code>k</code>.
		* @return <code>p</code> multiplied by <code>k</code>.
		*/
		public ECPoint Multiply(ECPoint point, BigInteger k, PreCompInfo preCompInfo)
		{
			if (!(point is F2mPoint))
				throw new ArgumentException("Only F2mPoint can be used in WTauNafMultiplier");

			F2mPoint p = (F2mPoint)point;

			F2mCurve curve = (F2mCurve) p.Curve;
			int m = curve.M;
			sbyte a = (sbyte) curve.A.ToBigInteger().IntValue;
			sbyte mu = curve.GetMu();
			BigInteger[] s = curve.GetSi();

			ZTauElement rho = Tnaf.PartModReduction(k, m, a, s, mu, (sbyte)10);

			return MultiplyWTnaf(p, rho, preCompInfo, a, mu);
		}

		/**
		* Multiplies a {@link org.bouncycastle.math.ec.F2mPoint F2mPoint}
		* by an element <code>&#955;</code> of <code><b>Z</b>[&#964;]</code> using
		* the <code>&#964;</code>-adic NAF (TNAF) method.
		* @param p The F2mPoint to multiply.
		* @param lambda The element <code>&#955;</code> of
		* <code><b>Z</b>[&#964;]</code> of which to compute the
		* <code>[&#964;]</code>-adic NAF.
		* @return <code>p</code> multiplied by <code>&#955;</code>.
		*/
		private F2mPoint MultiplyWTnaf(F2mPoint p, ZTauElement lambda,
			PreCompInfo preCompInfo, sbyte a, sbyte mu)
		{
			ZTauElement[] alpha;
			if (a == 0)
			{
				alpha = Tnaf.Alpha0;
			}
			else
			{
				// a == 1
				alpha = Tnaf.Alpha1;
			}

			BigInteger tw = Tnaf.GetTw(mu, Tnaf.Width);

			sbyte[]u = Tnaf.TauAdicWNaf(mu, lambda, Tnaf.Width,
				BigInteger.ValueOf(Tnaf.Pow2Width), tw, alpha);

			return MultiplyFromWTnaf(p, u, preCompInfo);
		}
	    
		/**
		* Multiplies a {@link org.bouncycastle.math.ec.F2mPoint F2mPoint}
		* by an element <code>&#955;</code> of <code><b>Z</b>[&#964;]</code>
		* using the window <code>&#964;</code>-adic NAF (TNAF) method, given the
		* WTNAF of <code>&#955;</code>.
		* @param p The F2mPoint to multiply.
		* @param u The the WTNAF of <code>&#955;</code>..
		* @return <code>&#955; * p</code>
		*/
		private static F2mPoint MultiplyFromWTnaf(F2mPoint p, sbyte[] u,
			PreCompInfo preCompInfo)
		{
			F2mCurve curve = (F2mCurve)p.Curve;
			sbyte a = (sbyte) curve.A.ToBigInteger().IntValue;

			F2mPoint[] pu;
			if ((preCompInfo == null) || !(preCompInfo is WTauNafPreCompInfo))
			{
				pu = Tnaf.GetPreComp(p, a);
				p.SetPreCompInfo(new WTauNafPreCompInfo(pu));
			}
			else
			{
				pu = ((WTauNafPreCompInfo)preCompInfo).GetPreComp();
			}

			// q = infinity
			F2mPoint q = (F2mPoint) p.Curve.Infinity;
			for (int i = u.Length - 1; i >= 0; i--)
			{
				q = Tnaf.Tau(q);
				if (u[i] != 0)
				{
					if (u[i] > 0)
					{
						q = q.AddSimple(pu[u[i]]);
					}
					else
					{
						// u[i] < 0
						q = q.SubtractSimple(pu[-u[i]]);
					}
				}
			}

			return q;
		}
	}
}
