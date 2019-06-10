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
using System.Collections;
using System.IO;
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;

namespace Org.BouncyCastle.Security
{
    public sealed class PrivateKeyFactory
    {
        private PrivateKeyFactory()
        {
        }

		public static AsymmetricKeyParameter CreateKey(
			byte[] privateKeyInfoData)
		{
			return CreateKey(
				PrivateKeyInfo.GetInstance(
					Asn1Object.FromByteArray(privateKeyInfoData)));
		}

		public static AsymmetricKeyParameter CreateKey(
			Stream inStr)
		{
			return CreateKey(
				PrivateKeyInfo.GetInstance(
					Asn1Object.FromStream(inStr)));
		}

		public static AsymmetricKeyParameter CreateKey(
			PrivateKeyInfo keyInfo)
        {
            AlgorithmIdentifier algID = keyInfo.AlgorithmID;
			DerObjectIdentifier algOid = algID.ObjectID;

			// TODO See RSAUtil.isRsaOid in Java build
			if (algOid.Equals(PkcsObjectIdentifiers.RsaEncryption)
				|| algOid.Equals(X509ObjectIdentifiers.IdEARsa)
				|| algOid.Equals(PkcsObjectIdentifiers.IdRsassaPss)
				|| algOid.Equals(PkcsObjectIdentifiers.IdRsaesOaep))
			{
				RsaPrivateKeyStructure keyStructure = new RsaPrivateKeyStructure(
					Asn1Sequence.GetInstance(keyInfo.PrivateKey));

				return new RsaPrivateCrtKeyParameters(
					keyStructure.Modulus,
					keyStructure.PublicExponent,
					keyStructure.PrivateExponent,
					keyStructure.Prime1,
					keyStructure.Prime2,
					keyStructure.Exponent1,
					keyStructure.Exponent2,
					keyStructure.Coefficient);
			}
			// TODO?
//			else if (algOid.Equals(X9ObjectIdentifiers.DHPublicNumber))
			else if (algOid.Equals(PkcsObjectIdentifiers.DhKeyAgreement))
			{
				DHParameter para = new DHParameter(
					Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object()));
				DerInteger derX = (DerInteger)keyInfo.PrivateKey;

				BigInteger lVal = para.L;
				int l = lVal == null ? 0 : lVal.IntValue;
				DHParameters dhParams = new DHParameters(para.P, para.G, null, l);

				return new DHPrivateKeyParameters(derX.Value, dhParams, algOid);
			}
			else if (algOid.Equals(OiwObjectIdentifiers.ElGamalAlgorithm))
			{
				ElGamalParameter  para = new ElGamalParameter(
					Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object()));
				DerInteger derX = (DerInteger)keyInfo.PrivateKey;

				return new ElGamalPrivateKeyParameters(
					derX.Value,
					new ElGamalParameters(para.P, para.G));
			}
			else if (algOid.Equals(X9ObjectIdentifiers.IdDsa))
			{
				DerInteger derX = (DerInteger) keyInfo.PrivateKey;
				Asn1Encodable ae = algID.Parameters;

				DsaParameters parameters = null;
				if (ae != null)
				{
					DsaParameter para = DsaParameter.GetInstance(ae.ToAsn1Object());
					parameters = new DsaParameters(para.P, para.Q, para.G);
				}

				return new DsaPrivateKeyParameters(derX.Value, parameters);
			}
			else if (algOid.Equals(X9ObjectIdentifiers.IdECPublicKey))
			{
				X962Parameters para = new X962Parameters(algID.Parameters.ToAsn1Object());

				X9ECParameters x9;
				if (para.IsNamedCurve)
				{
                    x9 = ECKeyPairGenerator.FindECCurveByOid((DerObjectIdentifier)para.Parameters);
				}
				else
				{
                    x9 = new X9ECParameters((Asn1Sequence)para.Parameters);
				}

                ECPrivateKeyStructure ec = new ECPrivateKeyStructure(
                    Asn1Sequence.GetInstance(keyInfo.PrivateKey));
                BigInteger d = ec.GetKey();

                if (para.IsNamedCurve)
                {
                    return new ECPrivateKeyParameters("EC", d, (DerObjectIdentifier)para.Parameters);
                }

                ECDomainParameters dParams = new ECDomainParameters(x9.Curve, x9.G, x9.N, x9.H,  x9.GetSeed());
				return new ECPrivateKeyParameters(d, dParams);
			}
			else if (algOid.Equals(CryptoProObjectIdentifiers.GostR3410x2001))
			{
				Gost3410PublicKeyAlgParameters gostParams = new Gost3410PublicKeyAlgParameters(
					Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object()));

