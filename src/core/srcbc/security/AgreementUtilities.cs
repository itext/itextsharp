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
using System.Collections;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security
{
	/// <remarks>
	///  Utility class for creating IBasicAgreement objects from their names/Oids
	/// </remarks>
	public sealed class AgreementUtilities
	{
		private AgreementUtilities()
		{
		}

		private static readonly IDictionary algorithms = Platform.CreateHashtable();
        //private static readonly IDictionary oids = Platform.CreateHashtable();

		static AgreementUtilities()
		{
			//algorithms[X9ObjectIdentifiers.DHSinglePassCofactorDHSha1KdfScheme.Id] = ?;
			algorithms[X9ObjectIdentifiers.DHSinglePassStdDHSha1KdfScheme.Id] = "ECDHWITHSHA1KDF";
			algorithms[X9ObjectIdentifiers.MqvSinglePassSha1KdfScheme.Id] = "ECMQVWITHSHA1KDF";
		}

		public static IBasicAgreement GetBasicAgreement(
			DerObjectIdentifier oid)
		{
			return GetBasicAgreement(oid.Id);
		}

		public static IBasicAgreement GetBasicAgreement(
			string algorithm)
		{
			string upper = Platform.ToUpperInvariant(algorithm);
			string mechanism = (string) algorithms[upper];

			if (mechanism == null)
			{
				mechanism = upper;
			}

			if (mechanism == "DH" || mechanism == "DIFFIEHELLMAN")
				return new DHBasicAgreement();

			if (mechanism == "ECDH")
				return new ECDHBasicAgreement();

			if (mechanism == "ECDHC")
				return new ECDHCBasicAgreement();

			if (mechanism == "ECMQV")
				return new ECMqvBasicAgreement();

			throw new SecurityUtilityException("Basic Agreement " + algorithm + " not recognised.");
		}

		public static IBasicAgreement GetBasicAgreementWithKdf(
			DerObjectIdentifier oid,
			string				wrapAlgorithm)
		{
			return GetBasicAgreementWithKdf(oid.Id, wrapAlgorithm);
		}

		public static IBasicAgreement GetBasicAgreementWithKdf(
			string agreeAlgorithm,
			string wrapAlgorithm)
		{
			string upper = Platform.ToUpperInvariant(agreeAlgorithm);
			string mechanism = (string) algorithms[upper];

			if (mechanism == null)
			{
				mechanism = upper;
			}

			// 'DHWITHSHA1KDF' retained for backward compatibility
			if (mechanism == "DHWITHSHA1KDF" || mechanism == "ECDHWITHSHA1KDF")
				return new ECDHWithKdfBasicAgreement(
					wrapAlgorithm,
					new ECDHKekGenerator(
						new Sha1Digest()));

			if (mechanism == "ECMQVWITHSHA1KDF")
				return new ECMqvWithKdfBasicAgreement(
					wrapAlgorithm,
					new ECDHKekGenerator(
						new Sha1Digest()));

			throw new SecurityUtilityException("Basic Agreement (with KDF) " + agreeAlgorithm + " not recognised.");
		}

		public static string GetAlgorithmName(
			DerObjectIdentifier oid)
		{
			return (string) algorithms[oid.Id];
		}
	}
}
