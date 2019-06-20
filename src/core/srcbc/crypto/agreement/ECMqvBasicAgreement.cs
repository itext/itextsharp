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
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Crypto.Agreement
{
    public class ECMqvBasicAgreement
        : IBasicAgreement
    {
        protected internal MqvPrivateParameters privParams;

        public virtual void Init(
            ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom)parameters).Parameters;
            }

            this.privParams = (MqvPrivateParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (privParams.StaticPrivateKey.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(
            ICipherParameters pubKey)
        {
            MqvPublicParameters pubParams = (MqvPublicParameters)pubKey;

            ECPrivateKeyParameters staticPrivateKey = privParams.StaticPrivateKey;

            ECPoint agreement = calculateMqvAgreement(staticPrivateKey.Parameters, staticPrivateKey,
                privParams.EphemeralPrivateKey, privParams.EphemeralPublicKey,
                pubParams.StaticPublicKey, pubParams.EphemeralPublicKey);

            return agreement.X.ToBigInteger();
        }
        
        // The ECMQV Primitive as described in SEC-1, 3.4
        private static ECPoint calculateMqvAgreement(
            ECDomainParameters		parameters,
            ECPrivateKeyParameters	d1U,
            ECPrivateKeyParameters	d2U,
            ECPublicKeyParameters	Q2U,
            ECPublicKeyParameters	Q1V,
            ECPublicKeyParameters	Q2V)
        {
            BigInteger n = parameters.N;
            int e = (n.BitLength + 1) / 2;
            BigInteger powE = BigInteger.One.ShiftLeft(e);

            // The Q2U public key is optional
            ECPoint q;
            if (Q2U == null)
            {
                q = parameters.G.Multiply(d2U.D);
            }
            else
            {
                q = Q2U.Q;
            }

            BigInteger x = q.X.ToBigInteger();
            BigInteger xBar = x.Mod(powE);
            BigInteger Q2UBar = xBar.SetBit(e);
            BigInteger s = d1U.D.Multiply(Q2UBar).Mod(n).Add(d2U.D).Mod(n);

            BigInteger xPrime = Q2V.Q.X.ToBigInteger();
            BigInteger xPrimeBar = xPrime.Mod(powE);
            BigInteger Q2VBar = xPrimeBar.SetBit(e);

            BigInteger hs = parameters.H.Multiply(s).Mod(n);

            //ECPoint p = Q1V.Q.Multiply(Q2VBar).Add(Q2V.Q).Multiply(hs);
            ECPoint p = ECAlgorithms.SumOfTwoMultiplies(
                Q1V.Q, Q2VBar.Multiply(hs).Mod(n), Q2V.Q, hs);

            if (p.IsInfinity)
                throw new InvalidOperationException("Infinity is not a valid agreement value for MQV");

            return p;
        }
    }
}