				ECPrivateKeyStructure ec = new ECPrivateKeyStructure(
					Asn1Sequence.GetInstance(keyInfo.PrivateKey));

				ECDomainParameters ecP = ECGost3410NamedCurves.GetByOid(gostParams.PublicKeyParamSet);

				if (ecP == null)
					return null;

				return new ECPrivateKeyParameters("ECGOST3410", ec.GetKey(), gostParams.PublicKeyParamSet);
			}
			else if (algOid.Equals(CryptoProObjectIdentifiers.GostR3410x94))
			{
				Gost3410PublicKeyAlgParameters gostParams = new Gost3410PublicKeyAlgParameters(
					Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object()));

				DerOctetString derX = (DerOctetString) keyInfo.PrivateKey;
				byte[] keyEnc = derX.GetOctets();
				byte[] keyBytes = new byte[keyEnc.Length];

				for (int i = 0; i != keyEnc.Length; i++)
				{
					keyBytes[i] = keyEnc[keyEnc.Length - 1 - i]; // was little endian
				}

				BigInteger x = new BigInteger(1, keyBytes);

				return new Gost3410PrivateKeyParameters(x, gostParams.PublicKeyParamSet);
			}
			else
			{
				throw new SecurityUtilityException("algorithm identifier in key not recognised");
			}
        }

		public static AsymmetricKeyParameter DecryptKey(
			char[]					passPhrase,
			EncryptedPrivateKeyInfo	encInfo)
		{
			return CreateKey(PrivateKeyInfoFactory.CreatePrivateKeyInfo(passPhrase, encInfo));
		}

		public static AsymmetricKeyParameter DecryptKey(
			char[]	passPhrase,
			byte[]	encryptedPrivateKeyInfoData)
		{
			return DecryptKey(passPhrase, Asn1Object.FromByteArray(encryptedPrivateKeyInfoData));
		}

		public static AsymmetricKeyParameter DecryptKey(
			char[]	passPhrase,
			Stream	encryptedPrivateKeyInfoStream)
		{
			return DecryptKey(passPhrase, Asn1Object.FromStream(encryptedPrivateKeyInfoStream));
		}

		private static AsymmetricKeyParameter DecryptKey(
			char[]		passPhrase,
			Asn1Object	asn1Object)
		{
			return DecryptKey(passPhrase, EncryptedPrivateKeyInfo.GetInstance(asn1Object));
		}

        public static byte[] EncryptKey(
            DerObjectIdentifier		algorithm,
            char[]					passPhrase,
            byte[]					salt,
            int						iterationCount,
            AsymmetricKeyParameter	key)
        {
			return EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(
				algorithm, passPhrase, salt, iterationCount, key).GetEncoded();
        }

		public static byte[] EncryptKey(
			string					algorithm,
            char[]					passPhrase,
            byte[]					salt,
            int						iterationCount,
            AsymmetricKeyParameter	key)
        {
			return EncryptedPrivateKeyInfoFactory.CreateEncryptedPrivateKeyInfo(
				algorithm, passPhrase, salt, iterationCount, key).GetEncoded();
        }
	}
}
