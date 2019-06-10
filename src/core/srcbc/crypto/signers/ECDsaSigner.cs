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
     * EC-DSA as described in X9.62
     */
    public class ECDsaSigner
        : IDsa
    {
        private ECKeyParameters key;
        private SecureRandom random;

        public string AlgorithmName
        {
            get { return "ECDSA"; }
        }

        public void Init(
            bool				forSigning,
            ICipherParameters	parameters)
        {
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

        // 5.3 pg 28
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
            BigInteger n = key.Parameters.N;
            BigInteger e = calculateE(n, message);

            BigInteger r = null;
            BigInteger s = null;

            // 5.3.2
            do // Generate s
            {
                BigInteger k = null;

                do // Generate r
                {
                    do
                    {
                        k = new BigInteger(n.BitLength, random);
                    }
                    while (k.SignValue == 0 || k.CompareTo(n) >= 0);

                    ECPoint p = key.Parameters.G.Multiply(k);

                    // 5.3.3
                    BigInteger x = p.X.ToBigInteger();

                    r = x.Mod(n);
                }
                while (r.SignValue == 0);

                BigInteger d = ((ECPrivateKeyParameters)key).D;

                s = k.ModInverse(n).Multiply(e.Add(d.Multiply(r).Mod(n))).Mod(n);
            }
            while (s.SignValue == 0);

            return new BigInteger[]{ r, s };
        }

        // 5.4 pg 29
        /**
         * return true if the value r and s represent a DSA signature for
         * the passed in message (for standard DSA the message should be
         * a SHA-1 hash of the real message to be verified).
         */
        public bool VerifySignature(
            byte[]		message,
            BigInteger	r,
            BigInteger	s)
        {
            BigInteger n = key.Parameters.N;

            // r and s should both in the range [1,n-1]
            if (r.SignValue < 1 || s.SignValue < 1
                || r.CompareTo(n) >= 0 || s.CompareTo(n) >= 0)
            {
                return false;
            }

            BigInteger e = calculateE(n, message);
            BigInteger c = s.ModInverse(n);

            BigInteger u1 = e.Multiply(c).Mod(n);
            BigInteger u2 = r.Multiply(c).Mod(n);

            ECPoint G = key.Parameters.G;
            ECPoint Q = ((ECPublicKeyParameters) key).Q;

            ECPoint point = ECAlgorithms.SumOfTwoMultiplies(G, u1, Q, u2);

            if (point.IsInfinity)
                return false;

            BigInteger v = point.X.ToBigInteger().Mod(n);

            return v.Equals(r);
        }

        private BigInteger calculateE(
            BigInteger	n,
            byte[]		message)
        {
            int messageBitLength = message.Length * 8;
            BigInteger trunc = new BigInteger(1, message);

            if (n.BitLength < messageBitLength)
            {
                trunc = trunc.ShiftRight(messageBitLength - n.BitLength);
            }

            return trunc;
        }
    }
}
