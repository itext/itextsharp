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
#if !(NETCF_1_0 || SILVERLIGHT)

using System;
using System.Security.Cryptography;
using SystemX509 = System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Security
{
	/// <summary>
	/// A class containing methods to interface the BouncyCastle world to the .NET Crypto world.
	/// </summary>
	public sealed class DotNetUtilities
	{
		private DotNetUtilities()
		{
		}

		/// <summary>
		/// Create an System.Security.Cryptography.X509Certificate from an X509Certificate Structure.
		/// </summary>
		/// <param name="x509Struct"></param>
		/// <returns>A System.Security.Cryptography.X509Certificate.</returns>
		public static SystemX509.X509Certificate ToX509Certificate(
			X509CertificateStructure x509Struct)
		{
			return new SystemX509.X509Certificate(x509Struct.GetDerEncoded());
		}

		public static SystemX509.X509Certificate ToX509Certificate(
			X509Certificate x509Cert)
		{
			return new SystemX509.X509Certificate(x509Cert.GetEncoded());
		}

		public static X509Certificate FromX509Certificate(
			SystemX509.X509Certificate x509Cert)
		{
			return new X509CertificateParser().ReadCertificate(x509Cert.GetRawCertData());
		}

		public static AsymmetricCipherKeyPair GetDsaKeyPair(
			DSA dsa)
		{
			return GetDsaKeyPair(dsa.ExportParameters(true));
		}

		public static AsymmetricCipherKeyPair GetDsaKeyPair(
			DSAParameters dp)
		{
			DsaValidationParameters validationParameters = (dp.Seed != null)
				?	new DsaValidationParameters(dp.Seed, dp.Counter)
				:	null;

			DsaParameters parameters = new DsaParameters(
				new BigInteger(1, dp.P),
				new BigInteger(1, dp.Q),
				new BigInteger(1, dp.G),
				validationParameters);

			DsaPublicKeyParameters pubKey = new DsaPublicKeyParameters(
				new BigInteger(1, dp.Y),
				parameters);

			DsaPrivateKeyParameters privKey = new DsaPrivateKeyParameters(
				new BigInteger(1, dp.X),
				parameters);

			return new AsymmetricCipherKeyPair(pubKey, privKey);
		}

		public static DsaPublicKeyParameters GetDsaPublicKey(
			DSA dsa)
		{
			return GetDsaPublicKey(dsa.ExportParameters(false));
		}

		public static DsaPublicKeyParameters GetDsaPublicKey(
			DSAParameters dp)
		{
			DsaValidationParameters validationParameters = (dp.Seed != null)
				?	new DsaValidationParameters(dp.Seed, dp.Counter)
				:	null;

			DsaParameters parameters = new DsaParameters(
				new BigInteger(1, dp.P),
				new BigInteger(1, dp.Q),
				new BigInteger(1, dp.G),
				validationParameters);

			return new DsaPublicKeyParameters(
				new BigInteger(1, dp.Y),
				parameters);
		}

		public static AsymmetricCipherKeyPair GetRsaKeyPair(
			RSA rsa)
		{
			return GetRsaKeyPair(rsa.ExportParameters(true));
		}

		public static AsymmetricCipherKeyPair GetRsaKeyPair(
			RSAParameters rp)
		{
			BigInteger modulus = new BigInteger(1, rp.Modulus);
			BigInteger pubExp = new BigInteger(1, rp.Exponent);

			RsaKeyParameters pubKey = new RsaKeyParameters(
				false,
				modulus,
				pubExp);

			RsaPrivateCrtKeyParameters privKey = new RsaPrivateCrtKeyParameters(
				modulus,
				pubExp,
				new BigInteger(1, rp.D),
				new BigInteger(1, rp.P),
				new BigInteger(1, rp.Q),
				new BigInteger(1, rp.DP),
				new BigInteger(1, rp.DQ),
				new BigInteger(1, rp.InverseQ));

			return new AsymmetricCipherKeyPair(pubKey, privKey);
		}

		public static RsaKeyParameters GetRsaPublicKey(
			RSA rsa)
		{
			return GetRsaPublicKey(rsa.ExportParameters(false));
		}

		public static RsaKeyParameters GetRsaPublicKey(
			RSAParameters rp)
		{
			return new RsaKeyParameters(
				false,
				new BigInteger(1, rp.Modulus),
				new BigInteger(1, rp.Exponent));
		}

		public static AsymmetricCipherKeyPair GetKeyPair(AsymmetricAlgorithm privateKey)
		{
			if (privateKey is DSA)
			{
				return GetDsaKeyPair((DSA)privateKey);
			}

			if (privateKey is RSA)
			{
				return GetRsaKeyPair((RSA)privateKey);
			}

			throw new ArgumentException("Unsupported algorithm specified", "privateKey");
		}

		public static RSA ToRSA(RsaKeyParameters rsaKey)
		{
			RSAParameters rp = ToRSAParameters(rsaKey);
			RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider();
			// TODO This call appears to not work for private keys (when no CRT info)
			rsaCsp.ImportParameters(rp);
			return rsaCsp;
		}

		public static RSA ToRSA(RsaPrivateCrtKeyParameters privKey)
		{
			RSAParameters rp = ToRSAParameters(privKey);
			RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider();
			rsaCsp.ImportParameters(rp);
			return rsaCsp;
		}

		public static RSAParameters ToRSAParameters(RsaKeyParameters rsaKey)
		{
			RSAParameters rp = new RSAParameters();
			rp.Modulus = rsaKey.Modulus.ToByteArrayUnsigned();
			if (rsaKey.IsPrivate)
				rp.D = ConvertRSAParametersField(rsaKey.Exponent, rp.Modulus.Length);
			else
				rp.Exponent = rsaKey.Exponent.ToByteArrayUnsigned();
			return rp;
		}

		public static RSAParameters ToRSAParameters(RsaPrivateCrtKeyParameters privKey)
		{
			RSAParameters rp = new RSAParameters();
			rp.Modulus = privKey.Modulus.ToByteArrayUnsigned();
			rp.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
			rp.P = privKey.P.ToByteArrayUnsigned();
			rp.Q = privKey.Q.ToByteArrayUnsigned();
			rp.D = ConvertRSAParametersField(privKey.Exponent, rp.Modulus.Length);
			rp.DP = ConvertRSAParametersField(privKey.DP, rp.P.Length);
			rp.DQ = ConvertRSAParametersField(privKey.DQ, rp.Q.Length);
			rp.InverseQ = ConvertRSAParametersField(privKey.QInv, rp.Q.Length);
			return rp;
		}

		// TODO Move functionality to more general class
		private static byte[] ConvertRSAParametersField(BigInteger n, int size)
		{
			byte[] bs = n.ToByteArrayUnsigned();

			if (bs.Length == size)
				return bs;

			if (bs.Length > size)
				throw new ArgumentException("Specified size too small", "size");

			byte[] padded = new byte[size];
			Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
			return padded;
		}
	}
}

#endif
