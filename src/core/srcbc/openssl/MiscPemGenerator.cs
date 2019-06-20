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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.OpenSsl
{
	/**
	* PEM generator for the original set of PEM objects used in Open SSL.
	*/
	public class MiscPemGenerator
		: PemObjectGenerator
	{
		private object obj;
		private string algorithm;
		private char[] password;
		private SecureRandom random;

		public MiscPemGenerator(object obj)
		{
			this.obj = obj;
		}

		public MiscPemGenerator(
			object			obj,
			string			algorithm,
			char[]			password,
			SecureRandom	random)
		{
			this.obj = obj;
			this.algorithm = algorithm;
			this.password = password;
			this.random = random;
		}

		private static PemObject CreatePemObject(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (obj is AsymmetricCipherKeyPair)
			{
				return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private);
			}

			string type;
			byte[] encoding;

			if (obj is PemObject)
				return (PemObject)obj;

			if (obj is PemObjectGenerator)
				return ((PemObjectGenerator)obj).Generate();

			if (obj is X509Certificate)
			{
				// TODO Should we prefer "X509 CERTIFICATE" here?
				type = "CERTIFICATE";
				try
				{
					encoding = ((X509Certificate)obj).GetEncoded();
				}
				catch (CertificateEncodingException e)
				{
					throw new IOException("Cannot Encode object: " + e.ToString());
				}
			}
			else if (obj is X509Crl)
			{
				type = "X509 CRL";
				try
				{
					encoding = ((X509Crl)obj).GetEncoded();
				}
				catch (CrlException e)
				{
					throw new IOException("Cannot Encode object: " + e.ToString());
				}
			}
			else if (obj is AsymmetricKeyParameter)
			{
				AsymmetricKeyParameter akp = (AsymmetricKeyParameter) obj;
				if (akp.IsPrivate)
				{
					string keyType;
					encoding = EncodePrivateKey(akp, out keyType);

					type = keyType + " PRIVATE KEY";
				}
				else
				{
					type = "PUBLIC KEY";

					encoding = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(akp).GetDerEncoded();
				}
			}
			else if (obj is IX509AttributeCertificate)
			{
				type = "ATTRIBUTE CERTIFICATE";
				encoding = ((X509V2AttributeCertificate)obj).GetEncoded();
			}
			else if (obj is Pkcs10CertificationRequest)
			{
				type = "CERTIFICATE REQUEST";
				encoding = ((Pkcs10CertificationRequest)obj).GetEncoded();
			}
			else if (obj is Asn1.Cms.ContentInfo)
			{
				type = "PKCS7";
				encoding = ((Asn1.Cms.ContentInfo)obj).GetEncoded();
			}
			else
			{
				throw new PemGenerationException("Object type not supported: " + obj.GetType().FullName);
			}

			return new PemObject(type, encoding);
		}

//		private string GetHexEncoded(byte[] bytes)
//		{
//			bytes = Hex.Encode(bytes);
//
//			char[] chars = new char[bytes.Length];
//
//			for (int i = 0; i != bytes.Length; i++)
//			{
//				chars[i] = (char)bytes[i];
//			}
//
//			return new string(chars);
//		}

		private static PemObject CreatePemObject(
			object			obj,
			string			algorithm,
			char[]			password,
			SecureRandom	random)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			if (algorithm == null)
				throw new ArgumentNullException("algorithm");
			if (password == null)
				throw new ArgumentNullException("password");
			if (random == null)
				throw new ArgumentNullException("random");

			if (obj is AsymmetricCipherKeyPair)
			{
				return CreatePemObject(((AsymmetricCipherKeyPair)obj).Private, algorithm, password, random);
			}

			string type = null;
			byte[] keyData = null;

			if (obj is AsymmetricKeyParameter)
			{
				AsymmetricKeyParameter akp = (AsymmetricKeyParameter) obj;
				if (akp.IsPrivate)
				{
					string keyType;
					keyData = EncodePrivateKey(akp, out keyType);

					type = keyType + " PRIVATE KEY";
				}
			}

			if (type == null || keyData == null)
			{
				// TODO Support other types?
				throw new PemGenerationException("Object type not supported: " + obj.GetType().FullName);
			}


			string dekAlgName = Platform.ToUpperInvariant(algorithm);

            // Note: For backward compatibility
			if (dekAlgName == "DESEDE")
			{
				dekAlgName = "DES-EDE3-CBC";
			}

			int ivLength = dekAlgName.StartsWith("AES-") ? 16 : 8;

			byte[] iv = new byte[ivLength];
			random.NextBytes(iv);

			byte[] encData = PemUtilities.Crypt(true, keyData, password, dekAlgName, iv);

			IList headers = Platform.CreateArrayList(2);

			headers.Add(new PemHeader("Proc-Type", "4,ENCRYPTED"));
			headers.Add(new PemHeader("DEK-Info", dekAlgName + "," + Hex.ToHexString(iv)));

			return new PemObject(type, headers, encData);
		}

		private static byte[] EncodePrivateKey(
			AsymmetricKeyParameter	akp,
			out string				keyType)
		{
			PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp);

			DerObjectIdentifier oid = info.AlgorithmID.ObjectID;

			if (oid.Equals(X9ObjectIdentifiers.IdDsa))
			{
				keyType = "DSA";

				DsaParameter p = DsaParameter.GetInstance(info.AlgorithmID.Parameters);

				BigInteger x = ((DsaPrivateKeyParameters) akp).X;
				BigInteger y = p.G.ModPow(x, p.P);

				// TODO Create an ASN1 object somewhere for this?
				return new DerSequence(
					new DerInteger(0),
					new DerInteger(p.P),
					new DerInteger(p.Q),
					new DerInteger(p.G),
					new DerInteger(y),
					new DerInteger(x)).GetEncoded();
			}

			if (oid.Equals(PkcsObjectIdentifiers.RsaEncryption))
			{
				keyType = "RSA";
			}
			else if (oid.Equals(CryptoProObjectIdentifiers.GostR3410x2001)
				|| oid.Equals(X9ObjectIdentifiers.IdECPublicKey))
			{
				keyType = "EC";
			}
			else
			{
				throw new ArgumentException("Cannot handle private key of type: " + akp.GetType().FullName, "akp");
			}

			return info.PrivateKey.GetEncoded();
		}

		public PemObject Generate()
		{
			try
			{
				if (algorithm != null)
				{
					return CreatePemObject(obj, algorithm, password, random);
				}

				return CreatePemObject(obj);
			}
			catch (IOException e)
			{
				throw new PemGenerationException("encoding exception", e);
			}
		}
	}
}
