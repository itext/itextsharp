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
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Signers
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class Gost3410DigestSigner
		: ISigner
	{
		private readonly IDigest digest;
		private readonly IDsa dsaSigner;
		private bool forSigning;

		public Gost3410DigestSigner(
			IDsa	signer,
			IDigest	digest)
		{
			this.dsaSigner = signer;
			this.digest = digest;
		}

		public string AlgorithmName
		{
			get { return digest.AlgorithmName + "with" + dsaSigner.AlgorithmName; }
		}

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
			{
				throw new InvalidKeyException("Signing Requires Private Key.");
			}

			if (!forSigning && k.IsPrivate)
			{
				throw new InvalidKeyException("Verification Requires Public Key.");
			}

			Reset();

			dsaSigner.Init(forSigning, parameters);
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
				throw new InvalidOperationException("GOST3410DigestSigner not initialised for signature generation.");

			byte[] hash = new byte[digest.GetDigestSize()];
			digest.DoFinal(hash, 0);

			try
			{
				BigInteger[] sig = dsaSigner.GenerateSignature(hash);
				byte[] sigBytes = new byte[64];

				// TODO Add methods to allow writing BigInteger to existing byte array?
				byte[] r = sig[0].ToByteArrayUnsigned();
				byte[] s = sig[1].ToByteArrayUnsigned();
				s.CopyTo(sigBytes, 32 - s.Length);
				r.CopyTo(sigBytes, 64 - r.Length);
				return sigBytes;
			}
			catch (Exception e)
			{
				throw new SignatureException(e.Message, e);
			}
		}

		/// <returns>true if the internal state represents the signature described in the passed in array.</returns>
		public bool VerifySignature(
			byte[] signature)
		{
			if (forSigning)
				throw new InvalidOperationException("DSADigestSigner not initialised for verification");

			byte[] hash = new byte[digest.GetDigestSize()];
			digest.DoFinal(hash, 0);

			BigInteger R, S;
			try
			{
				R = new BigInteger(1, signature, 32, 32);
				S = new BigInteger(1, signature, 0, 32);
			}
			catch (Exception e)
			{
				throw new SignatureException("error decoding signature bytes.", e);
			}

			return dsaSigner.VerifySignature(hash, R, S);
		}

		/// <summary>Reset the internal state</summary>
		public void Reset()
		{
			digest.Reset();
		}
	}
}
