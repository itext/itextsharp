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
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security
{
    /// <remarks>
    ///  Utility class for creating IDigest objects from their names/Oids
    /// </remarks>
    public sealed class DigestUtilities
    {
        private enum DigestAlgorithm {
            GOST3411,
            MD2, MD4, MD5,
            RIPEMD128, RIPEMD160, RIPEMD256, RIPEMD320,
            SHA_1, SHA_224, SHA_256, SHA_384, SHA_512,
            SHA_512_224, SHA_512_256,
            SHA3_224, SHA3_256, SHA3_384, SHA3_512,
            TIGER,
            WHIRLPOOL,
        };

        private DigestUtilities()
        {
        }

        private static readonly IDictionary algorithms = Platform.CreateHashtable();
        private static readonly IDictionary oids = Platform.CreateHashtable();

        static DigestUtilities()
        {
            // Signal to obfuscation tools not to change enum constants
            ((DigestAlgorithm)Enums.GetArbitraryValue(typeof(DigestAlgorithm))).ToString();

            algorithms[PkcsObjectIdentifiers.MD2.Id] = "MD2";
            algorithms[PkcsObjectIdentifiers.MD4.Id] = "MD4";
            algorithms[PkcsObjectIdentifiers.MD5.Id] = "MD5";

            algorithms["SHA1"] = "SHA-1";
            algorithms[OiwObjectIdentifiers.IdSha1.Id] = "SHA-1";
            algorithms["SHA224"] = "SHA-224";
            algorithms[NistObjectIdentifiers.IdSha224.Id] = "SHA-224";
            algorithms["SHA256"] = "SHA-256";
            algorithms[NistObjectIdentifiers.IdSha256.Id] = "SHA-256";
            algorithms["SHA384"] = "SHA-384";
            algorithms[NistObjectIdentifiers.IdSha384.Id] = "SHA-384";
            algorithms["SHA512"] = "SHA-512";
            algorithms[NistObjectIdentifiers.IdSha512.Id] = "SHA-512";
            algorithms["SHA512/224"] = "SHA-512/224";
            algorithms[NistObjectIdentifiers.IdSha512_224.Id] = "SHA-512/224";
            algorithms["SHA512/256"] = "SHA-512/256";
            algorithms[NistObjectIdentifiers.IdSha512_256.Id] = "SHA-512/256";

            algorithms["RIPEMD-128"] = "RIPEMD128";
            algorithms[TeleTrusTObjectIdentifiers.RipeMD128.Id] = "RIPEMD128";
            algorithms["RIPEMD-160"] = "RIPEMD160";
            algorithms[TeleTrusTObjectIdentifiers.RipeMD160.Id] = "RIPEMD160";
            algorithms["RIPEMD-256"] = "RIPEMD256";
            algorithms[TeleTrusTObjectIdentifiers.RipeMD256.Id] = "RIPEMD256";
            algorithms["RIPEMD-320"] = "RIPEMD320";
//			algorithms[TeleTrusTObjectIdentifiers.RipeMD320.Id] = "RIPEMD320";

            algorithms[CryptoProObjectIdentifiers.GostR3411.Id] = "GOST3411";



            oids["MD2"] = PkcsObjectIdentifiers.MD2;
            oids["MD4"] = PkcsObjectIdentifiers.MD4;
            oids["MD5"] = PkcsObjectIdentifiers.MD5;
            oids["SHA-1"] = OiwObjectIdentifiers.IdSha1;
            oids["SHA-224"] = NistObjectIdentifiers.IdSha224;
            oids["SHA-256"] = NistObjectIdentifiers.IdSha256;
            oids["SHA-384"] = NistObjectIdentifiers.IdSha384;
            oids["SHA-512"] = NistObjectIdentifiers.IdSha512;
            oids["SHA-512/224"] = NistObjectIdentifiers.IdSha512_224;
            oids["SHA-512/256"] = NistObjectIdentifiers.IdSha512_256;
            oids["RIPEMD128"] = TeleTrusTObjectIdentifiers.RipeMD128;
            oids["RIPEMD160"] = TeleTrusTObjectIdentifiers.RipeMD160;
            oids["RIPEMD256"] = TeleTrusTObjectIdentifiers.RipeMD256;
            oids["GOST3411"] = CryptoProObjectIdentifiers.GostR3411;
        }

        /// <summary>
        /// Returns a ObjectIdentifier for a given digest mechanism.
        /// </summary>
        /// <param name="mechanism">A string representation of the digest meanism.</param>
        /// <returns>A DerObjectIdentifier, null if the Oid is not available.</returns>

        public static DerObjectIdentifier GetObjectIdentifier(
            string mechanism)
        {
            if (mechanism == null)
                throw new System.ArgumentNullException("mechanism");

            mechanism = Platform.ToUpperInvariant(mechanism);
            string aliased = (string) algorithms[mechanism];

            if (aliased != null)
                mechanism = aliased;

            return (DerObjectIdentifier) oids[mechanism];
        }

        public static ICollection Algorithms
        {
            get { return oids.Keys; }
        }

        public static IDigest GetDigest(
            DerObjectIdentifier id)
        {
            return GetDigest(id.Id);
        }

        public static IDigest GetDigest(
            string algorithm)
        {
            string upper = Platform.ToUpperInvariant(algorithm);
            string mechanism = (string) algorithms[upper];

            if (mechanism == null)
            {
                mechanism = upper;
            }

            try
            {
                DigestAlgorithm digestAlgorithm = (DigestAlgorithm)Enums.GetEnumValue(
                    typeof(DigestAlgorithm), mechanism);

                switch (digestAlgorithm)
                {
                    case DigestAlgorithm.GOST3411:      return new Gost3411Digest();
                    case DigestAlgorithm.MD2:		    return new MD2Digest();
                    case DigestAlgorithm.MD4:		    return new MD4Digest();
                    case DigestAlgorithm.MD5:		    return new MD5Digest();
                    case DigestAlgorithm.RIPEMD128:	    return new RipeMD128Digest();
                    case DigestAlgorithm.RIPEMD160:	    return new RipeMD160Digest();
                    case DigestAlgorithm.RIPEMD256:	    return new RipeMD256Digest();
                    case DigestAlgorithm.RIPEMD320:	    return new RipeMD320Digest();
                    case DigestAlgorithm.SHA_1:		    return new Sha1Digest();
                    case DigestAlgorithm.SHA_224:	    return new Sha224Digest();
                    case DigestAlgorithm.SHA_256:	    return new Sha256Digest();
                    case DigestAlgorithm.SHA_384:	    return new Sha384Digest();
                    case DigestAlgorithm.SHA_512:	    return new Sha512Digest();
                    case DigestAlgorithm.SHA_512_224:   return new Sha512tDigest(224);
                    case DigestAlgorithm.SHA_512_256:   return new Sha512tDigest(256);
                    case DigestAlgorithm.SHA3_224:      return new Sha3Digest(224);
                    case DigestAlgorithm.SHA3_256:      return new Sha3Digest(256);
                    case DigestAlgorithm.SHA3_384:      return new Sha3Digest(384);
                    case DigestAlgorithm.SHA3_512:      return new Sha3Digest(512);
                    case DigestAlgorithm.TIGER:         return new TigerDigest();
                    case DigestAlgorithm.WHIRLPOOL:     return new WhirlpoolDigest();
                }
            }
            catch (ArgumentException)
            {
            }

            throw new SecurityUtilityException("Digest " + mechanism + " not recognised.");
        }

        public static string GetAlgorithmName(
            DerObjectIdentifier oid)
        {
            return (string) algorithms[oid.Id];
        }

        public static byte[] CalculateDigest(string algorithm, byte[] input)
        {
            IDigest digest = GetDigest(algorithm);
            digest.BlockUpdate(input, 0, input.Length);
            return DoFinal(digest);
        }

        public static byte[] DoFinal(
            IDigest digest)
        {
            byte[] b = new byte[digest.GetDigestSize()];
            digest.DoFinal(b, 0);
            return b;
        }

        public static byte[] DoFinal(
            IDigest	digest,
            byte[]	input)
        {
            digest.BlockUpdate(input, 0, input.Length);
            return DoFinal(digest);
        }
    }
}
