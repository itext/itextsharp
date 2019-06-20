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
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security
{
    /// <remarks>
    ///  Utility class for creating IWrapper objects from their names/Oids
    /// </remarks>
    public sealed class WrapperUtilities
    {
        private enum WrapAlgorithm { AESWRAP, CAMELLIAWRAP, DESEDEWRAP, RC2WRAP, SEEDWRAP,
            DESEDERFC3211WRAP, AESRFC3211WRAP, CAMELLIARFC3211WRAP };

        private WrapperUtilities()
        {
        }

        private static readonly IDictionary algorithms = Platform.CreateHashtable();
        //private static readonly IDictionary oids = Platform.CreateHashtable();

        static WrapperUtilities()
        {
            // Signal to obfuscation tools not to change enum constants
            ((WrapAlgorithm)Enums.GetArbitraryValue(typeof(WrapAlgorithm))).ToString();

            algorithms[NistObjectIdentifiers.IdAes128Wrap.Id] = "AESWRAP";
            algorithms[NistObjectIdentifiers.IdAes192Wrap.Id] = "AESWRAP";
            algorithms[NistObjectIdentifiers.IdAes256Wrap.Id] = "AESWRAP";

            algorithms[NttObjectIdentifiers.IdCamellia128Wrap.Id] = "CAMELLIAWRAP";
            algorithms[NttObjectIdentifiers.IdCamellia192Wrap.Id] = "CAMELLIAWRAP";
            algorithms[NttObjectIdentifiers.IdCamellia256Wrap.Id] = "CAMELLIAWRAP";

            algorithms[PkcsObjectIdentifiers.IdAlgCms3DesWrap.Id] = "DESEDEWRAP";
            algorithms["TDEAWRAP"] = "DESEDEWRAP";

            algorithms[PkcsObjectIdentifiers.IdAlgCmsRC2Wrap.Id] = "RC2WRAP";

            algorithms[KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap.Id] = "SEEDWRAP";
        }

        public static IWrapper GetWrapper(
            DerObjectIdentifier oid)
        {
            return GetWrapper(oid.Id);
        }

        public static IWrapper GetWrapper(
            string algorithm)
        {
            string upper = Platform.ToUpperInvariant(algorithm);
            string mechanism = (string)algorithms[upper];

            if (mechanism == null)
            {
                mechanism = upper;
            }

            try
            {
                WrapAlgorithm wrapAlgorithm = (WrapAlgorithm)Enums.GetEnumValue(
                    typeof(WrapAlgorithm), mechanism);

                switch (wrapAlgorithm)
                {
                    case WrapAlgorithm.AESWRAP:				return new AesWrapEngine();
                    case WrapAlgorithm.CAMELLIAWRAP:		return new CamelliaWrapEngine();
                    case WrapAlgorithm.DESEDEWRAP:			return new DesEdeWrapEngine();
                    case WrapAlgorithm.RC2WRAP:				return new RC2WrapEngine();
                    case WrapAlgorithm.SEEDWRAP:			return new SeedWrapEngine();
                    case WrapAlgorithm.DESEDERFC3211WRAP:	return new Rfc3211WrapEngine(new DesEdeEngine());
                    case WrapAlgorithm.AESRFC3211WRAP:		return new Rfc3211WrapEngine(new AesFastEngine());
                    case WrapAlgorithm.CAMELLIARFC3211WRAP:	return new Rfc3211WrapEngine(new CamelliaEngine());
                }
            }
            catch (ArgumentException)
            {
            }

            // Create an IBufferedCipher and use it as IWrapper (via BufferedCipherWrapper)
            IBufferedCipher blockCipher = CipherUtilities.GetCipher(algorithm);

            if (blockCipher != null)
                return new BufferedCipherWrapper(blockCipher);

            throw new SecurityUtilityException("Wrapper " + algorithm + " not recognised.");
        }

        public static string GetAlgorithmName(
            DerObjectIdentifier oid)
        {
            return (string) algorithms[oid.Id];
        }

        private class BufferedCipherWrapper
            : IWrapper
        {
            private readonly IBufferedCipher cipher;
            private bool forWrapping;

            public BufferedCipherWrapper(
                IBufferedCipher cipher)
            {
                this.cipher = cipher;
            }

            public string AlgorithmName
            {
                get { return cipher.AlgorithmName; }
            }

            public void Init(
                bool				forWrapping,
                ICipherParameters	parameters)
            {
                this.forWrapping = forWrapping;

                cipher.Init(forWrapping, parameters);
            }

            public byte[] Wrap(
                byte[]	input,
                int		inOff,
                int		length)
            {
                if (!forWrapping)
                    throw new InvalidOperationException("Not initialised for wrapping");

                return cipher.DoFinal(input, inOff, length);
            }

            public byte[] Unwrap(
                byte[]	input,
                int		inOff,
                int		length)
            {
                if (forWrapping)
                    throw new InvalidOperationException("Not initialised for unwrapping");

                return cipher.DoFinal(input, inOff, length);
            }
        }
    }
}
