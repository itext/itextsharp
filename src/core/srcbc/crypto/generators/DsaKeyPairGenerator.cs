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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators
{
    /**
     * a DSA key pair generator.
     *
     * This Generates DSA keys in line with the method described
	 * in <i>FIPS 186-3 B.1 FFC Key Pair Generation</i>.
     */
    public class DsaKeyPairGenerator
		: IAsymmetricCipherKeyPairGenerator
    {
        private DsaKeyGenerationParameters param;

		public void Init(
			KeyGenerationParameters parameters)
        {
			if (parameters == null)
				throw new ArgumentNullException("parameters");

			// Note: If we start accepting instances of KeyGenerationParameters,
			// must apply constraint checking on strength (see DsaParametersGenerator.Init)

			this.param = (DsaKeyGenerationParameters) parameters;
        }

		public AsymmetricCipherKeyPair GenerateKeyPair()
        {
			DsaParameters dsaParams = param.Parameters;

			BigInteger x = GeneratePrivateKey(dsaParams.Q, param.Random);
			BigInteger y = CalculatePublicKey(dsaParams.P, dsaParams.G, x);

			return new AsymmetricCipherKeyPair(
				new DsaPublicKeyParameters(y, dsaParams),
				new DsaPrivateKeyParameters(x, dsaParams));
        }

		private static BigInteger GeneratePrivateKey(BigInteger q, SecureRandom random)
		{
			// TODO Prefer this method? (change test cases that used fixed random)
			// B.1.1 Key Pair Generation Using Extra Random Bits
//	        BigInteger c = new BigInteger(q.BitLength + 64, random);
//	        return c.Mod(q.Subtract(BigInteger.One)).Add(BigInteger.One);

			// B.1.2 Key Pair Generation by Testing Candidates
			return BigIntegers.CreateRandomInRange(BigInteger.One, q.Subtract(BigInteger.One), random);
		}

		private static BigInteger CalculatePublicKey(BigInteger p, BigInteger g, BigInteger x)
		{
			return g.ModPow(x, p);
		}
	}
}
