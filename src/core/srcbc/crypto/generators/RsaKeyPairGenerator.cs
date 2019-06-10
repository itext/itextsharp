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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Generators
{
    /**
     * an RSA key pair generator.
     */
    public class RsaKeyPairGenerator
		: IAsymmetricCipherKeyPairGenerator
    {
		private static readonly BigInteger DefaultPublicExponent = BigInteger.ValueOf(0x10001);
		private const int DefaultTests = 12;

		private RsaKeyGenerationParameters param;

		public void Init(
            KeyGenerationParameters parameters)
        {
			if (parameters is RsaKeyGenerationParameters)
			{
				this.param = (RsaKeyGenerationParameters)parameters;
			}
			else
			{
				this.param = new RsaKeyGenerationParameters(
					DefaultPublicExponent, parameters.Random, parameters.Strength, DefaultTests);
			}
        }

		public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            BigInteger p, q, n, d, e, pSub1, qSub1, phi;

            //
            // p and q values should have a length of half the strength in bits
            //
			int strength = param.Strength;
            int pbitlength = (strength + 1) / 2;
            int qbitlength = (strength - pbitlength);
			int mindiffbits = strength / 3;

			e = param.PublicExponent;

			// TODO Consider generating safe primes for p, q (see DHParametersHelper.generateSafePrimes)
			// (then p-1 and q-1 will not consist of only small factors - see "Pollard's algorithm")

			//
            // Generate p, prime and (p-1) relatively prime to e
            //
            for (;;)
            {
				p = new BigInteger(pbitlength, 1, param.Random);

				if (p.Mod(e).Equals(BigInteger.One))
					continue;

				if (!p.IsProbablePrime(param.Certainty))
					continue;

				if (e.Gcd(p.Subtract(BigInteger.One)).Equals(BigInteger.One)) 
					break;
			}

            //
            // Generate a modulus of the required length
            //
            for (;;)
            {
                // Generate q, prime and (q-1) relatively prime to e,
                // and not equal to p
                //
                for (;;)
                {
					q = new BigInteger(qbitlength, 1, param.Random);

					if (q.Subtract(p).Abs().BitLength < mindiffbits)
						continue;

					if (q.Mod(e).Equals(BigInteger.One))
						continue;

					if (!q.IsProbablePrime(param.Certainty))
						continue;

					if (e.Gcd(q.Subtract(BigInteger.One)).Equals(BigInteger.One)) 
						break;
				}

                //
                // calculate the modulus
                //
                n = p.Multiply(q);

                if (n.BitLength == param.Strength)
					break;

                //
                // if we Get here our primes aren't big enough, make the largest
                // of the two p and try again
                //
                p = p.Max(q);
            }

			if (p.CompareTo(q) < 0)
			{
				phi = p;
				p = q;
				q = phi;
			}

            pSub1 = p.Subtract(BigInteger.One);
            qSub1 = q.Subtract(BigInteger.One);
            phi = pSub1.Multiply(qSub1);

            //
            // calculate the private exponent
            //
            d = e.ModInverse(phi);

            //
            // calculate the CRT factors
            //
            BigInteger dP, dQ, qInv;

            dP = d.Remainder(pSub1);
            dQ = d.Remainder(qSub1);
            qInv = q.ModInverse(p);

            return new AsymmetricCipherKeyPair(
                new RsaKeyParameters(false, n, e),
                new RsaPrivateCrtKeyParameters(n, e, d, p, q, dP, dQ, qInv));
        }
    }

}
