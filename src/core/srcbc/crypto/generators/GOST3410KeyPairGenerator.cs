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

using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators
{
	/**
	 * a GOST3410 key pair generator.
	 * This generates GOST3410 keys in line with the method described
	 * in GOST R 34.10-94.
	 */
	public class Gost3410KeyPairGenerator
		: IAsymmetricCipherKeyPairGenerator
	{
		private Gost3410KeyGenerationParameters param;

		public void Init(
			KeyGenerationParameters parameters)
		{
			if (parameters is Gost3410KeyGenerationParameters)
			{
				this.param = (Gost3410KeyGenerationParameters) parameters;
			}
			else
			{
				Gost3410KeyGenerationParameters kgp = new Gost3410KeyGenerationParameters(
					parameters.Random,
					CryptoProObjectIdentifiers.GostR3410x94CryptoProA);

				if (parameters.Strength != kgp.Parameters.P.BitLength - 1)
				{
					// TODO Should we complain?
				}

				this.param = kgp;
			}
		}

		public AsymmetricCipherKeyPair GenerateKeyPair()
		{
			SecureRandom random = param.Random;
			Gost3410Parameters gost3410Params = param.Parameters;

			BigInteger q = gost3410Params.Q;
			BigInteger x;
			do
			{
				x = new BigInteger(256, random);
			}
			while (x.SignValue < 1 || x.CompareTo(q) >= 0);

			BigInteger p = gost3410Params.P;
			BigInteger a = gost3410Params.A;

			// calculate the public key.
			BigInteger y = a.ModPow(x, p);

			if (param.PublicKeyParamSet != null)
			{
				return new AsymmetricCipherKeyPair(
					new Gost3410PublicKeyParameters(y, param.PublicKeyParamSet),
					new Gost3410PrivateKeyParameters(x, param.PublicKeyParamSet));
			}

			return new AsymmetricCipherKeyPair(
				new Gost3410PublicKeyParameters(y, gost3410Params),
				new Gost3410PrivateKeyParameters(x, gost3410Params));
		}
	}
}
