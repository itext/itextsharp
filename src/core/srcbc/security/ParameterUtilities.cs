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
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security
{
    public sealed class ParameterUtilities
    {
        private ParameterUtilities()
        {
        }

        private static readonly IDictionary algorithms = Platform.CreateHashtable();
        private static readonly IDictionary basicIVSizes = Platform.CreateHashtable();

        static ParameterUtilities()
        {
            AddAlgorithm("AES",
                "AESWRAP");
            AddAlgorithm("AES128",
                "2.16.840.1.101.3.4.2",
                NistObjectIdentifiers.IdAes128Cbc,
                NistObjectIdentifiers.IdAes128Cfb,
                NistObjectIdentifiers.IdAes128Ecb,
                NistObjectIdentifiers.IdAes128Ofb,
                NistObjectIdentifiers.IdAes128Wrap);
            AddAlgorithm("AES192",
                "2.16.840.1.101.3.4.22",
                NistObjectIdentifiers.IdAes192Cbc,
                NistObjectIdentifiers.IdAes192Cfb,
                NistObjectIdentifiers.IdAes192Ecb,
                NistObjectIdentifiers.IdAes192Ofb,
                NistObjectIdentifiers.IdAes192Wrap);
            AddAlgorithm("AES256",
                "2.16.840.1.101.3.4.42",
                NistObjectIdentifiers.IdAes256Cbc,
                NistObjectIdentifiers.IdAes256Cfb,
                NistObjectIdentifiers.IdAes256Ecb,
                NistObjectIdentifiers.IdAes256Ofb,
                NistObjectIdentifiers.IdAes256Wrap);
            AddAlgorithm("BLOWFISH",
                "1.3.6.1.4.1.3029.1.2");
            AddAlgorithm("CAMELLIA",
                "CAMELLIAWRAP");
            AddAlgorithm("CAMELLIA128",
                NttObjectIdentifiers.IdCamellia128Cbc,
                NttObjectIdentifiers.IdCamellia128Wrap);
            AddAlgorithm("CAMELLIA192",
                NttObjectIdentifiers.IdCamellia192Cbc,
                NttObjectIdentifiers.IdCamellia192Wrap);
            AddAlgorithm("CAMELLIA256",
                NttObjectIdentifiers.IdCamellia256Cbc,
                NttObjectIdentifiers.IdCamellia256Wrap);
            AddAlgorithm("CAST5",
                "1.2.840.113533.7.66.10");
            AddAlgorithm("CAST6");
            AddAlgorithm("DES",
                OiwObjectIdentifiers.DesCbc,
                OiwObjectIdentifiers.DesCfb,
                OiwObjectIdentifiers.DesEcb,
                OiwObjectIdentifiers.DesOfb);
            AddAlgorithm("DESEDE",
                "DESEDEWRAP",
                "TDEA",
                OiwObjectIdentifiers.DesEde,
                PkcsObjectIdentifiers.IdAlgCms3DesWrap);
            AddAlgorithm("DESEDE3",
                PkcsObjectIdentifiers.DesEde3Cbc);
            AddAlgorithm("GOST28147",
                "GOST",
                "GOST-28147",
                CryptoProObjectIdentifiers.GostR28147Cbc);
            AddAlgorithm("HC128");
            AddAlgorithm("HC256");
#if INCLUDE_IDEA
            AddAlgorithm("IDEA",
                "1.3.6.1.4.1.188.7.1.1.2");
#endif
            AddAlgorithm("NOEKEON");
            AddAlgorithm("RC2",
                PkcsObjectIdentifiers.RC2Cbc,
                PkcsObjectIdentifiers.IdAlgCmsRC2Wrap);
            AddAlgorithm("RC4",
                "ARC4",
                "1.2.840.113549.3.4");
            AddAlgorithm("RC5",
                "RC5-32");
            AddAlgorithm("RC5-64");
            AddAlgorithm("RC6");
            AddAlgorithm("RIJNDAEL");
            AddAlgorithm("SALSA20");
            AddAlgorithm("SEED",
                KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap,
                KisaObjectIdentifiers.IdSeedCbc);
            AddAlgorithm("SERPENT");
            AddAlgorithm("SKIPJACK");
            AddAlgorithm("TEA");
            AddAlgorithm("TWOFISH");
            AddAlgorithm("VMPC");
            AddAlgorithm("VMPC-KSA3");
            AddAlgorithm("XTEA");

            AddBasicIVSizeEntries(8, "BLOWFISH", "DES", "DESEDE", "DESEDE3");
            AddBasicIVSizeEntries(16, "AES", "AES128", "AES192", "AES256",
                "CAMELLIA", "CAMELLIA128", "CAMELLIA192", "CAMELLIA256", "NOEKEON", "SEED");

            // TODO These algorithms support an IV
            // but JCE doesn't seem to provide an AlgorithmParametersGenerator for them
            // "RIJNDAEL", "SKIPJACK", "TWOFISH"
        }

        private static void AddAlgorithm(
            string			canonicalName,
            params object[]	aliases)
        {
            algorithms[canonicalName] = canonicalName;

            foreach (object alias in aliases)
            {
                algorithms[alias.ToString()] = canonicalName;
            }
        }

        private static void AddBasicIVSizeEntries(int size, params string[] algorithms)
        {
            foreach (string algorithm in algorithms)
            {
                basicIVSizes.Add(algorithm, size);
            }
        }

        public static string GetCanonicalAlgorithmName(
            string algorithm)
        {
            return (string) algorithms[Platform.ToUpperInvariant(algorithm)];
        }

        public static KeyParameter CreateKeyParameter(
            DerObjectIdentifier algOid,
            byte[]				keyBytes)
        {
            return CreateKeyParameter(algOid.Id, keyBytes, 0, keyBytes.Length);
        }

        public static KeyParameter CreateKeyParameter(
            string	algorithm,
            byte[]	keyBytes)
        {
            return CreateKeyParameter(algorithm, keyBytes, 0, keyBytes.Length);
        }

        public static KeyParameter CreateKeyParameter(
            DerObjectIdentifier algOid,
            byte[]				keyBytes,
            int					offset,
            int					length)
        {
            return CreateKeyParameter(algOid.Id, keyBytes, offset, length);
        }

        public static KeyParameter CreateKeyParameter(
            string	algorithm,
            byte[]	keyBytes,
            int		offset,
            int		length)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");

            string canonical = GetCanonicalAlgorithmName(algorithm);

            if (canonical == null)
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");

            if (canonical == "DES")
                return new DesParameters(keyBytes, offset, length);

            if (canonical == "DESEDE" || canonical =="DESEDE3")
                return new DesEdeParameters(keyBytes, offset, length);

            if (canonical == "RC2")
                return new RC2Parameters(keyBytes, offset, length);

            return new KeyParameter(keyBytes, offset, length);
        }

        public static ICipherParameters GetCipherParameters(
            DerObjectIdentifier	algOid,
            ICipherParameters	key,
            Asn1Object			asn1Params)
        {
            return GetCipherParameters(algOid.Id, key, asn1Params);
        }

        public static ICipherParameters GetCipherParameters(
            string				algorithm,
            ICipherParameters	key,
            Asn1Object			asn1Params)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");

            string canonical = GetCanonicalAlgorithmName(algorithm);

            if (canonical == null)
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");

            byte[] iv = null;

            try
            {
                // TODO These algorithms support an IV
                // but JCE doesn't seem to provide an AlgorithmParametersGenerator for them
                // "RIJNDAEL", "SKIPJACK", "TWOFISH"

                int basicIVKeySize = FindBasicIVSize(canonical);
                if (basicIVKeySize != -1
                    || canonical == "RIJNDAEL" || canonical == "SKIPJACK" || canonical == "TWOFISH")
                {
                    iv = ((Asn1OctetString) asn1Params).GetOctets();
                }
                else if (canonical == "CAST5")
                {
                    iv = Cast5CbcParameters.GetInstance(asn1Params).GetIV();
                }
#if INCLUDE_IDEA
                else if (canonical == "IDEA")
                {
                    iv = IdeaCbcPar.GetInstance(asn1Params).GetIV();
                }
#endif
                else if (canonical == "RC2")
                {
                    iv = RC2CbcParameter.GetInstance(asn1Params).GetIV();
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not process ASN.1 parameters", e);
            }

            if (iv != null)
            {
                return new ParametersWithIV(key, iv);
            }

            throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
        }

        public static Asn1Encodable GenerateParameters(
            DerObjectIdentifier algID,
            SecureRandom		random)
        {
            return GenerateParameters(algID.Id, random);
        }

        public static Asn1Encodable GenerateParameters(
            string			algorithm,
            SecureRandom	random)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");

            string canonical = GetCanonicalAlgorithmName(algorithm);

            if (canonical == null)
                throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");

            // TODO These algorithms support an IV
            // but JCE doesn't seem to provide an AlgorithmParametersGenerator for them
            // "RIJNDAEL", "SKIPJACK", "TWOFISH"

            int basicIVKeySize = FindBasicIVSize(canonical);
            if (basicIVKeySize != -1)
                return CreateIVOctetString(random, basicIVKeySize);

            if (canonical == "CAST5")
                return new Cast5CbcParameters(CreateIV(random, 8), 128);

#if INCLUDE_IDEA
            if (canonical == "IDEA")
                return new IdeaCbcPar(CreateIV(random, 8));
#endif

            if (canonical == "RC2")
                return new RC2CbcParameter(CreateIV(random, 8));

            throw new SecurityUtilityException("Algorithm " + algorithm + " not recognised.");
        }

        private static Asn1OctetString CreateIVOctetString(
            SecureRandom	random,
            int				ivLength)
        {
            return new DerOctetString(CreateIV(random, ivLength));
        }

        private static byte[] CreateIV(
            SecureRandom	random,
            int				ivLength)
        {
            byte[] iv = new byte[ivLength];
            random.NextBytes(iv);
            return iv;
        }

        private static int FindBasicIVSize(
            string canonicalName)
        {
            if (!basicIVSizes.Contains(canonicalName))
                return -1;

            return (int)basicIVSizes[canonicalName];
        }
    }
}
