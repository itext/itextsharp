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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Signers
{
	/**
	 * Gost R 34.10-94 Signature Algorithm
	 */
	public class Gost3410Signer
		: IDsa
	{
		private Gost3410KeyParameters key;
		private SecureRandom random;

		public string AlgorithmName
		{
			get { return "GOST3410"; }
		}

		public void Init(
			bool				forSigning,
			ICipherParameters	parameters)
		{
			if (forSigning)
			{
				if (parameters is ParametersWithRandom)
				{
					ParametersWithRandom rParam = (ParametersWithRandom)parameters;

					this.random = rParam.Random;
					parameters = rParam.Parameters;
				}
				else
				{
					this.random = new SecureRandom();
				}

				if (!(parameters is Gost3410PrivateKeyParameters))
					throw new InvalidKeyException("GOST3410 private key required for signing");

				this.key = (Gost3410PrivateKeyParameters) parameters;
			}
			else
			{
				if (!(parameters is Gost3410PublicKeyParameters))
					throw new InvalidKeyException("GOST3410 public key required for signing");

				this.key = (Gost3410PublicKeyParameters) parameters;
			}
		}

		/**
		 * generate a signature for the given message using the key we were
		 * initialised with. For conventional Gost3410 the message should be a Gost3411
		 * hash of the message of interest.
		 *
		 * @param message the message that will be verified later.
		 */
		public BigInteger[] GenerateSignature(
			byte[] message)
		{
			byte[] mRev = new byte[message.Length]; // conversion is little-endian
			for (int i = 0; i != mRev.Length; i++)
			{
				mRev[i] = message[mRev.Length - 1 - i];
			}

			BigInteger m = new BigInteger(1, mRev);
			Gost3410Parameters parameters = key.Parameters;
			BigInteger k;

			do
			{
				k = new BigInteger(parameters.Q.BitLength, random);
			}
			while (k.CompareTo(parameters.Q) >= 0);

			BigInteger r = parameters.A.ModPow(k, parameters.P).Mod(parameters.Q);

			BigInteger s = k.Multiply(m).
				Add(((Gost3410PrivateKeyParameters)key).X.Multiply(r)).
				Mod(parameters.Q);

			return new BigInteger[]{ r, s };
		}

		/**
		 * return true if the value r and s represent a Gost3410 signature for
		 * the passed in message for standard Gost3410 the message should be a
		 * Gost3411 hash of the real message to be verified.
		 */
		public bool VerifySignature(
			byte[]		message,
			BigInteger	r,
			BigInteger	s)
		{
			byte[] mRev = new byte[message.Length]; // conversion is little-endian
			for (int i = 0; i != mRev.Length; i++)
			{
				mRev[i] = message[mRev.Length - 1 - i];
			}

			BigInteger m = new BigInteger(1, mRev);
			Gost3410Parameters parameters = key.Parameters;

			if (r.SignValue < 0 || parameters.Q.CompareTo(r) <= 0)
			{
				return false;
			}

			if (s.SignValue < 0 || parameters.Q.CompareTo(s) <= 0)
			{
				return false;
			}

			BigInteger v = m.ModPow(parameters.Q.Subtract(BigInteger.Two), parameters.Q);

			BigInteger z1 = s.Multiply(v).Mod(parameters.Q);
			BigInteger z2 = (parameters.Q.Subtract(r)).Multiply(v).Mod(parameters.Q);

			z1 = parameters.A.ModPow(z1, parameters.P);
			z2 = ((Gost3410PublicKeyParameters)key).Y.ModPow(z2, parameters.P);

			BigInteger u = z1.Multiply(z2).Mod(parameters.P).Mod(parameters.Q);

			return u.Equals(r);
		}
	}
}
