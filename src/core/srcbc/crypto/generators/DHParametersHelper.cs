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
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators
{
	internal class DHParametersHelper
	{
        private static readonly BigInteger Six = BigInteger.ValueOf(6);

        private static readonly int[][] primeLists = BigInteger.primeLists;
        private static readonly int[] primeProducts = BigInteger.primeProducts;
		private static readonly BigInteger[] BigPrimeProducts = ConstructBigPrimeProducts(primeProducts);

        private static BigInteger[] ConstructBigPrimeProducts(int[] primeProducts)
        {
            BigInteger[] bpp = new BigInteger[primeProducts.Length];
            for (int i = 0; i < bpp.Length; ++i)
            {
                bpp[i] = BigInteger.ValueOf(primeProducts[i]);
            }
            return bpp;
        }

        /*
         * Finds a pair of prime BigInteger's {p, q: p = 2q + 1}
         * 
         * (see: Handbook of Applied Cryptography 4.86)
         */
        internal static BigInteger[] GenerateSafePrimes(int size, int certainty, SecureRandom random)
		{
			BigInteger p, q;
			int qLength = size - 1;

			if (size <= 32)
			{
				for (;;)
				{
					q = new BigInteger(qLength, 2, random);

					p = q.ShiftLeft(1).Add(BigInteger.One);

					if (p.IsProbablePrime(certainty)
						&& (certainty <= 2 || q.IsProbablePrime(certainty)))
							break;
				}
			}
			else
			{
				// Note: Modified from Java version for speed
				for (;;)
				{
					q = new BigInteger(qLength, 0, random);

				retry:
					for (int i = 0; i < primeLists.Length; ++i)
					{
						int test = q.Remainder(BigPrimeProducts[i]).IntValue;

                        if (i == 0)
						{
							int rem3 = test % 3;
							if (rem3 != 2)
							{
								int diff = 2 * rem3 + 2;
								q = q.Add(BigInteger.ValueOf(diff));
								test = (test + diff) % primeProducts[i];
							}
						}

						int[] primeList = primeLists[i];
						for (int j = 0; j < primeList.Length; ++j)
						{
							int prime = primeList[j];
							int qRem = test % prime;
							if (qRem == 0 || qRem == (prime >> 1))
							{
								q = q.Add(Six);
								goto retry;
							}
						}
					}


					if (q.BitLength != qLength)
						continue;

					if (!q.RabinMillerTest(2, random))
						continue;

					p = q.ShiftLeft(1).Add(BigInteger.One);

					if (p.RabinMillerTest(certainty, random)
						&& (certainty <= 2 || q.RabinMillerTest(certainty - 2, random)))
						break;
				}
			}

			return new BigInteger[] { p, q };
		}

		/*
		 * Select a high order element of the multiplicative group Zp*
		 * 
		 * p and q must be s.t. p = 2*q + 1, where p and q are prime (see generateSafePrimes)
		 */
		internal static BigInteger SelectGenerator(BigInteger p, BigInteger q, SecureRandom random)
		{
			BigInteger pMinusTwo = p.Subtract(BigInteger.Two);
			BigInteger g;

			/*
			 * (see: Handbook of Applied Cryptography 4.80)
			 */
//			do
//			{
//				g = BigIntegers.CreateRandomInRange(BigInteger.Two, pMinusTwo, random);
//			}
//			while (g.ModPow(BigInteger.Two, p).Equals(BigInteger.One)
//				|| g.ModPow(q, p).Equals(BigInteger.One));

			/*
	         * RFC 2631 2.2.1.2 (and see: Handbook of Applied Cryptography 4.81)
	         */
			do
			{
				BigInteger h = BigIntegers.CreateRandomInRange(BigInteger.Two, pMinusTwo, random);

				g = h.ModPow(BigInteger.Two, p);
			}
			while (g.Equals(BigInteger.One));

			return g;
		}
	}
}
