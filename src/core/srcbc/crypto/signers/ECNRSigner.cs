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
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Signers
{
	/**
	 * EC-NR as described in IEEE 1363-2000
	 */
	public class ECNRSigner
		: IDsa
	{
		private bool			forSigning;
		private ECKeyParameters	key;
		private SecureRandom	random;

		public string AlgorithmName
		{
			get { return "ECNR"; }
		}

		public void Init(
			bool				forSigning,
			ICipherParameters	parameters)
		{
			this.forSigning = forSigning;

			if (forSigning)
			{
				if (parameters is ParametersWithRandom)
				{
					ParametersWithRandom rParam = (ParametersWithRandom) parameters;

					this.random = rParam.Random;
					parameters = rParam.Parameters;
				}
				else
				{
					this.random = new SecureRandom();
				}

				if (!(parameters is ECPrivateKeyParameters))
					throw new InvalidKeyException("EC private key required for signing");

				this.key = (ECPrivateKeyParameters) parameters;
			}
			else
			{
				if (!(parameters is ECPublicKeyParameters))
					throw new InvalidKeyException("EC public key required for verification");

				this.key = (ECPublicKeyParameters) parameters;
			}
		}

		// Section 7.2.5 ECSP-NR, pg 34
		/**
		 * generate a signature for the given message using the key we were
		 * initialised with.  Generally, the order of the curve should be at
		 * least as long as the hash of the message of interest, and with
		 * ECNR it *must* be at least as long.
		 *
		 * @param digest  the digest to be signed.
		 * @exception DataLengthException if the digest is longer than the key allows
		 */
		public BigInteger[] GenerateSignature(
			byte[] message)
		{
			if (!this.forSigning)
			{
				// not properly initilaized... deal with it
				throw new InvalidOperationException("not initialised for signing");
			}

			BigInteger n = ((ECPrivateKeyParameters) this.key).Parameters.N;
			int nBitLength = n.BitLength;

			BigInteger e = new BigInteger(1, message);
			int eBitLength = e.BitLength;

			ECPrivateKeyParameters  privKey = (ECPrivateKeyParameters)key;

			if (eBitLength > nBitLength)
			{
				throw new DataLengthException("input too large for ECNR key.");
			}

			BigInteger r = null;
			BigInteger s = null;

			AsymmetricCipherKeyPair tempPair;
			do // generate r
			{
				// generate another, but very temporary, key pair using
				// the same EC parameters
				ECKeyPairGenerator keyGen = new ECKeyPairGenerator();

				keyGen.Init(new ECKeyGenerationParameters(privKey.Parameters, this.random));

				tempPair = keyGen.GenerateKeyPair();

				//    BigInteger Vx = tempPair.getPublic().getW().getAffineX();
				ECPublicKeyParameters V = (ECPublicKeyParameters) tempPair.Public; // get temp's public key
				BigInteger Vx = V.Q.X.ToBigInteger(); // get the point's x coordinate

				r = Vx.Add(e).Mod(n);
			}
			while (r.SignValue == 0);

			// generate s
			BigInteger x = privKey.D;                // private key value
			BigInteger u = ((ECPrivateKeyParameters) tempPair.Private).D; // temp's private key value
			s = u.Subtract(r.Multiply(x)).Mod(n);

			return new BigInteger[]{ r, s };
		}

		// Section 7.2.6 ECVP-NR, pg 35
		/**
		 * return true if the value r and s represent a signature for the
		 * message passed in. Generally, the order of the curve should be at
		 * least as long as the hash of the message of interest, and with
		 * ECNR, it *must* be at least as long.  But just in case the signer
		 * applied mod(n) to the longer digest, this implementation will
		 * apply mod(n) during verification.
		 *
		 * @param digest  the digest to be verified.
		 * @param r       the r value of the signature.
		 * @param s       the s value of the signature.
		 * @exception DataLengthException if the digest is longer than the key allows
		 */
		public bool VerifySignature(
			byte[]		message,
			BigInteger	r,
			BigInteger	s)
		{
			if (this.forSigning)
			{
				// not properly initilaized... deal with it
				throw new InvalidOperationException("not initialised for verifying");
			}

			ECPublicKeyParameters pubKey = (ECPublicKeyParameters)key;
			BigInteger n = pubKey.Parameters.N;
			int nBitLength = n.BitLength;

			BigInteger e = new BigInteger(1, message);
			int eBitLength = e.BitLength;

			if (eBitLength > nBitLength)
			{
				throw new DataLengthException("input too large for ECNR key.");
			}

			// r in the range [1,n-1]
			if (r.CompareTo(BigInteger.One) < 0 || r.CompareTo(n) >= 0)
			{
				return false;
			}

			// TODO So why is this different from the spec?
			// s in the range [0,n-1]           NB: ECNR spec says 0
			if (s.CompareTo(BigInteger.Zero) < 0 || s.CompareTo(n) >= 0)
			{
				return false;
			}

			// compute P = sG + rW

			ECPoint G = pubKey.Parameters.G;
			ECPoint W = pubKey.Q;
			// calculate P using Bouncy math
			ECPoint P = ECAlgorithms.SumOfTwoMultiplies(G, s, W, r);

            if (P.IsInfinity)
                return false;

			BigInteger x = P.X.ToBigInteger();
			BigInteger t = r.Subtract(x).Mod(n);

			return t.Equals(e);
		}
	}
}
