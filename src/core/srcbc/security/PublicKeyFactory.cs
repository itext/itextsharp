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
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Security
{
    public sealed class PublicKeyFactory
    {
        private PublicKeyFactory()
        {
        }

		public static AsymmetricKeyParameter CreateKey(
			byte[] keyInfoData)
		{
			return CreateKey(
				SubjectPublicKeyInfo.GetInstance(
					Asn1Object.FromByteArray(keyInfoData)));
		}

		public static AsymmetricKeyParameter CreateKey(
			Stream inStr)
		{
			return CreateKey(
				SubjectPublicKeyInfo.GetInstance(
					Asn1Object.FromStream(inStr)));
		}

		public static AsymmetricKeyParameter CreateKey(
			SubjectPublicKeyInfo keyInfo)
        {
            AlgorithmIdentifier algID = keyInfo.AlgorithmID;
			DerObjectIdentifier algOid = algID.ObjectID;

			// TODO See RSAUtil.isRsaOid in Java build
			if (algOid.Equals(PkcsObjectIdentifiers.RsaEncryption)
				|| algOid.Equals(X509ObjectIdentifiers.IdEARsa)
				|| algOid.Equals(PkcsObjectIdentifiers.IdRsassaPss)
				|| algOid.Equals(PkcsObjectIdentifiers.IdRsaesOaep))
			{
				RsaPublicKeyStructure pubKey = RsaPublicKeyStructure.GetInstance(
					keyInfo.GetPublicKey());

				return new RsaKeyParameters(false, pubKey.Modulus, pubKey.PublicExponent);
			}
			else if (algOid.Equals(X9ObjectIdentifiers.DHPublicNumber))
			{
				Asn1Sequence seq = Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object());

				DHPublicKey dhPublicKey = DHPublicKey.GetInstance(keyInfo.GetPublicKey());

				BigInteger y = dhPublicKey.Y.Value;

				if (IsPkcsDHParam(seq))
					return ReadPkcsDHParam(algOid, y, seq);

				DHDomainParameters dhParams = DHDomainParameters.GetInstance(seq);

				BigInteger p = dhParams.P.Value;
				BigInteger g = dhParams.G.Value;
				BigInteger q = dhParams.Q.Value;

				BigInteger j = null;
				if (dhParams.J != null)
				{
					j = dhParams.J.Value;
				}

				DHValidationParameters validation = null;
				DHValidationParms dhValidationParms = dhParams.ValidationParms;
				if (dhValidationParms != null)
				{
					byte[] seed = dhValidationParms.Seed.GetBytes();
					BigInteger pgenCounter = dhValidationParms.PgenCounter.Value;

					// TODO Check pgenCounter size?

					validation = new DHValidationParameters(seed, pgenCounter.IntValue);
				}

				return new DHPublicKeyParameters(y, new DHParameters(p, g, q, j, validation));
			}
			else if (algOid.Equals(PkcsObjectIdentifiers.DhKeyAgreement))
			{
				Asn1Sequence seq = Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object());

				DerInteger derY = (DerInteger) keyInfo.GetPublicKey();

				return ReadPkcsDHParam(algOid, derY.Value, seq);
			}
			else if (algOid.Equals(OiwObjectIdentifiers.ElGamalAlgorithm))
			{
				ElGamalParameter para = new ElGamalParameter(
					Asn1Sequence.GetInstance(algID.Parameters.ToAsn1Object()));
				DerInteger derY = (DerInteger) keyInfo.GetPublicKey();

				return new ElGamalPublicKeyParameters(
					derY.Value,
					new ElGamalParameters(para.P, para.G));
			}
			else if (algOid.Equals(X9ObjectIdentifiers.IdDsa)
				|| algOid.Equals(OiwObjectIdentifiers.DsaWithSha1))
			{
				DerInteger derY = (DerInteger) keyInfo.GetPublicKey();
				Asn1Encodable ae = algID.Parameters;

				DsaParameters parameters = null;
				if (ae != null)
				{
					DsaParameter para = DsaParameter.GetInstance(ae.ToAsn1Object());
					parameters = new DsaParameters(para.P, para.Q, para.G);
				}

				return new DsaPublicKeyParameters(derY.Value, parameters);
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

                Asn1OctetString key = new DerOctetString(keyInfo.PublicKeyData.GetBytes());
                X9ECPoint derQ = new X9ECPoint(x9.Curve, key);
                ECPoint q = derQ.Point;

                if (para.IsNamedCurve)
                {
                    return new ECPublicKeyParameters("EC", q, (DerObjectIdentifier)para.Parameters);
                }

                ECDomainParameters dParams = new ECDomainParameters(x9.Curve, x9.G, x9.N, x9.H, x9.GetSeed());
				return new ECPublicKeyParameters(q, dParams);
			}
			else if (algOid.Equals(CryptoProObjectIdentifiers.GostR3410x2001))
			{
				Gost3410PublicKeyAlgParameters gostParams = new Gost3410PublicKeyAlgParameters(
					(Asn1Sequence) algID.Parameters);

				Asn1OctetString key;
				try
				{
					key = (Asn1OctetString) keyInfo.GetPublicKey();
				}
				catch (IOException)
				{
					throw new ArgumentException("invalid info structure in GOST3410 public key");
				}

				byte[] keyEnc = key.GetOctets();
				byte[] x = new byte[32];
				byte[] y = new byte[32];

				for (int i = 0; i != y.Length; i++)
				{
					x[i] = keyEnc[32 - 1 - i];
				}

				for (int i = 0; i != x.Length; i++)
				{
					y[i] = keyEnc[64 - 1 - i];
				}

				ECDomainParameters ecP = ECGost3410NamedCurves.GetByOid(gostParams.PublicKeyParamSet);

				if (ecP == null)
					return null;

				ECPoint q = ecP.Curve.CreatePoint(new BigInteger(1, x), new BigInteger(1, y), false);

				return new ECPublicKeyParameters("ECGOST3410", q, gostParams.PublicKeyParamSet);
			}
			else if (algOid.Equals(CryptoProObjectIdentifiers.GostR3410x94))
			{
				Gost3410PublicKeyAlgParameters algParams = new Gost3410PublicKeyAlgParameters(
					(Asn1Sequence) algID.Parameters);

				DerOctetString derY;
				try
				{
					derY = (DerOctetString) keyInfo.GetPublicKey();
				}
				catch (IOException)
				{
					throw new ArgumentException("invalid info structure in GOST3410 public key");
				}

				byte[] keyEnc = derY.GetOctets();
				byte[] keyBytes = new byte[keyEnc.Length];

				for (int i = 0; i != keyEnc.Length; i++)
				{
					keyBytes[i] = keyEnc[keyEnc.Length - 1 - i]; // was little endian
				}

				BigInteger y = new BigInteger(1, keyBytes);

				return new Gost3410PublicKeyParameters(y, algParams.PublicKeyParamSet);
			}
            else
            {
                throw new SecurityUtilityException("algorithm identifier in key not recognised: " + algOid);
            }
        }

		private static bool IsPkcsDHParam(Asn1Sequence seq)
		{
			if (seq.Count == 2)
				return true;

			if (seq.Count > 3)
				return false;

			DerInteger l = DerInteger.GetInstance(seq[2]);
			DerInteger p = DerInteger.GetInstance(seq[0]);

			return l.Value.CompareTo(BigInteger.ValueOf(p.Value.BitLength)) <= 0;
		}

		private static DHPublicKeyParameters ReadPkcsDHParam(DerObjectIdentifier algOid,
			BigInteger y, Asn1Sequence seq)
		{
			DHParameter para = new DHParameter(seq);

			BigInteger lVal = para.L;
			int l = lVal == null ? 0 : lVal.IntValue;
			DHParameters dhParams = new DHParameters(para.P, para.G, null, l);

			return new DHPublicKeyParameters(y, dhParams, algOid);
		}
	}
}
