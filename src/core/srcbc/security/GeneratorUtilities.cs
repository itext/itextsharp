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
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Iana;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Security
{
    public sealed class GeneratorUtilities
    {
        private GeneratorUtilities()
        {
        }

        private static readonly IDictionary kgAlgorithms = Platform.CreateHashtable();
        private static readonly IDictionary kpgAlgorithms = Platform.CreateHashtable();
        private static readonly IDictionary defaultKeySizes = Platform.CreateHashtable();

        static GeneratorUtilities()
        {
            //
            // key generators.
            //
            AddKgAlgorithm("AES",
                "AESWRAP");
            AddKgAlgorithm("AES128",
                "2.16.840.1.101.3.4.2",
                NistObjectIdentifiers.IdAes128Cbc,
                NistObjectIdentifiers.IdAes128Cfb,
                NistObjectIdentifiers.IdAes128Ecb,
                NistObjectIdentifiers.IdAes128Ofb,
                NistObjectIdentifiers.IdAes128Wrap);
            AddKgAlgorithm("AES192",
                "2.16.840.1.101.3.4.22",
                NistObjectIdentifiers.IdAes192Cbc,
                NistObjectIdentifiers.IdAes192Cfb,
                NistObjectIdentifiers.IdAes192Ecb,
                NistObjectIdentifiers.IdAes192Ofb,
                NistObjectIdentifiers.IdAes192Wrap);
            AddKgAlgorithm("AES256",
                "2.16.840.1.101.3.4.42",
                NistObjectIdentifiers.IdAes256Cbc,
                NistObjectIdentifiers.IdAes256Cfb,
                NistObjectIdentifiers.IdAes256Ecb,
                NistObjectIdentifiers.IdAes256Ofb,
                NistObjectIdentifiers.IdAes256Wrap);
            AddKgAlgorithm("BLOWFISH",
                "1.3.6.1.4.1.3029.1.2");
            AddKgAlgorithm("CAMELLIA",
                "CAMELLIAWRAP");
            AddKgAlgorithm("CAMELLIA128",
                NttObjectIdentifiers.IdCamellia128Cbc,
                NttObjectIdentifiers.IdCamellia128Wrap);
            AddKgAlgorithm("CAMELLIA192",
                NttObjectIdentifiers.IdCamellia192Cbc,
                NttObjectIdentifiers.IdCamellia192Wrap);
            AddKgAlgorithm("CAMELLIA256",
                NttObjectIdentifiers.IdCamellia256Cbc,
                NttObjectIdentifiers.IdCamellia256Wrap);
            AddKgAlgorithm("CAST5",
                "1.2.840.113533.7.66.10");
            AddKgAlgorithm("CAST6");
            AddKgAlgorithm("DES",
                OiwObjectIdentifiers.DesCbc,
                OiwObjectIdentifiers.DesCfb,
                OiwObjectIdentifiers.DesEcb,
                OiwObjectIdentifiers.DesOfb);
            AddKgAlgorithm("DESEDE",
                "DESEDEWRAP",
                "TDEA",
                OiwObjectIdentifiers.DesEde);
            AddKgAlgorithm("DESEDE3",
                PkcsObjectIdentifiers.DesEde3Cbc,
                PkcsObjectIdentifiers.IdAlgCms3DesWrap);
            AddKgAlgorithm("GOST28147",
                "GOST",
                "GOST-28147",
                CryptoProObjectIdentifiers.GostR28147Cbc);
            AddKgAlgorithm("HC128");
            AddKgAlgorithm("HC256");
            AddKgAlgorithm("IDEA",
                "1.3.6.1.4.1.188.7.1.1.2");
            AddKgAlgorithm("NOEKEON");
            AddKgAlgorithm("RC2",
                PkcsObjectIdentifiers.RC2Cbc,
                PkcsObjectIdentifiers.IdAlgCmsRC2Wrap);
            AddKgAlgorithm("RC4",
                "ARC4",
                "1.2.840.113549.3.4");
            AddKgAlgorithm("RC5",
                "RC5-32");
            AddKgAlgorithm("RC5-64");
            AddKgAlgorithm("RC6");
            AddKgAlgorithm("RIJNDAEL");
            AddKgAlgorithm("SALSA20");
            AddKgAlgorithm("SEED",
                KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap,
                KisaObjectIdentifiers.IdSeedCbc);
            AddKgAlgorithm("SERPENT");
            AddKgAlgorithm("SKIPJACK");
            AddKgAlgorithm("TEA");
            AddKgAlgorithm("TWOFISH");
            AddKgAlgorithm("VMPC");
            AddKgAlgorithm("VMPC-KSA3");
            AddKgAlgorithm("XTEA");

            //
            // HMac key generators
            //
            AddHMacKeyGenerator("MD2");
            AddHMacKeyGenerator("MD4");
            AddHMacKeyGenerator("MD5",
                IanaObjectIdentifiers.HmacMD5);
            AddHMacKeyGenerator("SHA1",
                PkcsObjectIdentifiers.IdHmacWithSha1,
                IanaObjectIdentifiers.HmacSha1);
            AddHMacKeyGenerator("SHA224",
                PkcsObjectIdentifiers.IdHmacWithSha224);
            AddHMacKeyGenerator("SHA256",
                PkcsObjectIdentifiers.IdHmacWithSha256);
            AddHMacKeyGenerator("SHA384",
                PkcsObjectIdentifiers.IdHmacWithSha384);
            AddHMacKeyGenerator("SHA512",
                PkcsObjectIdentifiers.IdHmacWithSha512);
            AddHMacKeyGenerator("SHA512/224");
            AddHMacKeyGenerator("SHA512/256");
            AddHMacKeyGenerator("SHA3-224");
            AddHMacKeyGenerator("SHA3-256");
            AddHMacKeyGenerator("SHA3-384");
            AddHMacKeyGenerator("SHA3-512");
            AddHMacKeyGenerator("RIPEMD128");
            AddHMacKeyGenerator("RIPEMD160",
                IanaObjectIdentifiers.HmacRipeMD160);
            AddHMacKeyGenerator("TIGER",
                IanaObjectIdentifiers.HmacTiger);



            //
            // key pair generators.
            //
            AddKpgAlgorithm("DH",
                "DIFFIEHELLMAN");
            AddKpgAlgorithm("DSA");
            AddKpgAlgorithm("EC",
                // TODO Should this be an alias for ECDH?
                X9ObjectIdentifiers.DHSinglePassStdDHSha1KdfScheme);
            AddKpgAlgorithm("ECDH",
                "ECIES");
            AddKpgAlgorithm("ECDHC");
            AddKpgAlgorithm("ECMQV",
                X9ObjectIdentifiers.MqvSinglePassSha1KdfScheme);
            AddKpgAlgorithm("ECDSA");
            AddKpgAlgorithm("ECGOST3410",
                "ECGOST-3410",
                "GOST-3410-2001");
            AddKpgAlgorithm("ELGAMAL");
            AddKpgAlgorithm("GOST3410",
                "GOST-3410",
                "GOST-3410-94");
            AddKpgAlgorithm("RSA",
                "1.2.840.113549.1.1.1");

            AddDefaultKeySizeEntries(64, "DES");
            AddDefaultKeySizeEntries(80, "SKIPJACK");
            AddDefaultKeySizeEntries(128, "AES128", "BLOWFISH", "CAMELLIA128", "CAST5", "DESEDE",
                "HC128", "HMACMD2", "HMACMD4", "HMACMD5", "HMACRIPEMD128", "IDEA", "NOEKEON",
                "RC2", "RC4", "RC5", "SALSA20", "SEED", "TEA", "XTEA", "VMPC", "VMPC-KSA3");
            AddDefaultKeySizeEntries(160, "HMACRIPEMD160", "HMACSHA1");
            AddDefaultKeySizeEntries(192, "AES", "AES192", "CAMELLIA192", "DESEDE3", "HMACTIGER",
                "RIJNDAEL", "SERPENT");
            AddDefaultKeySizeEntries(224, "HMACSHA224");
            AddDefaultKeySizeEntries(256, "AES256", "CAMELLIA", "CAMELLIA256", "CAST6", "GOST28147",
                "HC256", "HMACSHA256", "RC5-64", "RC6", "TWOFISH");
            AddDefaultKeySizeEntries(384, "HMACSHA384");
            AddDefaultKeySizeEntries(512, "HMACSHA512");
            AddDefaultKeySizeEntries(224, "HMACSHA512/224");
            AddDefaultKeySizeEntries(256, "HMACSHA512/256");
        }

        private static void AddDefaultKeySizeEntries(int size, params string[] algorithms)
        {
            foreach (string algorithm in algorithms)
            {
                defaultKeySizes.Add(algorithm, size);
            }
        }

        private static void AddKgAlgorithm(
            string			canonicalName,
            params object[] aliases)
        {
            kgAlgorithms[canonicalName] = canonicalName;

            foreach (object alias in aliases)
            {
                kgAlgorithms[alias.ToString()] = canonicalName;
            }
        }

        private static void AddKpgAlgorithm(
            string			canonicalName,
            params object[] aliases)
        {
            kpgAlgorithms[canonicalName] = canonicalName;

            foreach (object alias in aliases)
            {
                kpgAlgorithms[alias.ToString()] = canonicalName;
            }
        }

        private static void AddHMacKeyGenerator(
            string			algorithm,
            params object[]	aliases)
        {
            string mainName = "HMAC" + algorithm;

            kgAlgorithms[mainName] = mainName;
            kgAlgorithms["HMAC-" + algorithm] = mainName;
            kgAlgorithms["HMAC/" + algorithm] = mainName;

            foreach (object alias in aliases)
            {
                kgAlgorithms[alias.ToString()] = mainName;
            }
        }

        // TODO Consider making this public
        internal static string GetCanonicalKeyGeneratorAlgorithm(
            string algorithm)
        {
            return (string) kgAlgorithms[Platform.ToUpperInvariant(algorithm)];
        }

        // TODO Consider making this public
        internal static string GetCanonicalKeyPairGeneratorAlgorithm(
            string algorithm)
        {
            return (string)kpgAlgorithms[Platform.ToUpperInvariant(algorithm)];
        }

        public static CipherKeyGenerator GetKeyGenerator(
            DerObjectIdentifier oid)
        {
            return GetKeyGenerator(oid.Id);
        }

        public static CipherKeyGenerator GetKeyGenerator(
            string algorithm)
        {
            string canonicalName = GetCanonicalKeyGeneratorAlgorithm(algorithm);

            if (canonicalName == null)
                throw new SecurityUtilityException("KeyGenerator " + algorithm + " not recognised.");

            int defaultKeySize = FindDefaultKeySize(canonicalName);
            if (defaultKeySize == -1)
                throw new SecurityUtilityException("KeyGenerator " + algorithm
                    + " (" + canonicalName + ") not supported.");

            if (canonicalName == "DES")
                return new DesKeyGenerator(defaultKeySize);

            if (canonicalName == "DESEDE" || canonicalName == "DESEDE3")
                return new DesEdeKeyGenerator(defaultKeySize);

            return new CipherKeyGenerator(defaultKeySize);
        }

        public static IAsymmetricCipherKeyPairGenerator GetKeyPairGenerator(
            DerObjectIdentifier oid)
        {
            return GetKeyPairGenerator(oid.Id);
        }

        public static IAsymmetricCipherKeyPairGenerator GetKeyPairGenerator(
            string algorithm)
        {
            string canonicalName = GetCanonicalKeyPairGeneratorAlgorithm(algorithm);

            if (canonicalName == null)
                throw new SecurityUtilityException("KeyPairGenerator " + algorithm + " not recognised.");

            if (canonicalName == "DH")
                return new DHKeyPairGenerator();

            if (canonicalName == "DSA")
                return new DsaKeyPairGenerator();

            // "EC", "ECDH", "ECDHC", "ECDSA", "ECGOST3410", "ECMQV"
            if (canonicalName.StartsWith("EC"))
                return new ECKeyPairGenerator(canonicalName);

            if (canonicalName == "ELGAMAL")
                return new ElGamalKeyPairGenerator();

            if (canonicalName == "GOST3410")
                return new Gost3410KeyPairGenerator();

            if (canonicalName == "RSA")
                return new RsaKeyPairGenerator();

            throw new SecurityUtilityException("KeyPairGenerator " + algorithm
                + " (" + canonicalName + ") not supported.");
        }

        internal static int GetDefaultKeySize(
            DerObjectIdentifier oid)
        {
            return GetDefaultKeySize(oid.Id);
        }

        internal static int GetDefaultKeySize(
            string algorithm)
        {
            string canonicalName = GetCanonicalKeyGeneratorAlgorithm(algorithm);

            if (canonicalName == null)
                throw new SecurityUtilityException("KeyGenerator " + algorithm + " not recognised.");

            int defaultKeySize = FindDefaultKeySize(canonicalName);
            if (defaultKeySize == -1)
                throw new SecurityUtilityException("KeyGenerator " + algorithm
                    + " (" + canonicalName + ") not supported.");

            return defaultKeySize;
        }

        private static int FindDefaultKeySize(
            string canonicalName)
        {
            if (!defaultKeySizes.Contains(canonicalName))
                return -1;

            return (int)defaultKeySizes[canonicalName];
        }
    }
}
