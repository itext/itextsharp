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
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Signers
{
	/**
	 * The Digital Signature Algorithm - as described in "Handbook of Applied
	 * Cryptography", pages 452 - 453.
	 */
	public class DsaSigner
		: IDsa
	{
		private DsaKeyParameters key;
		private SecureRandom random;

		public string AlgorithmName
		{
			get { return "DSA"; }
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

				if (!(parameters is DsaPrivateKeyParameters))
					throw new InvalidKeyException("DSA private key required for signing");

				this.key = (DsaPrivateKeyParameters) parameters;
			}
			else
			{
				if (!(parameters is DsaPublicKeyParameters))
					throw new InvalidKeyException("DSA public key required for verification");

				this.key = (DsaPublicKeyParameters) parameters;
			}
		}

		/**
		 * Generate a signature for the given message using the key we were
		 * initialised with. For conventional DSA the message should be a SHA-1
		 * hash of the message of interest.
		 *
		 * @param message the message that will be verified later.
		 */
		public BigInteger[] GenerateSignature(
			byte[] message)
		{
			DsaParameters parameters = key.Parameters;
			BigInteger q = parameters.Q;
			BigInteger m = calculateE(q, message);
			BigInteger k;

			do
			{
				k = new BigInteger(q.BitLength, random);
			}
			while (k.CompareTo(q) >= 0);

			BigInteger r = parameters.G.ModPow(k, parameters.P).Mod(q);

			k = k.ModInverse(q).Multiply(
				m.Add(((DsaPrivateKeyParameters)key).X.Multiply(r)));

			BigInteger s = k.Mod(q);

			return new BigInteger[]{ r, s };
		}

		/**
		 * return true if the value r and s represent a DSA signature for
		 * the passed in message for standard DSA the message should be a
		 * SHA-1 hash of the real message to be verified.
		 */
		public bool VerifySignature(
			byte[]		message,
			BigInteger	r,
			BigInteger	s)
		{
			DsaParameters parameters = key.Parameters;
			BigInteger q = parameters.Q;
			BigInteger m = calculateE(q, message);

			if (r.SignValue <= 0 || q.CompareTo(r) <= 0)
			{
				return false;
			}

			if (s.SignValue <= 0 || q.CompareTo(s) <= 0)
			{
				return false;
			}

			BigInteger w = s.ModInverse(q);

			BigInteger u1 = m.Multiply(w).Mod(q);
			BigInteger u2 = r.Multiply(w).Mod(q);

			BigInteger p = parameters.P;
			u1 = parameters.G.ModPow(u1, p);
			u2 = ((DsaPublicKeyParameters)key).Y.ModPow(u2, p);

			BigInteger v = u1.Multiply(u2).Mod(p).Mod(q);

			return v.Equals(r);
		}

		private BigInteger calculateE(
			BigInteger	n,
			byte[]		message)
		{
			int length = System.Math.Min(message.Length, n.BitLength / 8);

			return new BigInteger(1, message, 0, length);
		}
	}
}
