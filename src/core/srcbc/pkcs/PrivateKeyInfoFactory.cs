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
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Pkcs
{
    public sealed class PrivateKeyInfoFactory
    {
        private PrivateKeyInfoFactory()
        {
        }

        public static PrivateKeyInfo CreatePrivateKeyInfo(
            AsymmetricKeyParameter key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (!key.IsPrivate)
                throw new ArgumentException("Public key passed - private key expected", "key");

            if (key is ElGamalPrivateKeyParameters)
            {
                ElGamalPrivateKeyParameters _key = (ElGamalPrivateKeyParameters)key;
                return new PrivateKeyInfo(
                    new AlgorithmIdentifier(
                    OiwObjectIdentifiers.ElGamalAlgorithm,
                    new ElGamalParameter(
                    _key.Parameters.P,
                    _key.Parameters.G).ToAsn1Object()),
                    new DerInteger(_key.X));
            }

            if (key is DsaPrivateKeyParameters)
            {
                DsaPrivateKeyParameters _key = (DsaPrivateKeyParameters)key;
                return new PrivateKeyInfo(
                    new AlgorithmIdentifier(
                    X9ObjectIdentifiers.IdDsa,
                    new DsaParameter(
                    _key.Parameters.P,
                    _key.Parameters.Q,
                    _key.Parameters.G).ToAsn1Object()),
                    new DerInteger(_key.X));
            }

            if (key is DHPrivateKeyParameters)
            {
                DHPrivateKeyParameters _key = (DHPrivateKeyParameters)key;

                DHParameter p = new DHParameter(
                    _key.Parameters.P, _key.Parameters.G, _key.Parameters.L);

                return new PrivateKeyInfo(
                    new AlgorithmIdentifier(_key.AlgorithmOid, p.ToAsn1Object()),
                    new DerInteger(_key.X));
            }

            if (key is RsaKeyParameters)
            {
                AlgorithmIdentifier algID = new AlgorithmIdentifier(
                    PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance);

                RsaPrivateKeyStructure keyStruct;
                if (key is RsaPrivateCrtKeyParameters)
                {
                    RsaPrivateCrtKeyParameters _key = (RsaPrivateCrtKeyParameters)key;

                    keyStruct = new RsaPrivateKeyStructure(
                        _key.Modulus,
                        _key.PublicExponent,
                        _key.Exponent,
                        _key.P,
                        _key.Q,
                        _key.DP,
                        _key.DQ,
                        _key.QInv);
                }
                else
                {
                    RsaKeyParameters _key = (RsaKeyParameters) key;

                    keyStruct = new RsaPrivateKeyStructure(
                        _key.Modulus,
                        BigInteger.Zero,
                        _key.Exponent,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero,
                        BigInteger.Zero);
                }

                return new PrivateKeyInfo(algID, keyStruct.ToAsn1Object());
            }

            if (key is ECPrivateKeyParameters)
            {
                ECPrivateKeyParameters _key = (ECPrivateKeyParameters)key;
                AlgorithmIdentifier algID;
                ECPrivateKeyStructure ec;

                if (_key.AlgorithmName == "ECGOST3410")
                {
                    if (_key.PublicKeyParamSet == null)
                        throw Platform.CreateNotImplementedException("Not a CryptoPro parameter set");

                    Gost3410PublicKeyAlgParameters gostParams = new Gost3410PublicKeyAlgParameters(
                        _key.PublicKeyParamSet, CryptoProObjectIdentifiers.GostR3411x94CryptoProParamSet);

                    algID = new AlgorithmIdentifier(
                        CryptoProObjectIdentifiers.GostR3410x2001,
                        gostParams.ToAsn1Object());

                    // TODO Do we need to pass any parameters here?
                    ec = new ECPrivateKeyStructure(_key.D);
                }
                else
                {
                    X962Parameters x962;
                    if (_key.PublicKeyParamSet == null)
                    {
                        ECDomainParameters kp = _key.Parameters;
                        X9ECParameters ecP = new X9ECParameters(kp.Curve, kp.G, kp.N, kp.H, kp.GetSeed());

                        x962 = new X962Parameters(ecP);
                    }
                    else
                    {
                        x962 = new X962Parameters(_key.PublicKeyParamSet);
                    }

                    Asn1Object x962Object = x962.ToAsn1Object();

                    // TODO Possible to pass the publicKey bitstring here?
                    ec = new ECPrivateKeyStructure(_key.D, x962Object);

                    algID = new AlgorithmIdentifier(X9ObjectIdentifiers.IdECPublicKey, x962Object);
                }

                return new PrivateKeyInfo(algID, ec.ToAsn1Object());
            }

            if (key is Gost3410PrivateKeyParameters)
            {
                Gost3410PrivateKeyParameters _key = (Gost3410PrivateKeyParameters)key;

                if (_key.PublicKeyParamSet == null)
                    throw Platform.CreateNotImplementedException("Not a CryptoPro parameter set");

                byte[] keyEnc = _key.X.ToByteArrayUnsigned();
                byte[] keyBytes = new byte[keyEnc.Length];

                for (int i = 0; i != keyBytes.Length; i++)
                {
                    keyBytes[i] = keyEnc[keyEnc.Length - 1 - i]; // must be little endian
                }

                Gost3410PublicKeyAlgParameters algParams = new Gost3410PublicKeyAlgParameters(
                    _key.PublicKeyParamSet, CryptoProObjectIdentifiers.GostR3411x94CryptoProParamSet, null);

                AlgorithmIdentifier algID = new AlgorithmIdentifier(
                    CryptoProObjectIdentifiers.GostR3410x94,
                    algParams.ToAsn1Object());

                return new PrivateKeyInfo(algID, new DerOctetString(keyBytes));
            }

            throw new ArgumentException("Class provided is not convertible: " + key.GetType().FullName);
        }

        public static PrivateKeyInfo CreatePrivateKeyInfo(
            char[]					passPhrase,
            EncryptedPrivateKeyInfo	encInfo)
        {
            return CreatePrivateKeyInfo(passPhrase, false, encInfo);
        }

        public static PrivateKeyInfo CreatePrivateKeyInfo(
            char[]					passPhrase,
            bool					wrongPkcs12Zero,
            EncryptedPrivateKeyInfo	encInfo)
        {
            AlgorithmIdentifier algID = encInfo.EncryptionAlgorithm;

            IBufferedCipher cipher = PbeUtilities.CreateEngine(algID) as IBufferedCipher;
            if (cipher == null)
                throw new Exception("Unknown encryption algorithm: " + algID.ObjectID);

            ICipherParameters cipherParameters = PbeUtilities.GenerateCipherParameters(
                algID, passPhrase, wrongPkcs12Zero);
            cipher.Init(false, cipherParameters);
            byte[] keyBytes = cipher.DoFinal(encInfo.GetEncryptedData());

            return PrivateKeyInfo.GetInstance(keyBytes);
        }
    }
}
