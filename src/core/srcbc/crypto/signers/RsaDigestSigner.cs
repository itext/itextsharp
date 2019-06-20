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
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Signers
{
    public class RsaDigestSigner
		: ISigner
    {
        private readonly IAsymmetricBlockCipher rsaEngine = new Pkcs1Encoding(new RsaBlindedEngine());
        private readonly AlgorithmIdentifier algId;
		private readonly IDigest digest;
		private bool forSigning;

		private static readonly IDictionary oidMap = Platform.CreateHashtable();

		/// <summary>
        /// Load oid table.
        /// </summary>
        static RsaDigestSigner()
        {
            oidMap["RIPEMD128"] = TeleTrusTObjectIdentifiers.RipeMD128;
            oidMap["RIPEMD160"] = TeleTrusTObjectIdentifiers.RipeMD160;
            oidMap["RIPEMD256"] = TeleTrusTObjectIdentifiers.RipeMD256;

            oidMap["SHA-1"] = X509ObjectIdentifiers.IdSha1;
            oidMap["SHA-224"] = NistObjectIdentifiers.IdSha224;
            oidMap["SHA-256"] = NistObjectIdentifiers.IdSha256;
            oidMap["SHA-384"] = NistObjectIdentifiers.IdSha384;
            oidMap["SHA-512"] = NistObjectIdentifiers.IdSha512;

            oidMap["MD2"] = PkcsObjectIdentifiers.MD2;
            oidMap["MD4"] = PkcsObjectIdentifiers.MD4;
            oidMap["MD5"] = PkcsObjectIdentifiers.MD5;
        }

		public RsaDigestSigner(
			IDigest digest)
        {
            this.digest = digest;

			string algName = digest.AlgorithmName;
			if (algName.Equals("NULL"))
			{
				this.algId = null;
			}
			else
			{
				this.algId = new AlgorithmIdentifier(
					(DerObjectIdentifier)oidMap[digest.AlgorithmName], DerNull.Instance);
			}
        }

		public string AlgorithmName
        {
            get { return digest.AlgorithmName + "withRSA"; }
        }

		/**
         * Initialise the signer for signing or verification.
         *
         * @param forSigning true if for signing, false otherwise
         * @param param necessary parameters.
         */
        public void Init(
			bool				forSigning,
			ICipherParameters	parameters)
        {
            this.forSigning = forSigning;
            AsymmetricKeyParameter k;

            if (parameters is ParametersWithRandom)
            {
                k = (AsymmetricKeyParameter)((ParametersWithRandom)parameters).Parameters;
            }
            else
            {
                k = (AsymmetricKeyParameter)parameters;
            }

            if (forSigning && !k.IsPrivate)
                throw new InvalidKeyException("Signing requires private key.");

			if (!forSigning && k.IsPrivate)
                throw new InvalidKeyException("Verification requires public key.");

			Reset();

            rsaEngine.Init(forSigning, parameters);
        }

        /**
         * update the internal digest with the byte b
         */
        public void Update(
			byte input)
        {
            digest.Update(input);
        }

        /**
         * update the internal digest with the byte array in
         */
        public void BlockUpdate(
			byte[]	input,
			int		inOff,
			int		length)
        {
            digest.BlockUpdate(input, inOff, length);
        }

        /**
         * Generate a signature for the message we've been loaded with using
         * the key we were initialised with.
         */
        public byte[] GenerateSignature()
        {
            if (!forSigning)
                throw new InvalidOperationException("RsaDigestSigner not initialised for signature generation.");

			byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

			byte[] data = DerEncode(hash);
            return rsaEngine.ProcessBlock(data, 0, data.Length);
        }

		/**
         * return true if the internal state represents the signature described
         * in the passed in array.
         */
        public bool VerifySignature(
			byte[] signature)
        {
			if (forSigning)
				throw new InvalidOperationException("RsaDigestSigner not initialised for verification");

			byte[] hash = new byte[digest.GetDigestSize()];
			digest.DoFinal(hash, 0);

			byte[] sig;
			byte[] expected;

			try
			{
				sig = rsaEngine.ProcessBlock(signature, 0, signature.Length);
				expected = DerEncode(hash);
			}
			catch (Exception)
			{
				return false;
			}

			if (sig.Length == expected.Length)
			{
				for (int i = 0; i < sig.Length; i++)
				{
					if (sig[i] != expected[i])
					{
						return false;
					}
				}
			}
			else if (sig.Length == expected.Length - 2)  // NULL left out
			{
				int sigOffset = sig.Length - hash.Length - 2;
				int expectedOffset = expected.Length - hash.Length - 2;

				expected[1] -= 2;      // adjust lengths
				expected[3] -= 2;

				for (int i = 0; i < hash.Length; i++)
				{
					if (sig[sigOffset + i] != expected[expectedOffset + i])  // check hash
					{
						return false;
					}
				}

				for (int i = 0; i < sigOffset; i++)
				{
					if (sig[i] != expected[i])  // check header less NULL
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}

			return true;
        }

        public void Reset()
        {
            digest.Reset();
        }

		private byte[] DerEncode(byte[] hash)
		{
			if (algId == null)
			{
				// For raw RSA, the DigestInfo must be prepared externally
				return hash;
			}

			DigestInfo dInfo = new DigestInfo(algId, hash);

			return dInfo.GetDerEncoded();
		}
    }
}
