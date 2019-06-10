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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators
{
	/**
	* Generator for Pbe derived keys and ivs as defined by Pkcs 5 V2.0 Scheme 1.
	* Note this generator is limited to the size of the hash produced by the
	* digest used to drive it.
	* <p>
	* The document this implementation is based on can be found at
	* <a href="http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html">
	* RSA's Pkcs5 Page</a>
	* </p>
	*/
	public class Pkcs5S1ParametersGenerator
		: PbeParametersGenerator
	{
		private readonly IDigest digest;

		/**
		* Construct a Pkcs 5 Scheme 1 Parameters generator.
		*
		* @param digest the digest to be used as the source of derived keys.
		*/
		public Pkcs5S1ParametersGenerator(
			IDigest digest)
		{
			this.digest = digest;
		}

		/**
		* the derived key function, the ith hash of the mPassword and the mSalt.
		*/
		private byte[] GenerateDerivedKey()
		{
			byte[] digestBytes = new byte[digest.GetDigestSize()];

			digest.BlockUpdate(mPassword, 0, mPassword.Length);
			digest.BlockUpdate(mSalt, 0, mSalt.Length);

			digest.DoFinal(digestBytes, 0);
			for (int i = 1; i < mIterationCount; i++)
			{
				digest.BlockUpdate(digestBytes, 0, digestBytes.Length);
				digest.DoFinal(digestBytes, 0);
			}

			return digestBytes;
		}

		/**
		* Generate a key parameter derived from the mPassword, mSalt, and iteration
		* count we are currently initialised with.
		*
		* @param keySize the size of the key we want (in bits)
		* @return a KeyParameter object.
		* @exception ArgumentException if the key length larger than the base hash size.
		*/
		[Obsolete("Use version with 'algorithm' parameter")]
		public override ICipherParameters GenerateDerivedParameters(
			int keySize)
		{
			return GenerateDerivedMacParameters(keySize);
		}

		public override ICipherParameters GenerateDerivedParameters(
			string	algorithm,
			int		keySize)
		{
			keySize /= 8;

			if (keySize > digest.GetDigestSize())
			{
				throw new ArgumentException(
					"Can't Generate a derived key " + keySize + " bytes long.");
			}

			byte[] dKey = GenerateDerivedKey();

			return ParameterUtilities.CreateKeyParameter(algorithm, dKey, 0, keySize);
		}

		/**
		* Generate a key with initialisation vector parameter derived from
		* the mPassword, mSalt, and iteration count we are currently initialised
		* with.
		*
		* @param keySize the size of the key we want (in bits)
		* @param ivSize the size of the iv we want (in bits)
		* @return a ParametersWithIV object.
		* @exception ArgumentException if keySize + ivSize is larger than the base hash size.
		*/
		[Obsolete("Use version with 'algorithm' parameter")]
		public override ICipherParameters GenerateDerivedParameters(
			int	keySize,
			int	ivSize)
		{
			keySize /= 8;
			ivSize /= 8;

			if ((keySize + ivSize) > digest.GetDigestSize())
			{
				throw new ArgumentException(
					"Can't Generate a derived key " + (keySize + ivSize) + " bytes long.");
			}

			byte[] dKey = GenerateDerivedKey();

			return new ParametersWithIV(new KeyParameter(dKey, 0, keySize), dKey, keySize, ivSize);
		}

		public override ICipherParameters GenerateDerivedParameters(
			string	algorithm,
			int		keySize,
			int		ivSize)
		{
			keySize /= 8;
			ivSize /= 8;

			if ((keySize + ivSize) > digest.GetDigestSize())
			{
				throw new ArgumentException(
					"Can't Generate a derived key " + (keySize + ivSize) + " bytes long.");
			}

			byte[] dKey = GenerateDerivedKey();
			KeyParameter key = ParameterUtilities.CreateKeyParameter(algorithm, dKey, 0, keySize);

			return new ParametersWithIV(key, dKey, keySize, ivSize);
		}

		/**
		* Generate a key parameter for use with a MAC derived from the mPassword,
		* mSalt, and iteration count we are currently initialised with.
		*
		* @param keySize the size of the key we want (in bits)
		* @return a KeyParameter object.
		* @exception ArgumentException if the key length larger than the base hash size.
		*/
		public override ICipherParameters GenerateDerivedMacParameters(
			int keySize)
		{
			keySize /= 8;

			if (keySize > digest.GetDigestSize())
			{
				throw new ArgumentException(
					"Can't Generate a derived key " + keySize + " bytes long.");
			}

			byte[] dKey = GenerateDerivedKey();

			return new KeyParameter(dKey, 0, keySize);
		}
	}
}
