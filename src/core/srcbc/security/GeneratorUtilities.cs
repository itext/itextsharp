using System.Collections;
using System.Globalization;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Iana;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;

namespace Org.BouncyCastle.Security
{
	public sealed class GeneratorUtilities
	{
		private GeneratorUtilities()
		{
		}

		private static readonly Hashtable kgAlgorithms = new Hashtable();
		private static readonly Hashtable kpgAlgorithms = new Hashtable();

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
				OiwObjectIdentifiers.DesEde,
				PkcsObjectIdentifiers.IdAlgCms3DesWrap);
			AddKgAlgorithm("DESEDE3",
				PkcsObjectIdentifiers.DesEde3Cbc);
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
			AddKpgAlgorithm("EC");
			AddKpgAlgorithm("ECDH",
				"ECIES");
			AddKpgAlgorithm("ECDHC");
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
			return (string) kgAlgorithms[algorithm.ToUpper(CultureInfo.InvariantCulture)];
		}

		// TODO Consider making this public
		internal static string GetCanonicalKeyPairGeneratorAlgorithm(
			string algorithm)
		{
			return (string) kpgAlgorithms[algorithm.ToUpper(CultureInfo.InvariantCulture)];
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

			switch (canonicalName)
			{
				case "DES":
					return new DesKeyGenerator(defaultKeySize);
				case "DESEDE":
				case "DESEDE3":
					return new DesEdeKeyGenerator(defaultKeySize);
				default:
					return new CipherKeyGenerator(defaultKeySize);
			}
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

			switch (canonicalName)
			{
				case "DH":
					return new DHKeyPairGenerator();
				case "DSA":
					return new DsaKeyPairGenerator();
				case "EC":
				case "ECDH":
				case "ECDHC":
				case "ECDSA":
				case "ECGOST3410":
					return new ECKeyPairGenerator(canonicalName);
				case "ELGAMAL":
					return new ElGamalKeyPairGenerator();
				case "GOST3410":
					return new Gost3410KeyPairGenerator();
				case "RSA":
					return new RsaKeyPairGenerator();
				default:
					break;
			}

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
			switch (canonicalName)
			{
				case "DES":
					return 64;
				case "SKIPJACK":
					return 80;
				case "AES128":
				case "BLOWFISH":
				case "CAMELLIA128":
				case "CAST5":
				case "DESEDE":
				case "HC128":
				case "HMACMD2":
				case "HMACMD4":
				case "HMACMD5":
				case "HMACRIPEMD128":
				case "IDEA":
				case "NOEKEON":
				case "RC2":
				case "RC4":
				case "RC5":
				case "SALSA20":
				case "SEED":
				case "TEA":
				case "XTEA":
				case "VMPC":
				case "VMPC-KSA3":
					return 128;
				case "HMACRIPEMD160":
				case "HMACSHA1":
					return 160;
				case "AES":
				case "AES192":
				case "CAMELLIA192":
				case "DESEDE3":
				case "HMACTIGER":
				case "RIJNDAEL":
				case "SERPENT":
					return 192;
				case "HMACSHA224":
					return 224;
				case "AES256":
				case "CAMELLIA":
				case "CAMELLIA256":
				case "CAST6":
				case "GOST28147":
				case "HC256":
				case "HMACSHA256":
				case "RC5-64":
				case "RC6":
				case "TWOFISH":
					return 256;
				case "HMACSHA384":
					return 384;
				case "HMACSHA512":
					return 512;
				default:
					return -1;
			}
		}
	}
}
