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
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;

namespace Org.BouncyCastle.X509
{
	internal class X509SignatureUtilities
	{
		private static readonly Asn1Null derNull = DerNull.Instance;

		internal static void SetSignatureParameters(
			ISigner			signature,
			Asn1Encodable	parameters)
		{
			if (parameters != null && !derNull.Equals(parameters))
			{
				// TODO Put back in
//				AlgorithmParameters sigParams = AlgorithmParameters.GetInstance(signature.getAlgorithm());
//
//				try
//				{
//					sigParams.Init(parameters.ToAsn1Object().GetDerEncoded());
//				}
//				catch (IOException e)
//				{
//					throw new SignatureException("IOException decoding parameters: " + e.Message);
//				}
//
//				if (signature.getAlgorithm().EndsWith("MGF1"))
//				{
//					try
//					{
//						signature.setParameter(sigParams.getParameterSpec(PSSParameterSpec.class));
//					}
//					catch (GeneralSecurityException e)
//					{
//						throw new SignatureException("Exception extracting parameters: " + e.Message);
//					}
//				}
			}
		}

		internal static string GetSignatureName(
			AlgorithmIdentifier sigAlgId)
		{
			Asn1Encodable parameters = sigAlgId.Parameters;

			if (parameters != null && !derNull.Equals(parameters))
			{
				if (sigAlgId.ObjectID.Equals(PkcsObjectIdentifiers.IdRsassaPss))
				{
					RsassaPssParameters rsaParams = RsassaPssParameters.GetInstance(parameters);

					return GetDigestAlgName(rsaParams.HashAlgorithm.ObjectID) + "withRSAandMGF1";
				}
				if (sigAlgId.ObjectID.Equals(X9ObjectIdentifiers.ECDsaWithSha2))
				{
					Asn1Sequence ecDsaParams = Asn1Sequence.GetInstance(parameters);

					return GetDigestAlgName((DerObjectIdentifier)ecDsaParams[0]) + "withECDSA";
				}
			}

			return sigAlgId.ObjectID.Id;
		}

		/**
		 * Return the digest algorithm using one of the standard JCA string
		 * representations rather than the algorithm identifier (if possible).
		 */
		private static string GetDigestAlgName(
			DerObjectIdentifier digestAlgOID)
		{
			if (PkcsObjectIdentifiers.MD5.Equals(digestAlgOID))
			{
				return "MD5";
			}
			else if (OiwObjectIdentifiers.IdSha1.Equals(digestAlgOID))
			{
				return "SHA1";
			}
			else if (NistObjectIdentifiers.IdSha224.Equals(digestAlgOID))
			{
				return "SHA224";
			}
			else if (NistObjectIdentifiers.IdSha256.Equals(digestAlgOID))
			{
				return "SHA256";
			}
			else if (NistObjectIdentifiers.IdSha384.Equals(digestAlgOID))
			{
				return "SHA384";
			}
			else if (NistObjectIdentifiers.IdSha512.Equals(digestAlgOID))
			{
				return "SHA512";
			}
			else if (TeleTrusTObjectIdentifiers.RipeMD128.Equals(digestAlgOID))
			{
				return "RIPEMD128";
			}
			else if (TeleTrusTObjectIdentifiers.RipeMD160.Equals(digestAlgOID))
			{
				return "RIPEMD160";
			}
			else if (TeleTrusTObjectIdentifiers.RipeMD256.Equals(digestAlgOID))
			{
				return "RIPEMD256";
			}
			else if (CryptoProObjectIdentifiers.GostR3411.Equals(digestAlgOID))
			{
				return "GOST3411";
			}
			else
			{
				return digestAlgOID.Id;
			}
		}
	}
}
