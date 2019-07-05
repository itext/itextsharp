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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Ocsp
{
	class OcspUtilities
	{
		private static readonly IDictionary algorithms = Platform.CreateHashtable();
        private static readonly IDictionary oids = Platform.CreateHashtable();
		private static readonly ISet noParams = new HashSet();

		static OcspUtilities()
		{
			algorithms.Add("MD2WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD2WithRsaEncryption);
			algorithms.Add("MD2WITHRSA", PkcsObjectIdentifiers.MD2WithRsaEncryption);
			algorithms.Add("MD5WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD5WithRsaEncryption);
			algorithms.Add("MD5WITHRSA", PkcsObjectIdentifiers.MD5WithRsaEncryption);
			algorithms.Add("SHA1WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
			algorithms.Add("SHA1WITHRSA", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
			algorithms.Add("SHA224WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
			algorithms.Add("SHA224WITHRSA", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
			algorithms.Add("SHA256WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
			algorithms.Add("SHA256WITHRSA", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
			algorithms.Add("SHA384WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
			algorithms.Add("SHA384WITHRSA", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
			algorithms.Add("SHA512WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
			algorithms.Add("SHA512WITHRSA", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
			algorithms.Add("RIPEMD160WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
			algorithms.Add("RIPEMD160WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
			algorithms.Add("RIPEMD128WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
			algorithms.Add("RIPEMD128WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
			algorithms.Add("RIPEMD256WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
			algorithms.Add("RIPEMD256WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
			algorithms.Add("SHA1WITHDSA", X9ObjectIdentifiers.IdDsaWithSha1);
			algorithms.Add("DSAWITHSHA1", X9ObjectIdentifiers.IdDsaWithSha1);
			algorithms.Add("SHA224WITHDSA", NistObjectIdentifiers.DsaWithSha224);
			algorithms.Add("SHA256WITHDSA", NistObjectIdentifiers.DsaWithSha256);
			algorithms.Add("SHA1WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha1);
			algorithms.Add("ECDSAWITHSHA1", X9ObjectIdentifiers.ECDsaWithSha1);
			algorithms.Add("SHA224WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha224);
			algorithms.Add("SHA256WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha256);
			algorithms.Add("SHA384WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha384);
			algorithms.Add("SHA512WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha512);
			algorithms.Add("GOST3411WITHGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
			algorithms.Add("GOST3411WITHGOST3410-94", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);

			oids.Add(PkcsObjectIdentifiers.MD2WithRsaEncryption, "MD2WITHRSA");
			oids.Add(PkcsObjectIdentifiers.MD5WithRsaEncryption, "MD5WITHRSA");
			oids.Add(PkcsObjectIdentifiers.Sha1WithRsaEncryption, "SHA1WITHRSA");
			oids.Add(PkcsObjectIdentifiers.Sha224WithRsaEncryption, "SHA224WITHRSA");
			oids.Add(PkcsObjectIdentifiers.Sha256WithRsaEncryption, "SHA256WITHRSA");
			oids.Add(PkcsObjectIdentifiers.Sha384WithRsaEncryption, "SHA384WITHRSA");
			oids.Add(PkcsObjectIdentifiers.Sha512WithRsaEncryption, "SHA512WITHRSA");
			oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160, "RIPEMD160WITHRSA");
			oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128, "RIPEMD128WITHRSA");
			oids.Add(TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256, "RIPEMD256WITHRSA");
			oids.Add(X9ObjectIdentifiers.IdDsaWithSha1, "SHA1WITHDSA");
			oids.Add(NistObjectIdentifiers.DsaWithSha224, "SHA224WITHDSA");
			oids.Add(NistObjectIdentifiers.DsaWithSha256, "SHA256WITHDSA");
			oids.Add(X9ObjectIdentifiers.ECDsaWithSha1, "SHA1WITHECDSA");
			oids.Add(X9ObjectIdentifiers.ECDsaWithSha224, "SHA224WITHECDSA");
			oids.Add(X9ObjectIdentifiers.ECDsaWithSha256, "SHA256WITHECDSA");
			oids.Add(X9ObjectIdentifiers.ECDsaWithSha384, "SHA384WITHECDSA");
			oids.Add(X9ObjectIdentifiers.ECDsaWithSha512, "SHA512WITHECDSA");
			oids.Add(CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94, "GOST3411WITHGOST3410");

			//
			// According to RFC 3279, the ASN.1 encoding SHALL (id-dsa-with-sha1) or MUST (ecdsa-with-SHA*) omit the parameters field.
			// The parameters field SHALL be NULL for RSA based signature algorithms.
			//
			noParams.Add(X9ObjectIdentifiers.ECDsaWithSha1);
			noParams.Add(X9ObjectIdentifiers.ECDsaWithSha224);
			noParams.Add(X9ObjectIdentifiers.ECDsaWithSha256);
			noParams.Add(X9ObjectIdentifiers.ECDsaWithSha384);
			noParams.Add(X9ObjectIdentifiers.ECDsaWithSha512);
			noParams.Add(X9ObjectIdentifiers.IdDsaWithSha1);
			noParams.Add(NistObjectIdentifiers.DsaWithSha224);
			noParams.Add(NistObjectIdentifiers.DsaWithSha256);
		}

		internal static DerObjectIdentifier GetAlgorithmOid(
			string algorithmName)
		{
			algorithmName = Platform.ToUpperInvariant(algorithmName);

            if (algorithms.Contains(algorithmName))
			{
				return (DerObjectIdentifier)algorithms[algorithmName];
			}

			return new DerObjectIdentifier(algorithmName);
		}


		internal static string GetAlgorithmName(
			DerObjectIdentifier oid)
		{
			if (oids.Contains(oid))
			{
				return (string)oids[oid];
			}

			return oid.Id;
		}

		internal static AlgorithmIdentifier GetSigAlgID(
			DerObjectIdentifier sigOid)
		{
			if (noParams.Contains(sigOid))
			{
				return new AlgorithmIdentifier(sigOid);
			}

			return new AlgorithmIdentifier(sigOid, DerNull.Instance);
		}

		internal static IEnumerable AlgNames
		{
			get { return new EnumerableProxy(algorithms.Keys); }
		}
	}
}
