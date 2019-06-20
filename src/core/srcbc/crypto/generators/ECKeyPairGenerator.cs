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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Generators
{
    public class ECKeyPairGenerator
        : IAsymmetricCipherKeyPairGenerator
    {
        private readonly string algorithm;

        private ECDomainParameters parameters;
        private DerObjectIdentifier publicKeyParamSet;
        private SecureRandom random;

        public ECKeyPairGenerator()
            : this("EC")
        {
        }

        public ECKeyPairGenerator(
            string algorithm)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");

            this.algorithm = ECKeyParameters.VerifyAlgorithmName(algorithm);
        }

        public void Init(
            KeyGenerationParameters parameters)
        {
            if (parameters is ECKeyGenerationParameters)
            {
                ECKeyGenerationParameters ecP = (ECKeyGenerationParameters) parameters;

                this.publicKeyParamSet = ecP.PublicKeyParamSet;
                this.parameters = ecP.DomainParameters;
            }
            else
            {
                DerObjectIdentifier oid;
                switch (parameters.Strength)
                {
                    case 192:
                        oid = X9ObjectIdentifiers.Prime192v1;
                        break;
                    case 224:
                        oid = SecObjectIdentifiers.SecP224r1;
                        break;
                    case 239:
                        oid = X9ObjectIdentifiers.Prime239v1;
                        break;
                    case 256:
                        oid = X9ObjectIdentifiers.Prime256v1;
                        break;
                    case 384:
                        oid = SecObjectIdentifiers.SecP384r1;
                        break;
                    case 521:
                        oid = SecObjectIdentifiers.SecP521r1;
                        break;
                    default:
                        throw new InvalidParameterException("unknown key size.");
                }

                X9ECParameters ecps = FindECCurveByOid(oid);

                this.parameters = new ECDomainParameters(
                    ecps.Curve, ecps.G, ecps.N, ecps.H, ecps.GetSeed());
            }

            this.random = parameters.Random;
        }

        /**
         * Given the domain parameters this routine Generates an EC key
         * pair in accordance with X9.62 section 5.2.1 pages 26, 27.
         */
        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            BigInteger n = parameters.N;
            BigInteger d;

            do
            {
                d = new BigInteger(n.BitLength, random);
            }
            while (d.SignValue == 0 || (d.CompareTo(n) >= 0));

            ECPoint q = parameters.G.Multiply(d);

            if (publicKeyParamSet != null)
            {
                return new AsymmetricCipherKeyPair(
                    new ECPublicKeyParameters(algorithm, q, publicKeyParamSet),
                    new ECPrivateKeyParameters(algorithm, d, publicKeyParamSet));
            }

            return new AsymmetricCipherKeyPair(
                new ECPublicKeyParameters(algorithm, q, parameters),
                new ECPrivateKeyParameters(algorithm, d, parameters));
        }

        internal static X9ECParameters FindECCurveByOid(DerObjectIdentifier oid)
        {
            // TODO ECGost3410NamedCurves support (returns ECDomainParameters though)

            X9ECParameters ecP = X962NamedCurves.GetByOid(oid);

            if (ecP == null)
            {
                ecP = SecNamedCurves.GetByOid(oid);

                if (ecP == null)
                {
                    ecP = NistNamedCurves.GetByOid(oid);

                    if (ecP == null)
                    {
                        ecP = TeleTrusTNamedCurves.GetByOid(oid);
                    }
                }
            }

            return ecP;
        }

        internal static ECPublicKeyParameters GetCorrespondingPublicKey(
            ECPrivateKeyParameters privKey)
        {
            ECDomainParameters parameters = privKey.Parameters;
            ECPoint q = parameters.G.Multiply(privKey.D);

            if (privKey.PublicKeyParamSet != null)
            {
                return new ECPublicKeyParameters(privKey.AlgorithmName, q, privKey.PublicKeyParamSet);
            }

            return new ECPublicKeyParameters(privKey.AlgorithmName, q, parameters);
        }
    }
}
