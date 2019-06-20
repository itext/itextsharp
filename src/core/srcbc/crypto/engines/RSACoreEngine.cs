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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* this does your basic RSA algorithm.
	*/
	class RsaCoreEngine
	{
		private RsaKeyParameters	key;
		private bool				forEncryption;
		private int					bitSize;

		/**
		* initialise the RSA engine.
		*
		* @param forEncryption true if we are encrypting, false otherwise.
		* @param param the necessary RSA key parameters.
		*/
		public void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom) parameters).Parameters;
			}

			if (!(parameters is RsaKeyParameters))
				throw new InvalidKeyException("Not an RSA key");

			this.key = (RsaKeyParameters) parameters;
			this.forEncryption = forEncryption;
			this.bitSize = key.Modulus.BitLength;
		}

		/**
		* Return the maximum size for an input block to this engine.
		* For RSA this is always one byte less than the key size on
		* encryption, and the same length as the key size on decryption.
		*
		* @return maximum size for an input block.
		*/
		public int GetInputBlockSize()
		{
			if (forEncryption)
			{
				return (bitSize - 1) / 8;
			}

			return (bitSize + 7) / 8;
		}

		/**
		* Return the maximum size for an output block to this engine.
		* For RSA this is always one byte less than the key size on
		* decryption, and the same length as the key size on encryption.
		*
		* @return maximum size for an output block.
		*/
		public int GetOutputBlockSize()
		{
			if (forEncryption)
			{
				return (bitSize + 7) / 8;
			}

			return (bitSize - 1) / 8;
		}

		public BigInteger ConvertInput(
			byte[]	inBuf,
			int		inOff,
			int		inLen)
		{
			int maxLength = (bitSize + 7) / 8;

			if (inLen > maxLength)
				throw new DataLengthException("input too large for RSA cipher.");

			BigInteger input = new BigInteger(1, inBuf, inOff, inLen);

			if (input.CompareTo(key.Modulus) >= 0)
				throw new DataLengthException("input too large for RSA cipher.");

			return input;
		}

		public byte[] ConvertOutput(
			BigInteger result)
		{
			byte[] output = result.ToByteArrayUnsigned();

			if (forEncryption)
			{
				int outSize = GetOutputBlockSize();

				// TODO To avoid this, create version of BigInteger.ToByteArray that
				// writes to an existing array
				if (output.Length < outSize) // have ended up with less bytes than normal, lengthen
				{
					byte[] tmp = new byte[outSize];
					output.CopyTo(tmp, tmp.Length - output.Length);
					output = tmp;
				}
			}

			return output;
		}

		public BigInteger ProcessBlock(
			BigInteger input)
		{
			if (key is RsaPrivateCrtKeyParameters)
			{
				//
				// we have the extra factors, use the Chinese Remainder Theorem - the author
				// wishes to express his thanks to Dirk Bonekaemper at rtsffm.com for
				// advice regarding the expression of this.
				//
				RsaPrivateCrtKeyParameters crtKey = (RsaPrivateCrtKeyParameters)key;

				BigInteger p = crtKey.P;;
				BigInteger q = crtKey.Q;
				BigInteger dP = crtKey.DP;
				BigInteger dQ = crtKey.DQ;
				BigInteger qInv = crtKey.QInv;

				BigInteger mP, mQ, h, m;

				// mP = ((input Mod p) ^ dP)) Mod p
				mP = (input.Remainder(p)).ModPow(dP, p);

				// mQ = ((input Mod q) ^ dQ)) Mod q
				mQ = (input.Remainder(q)).ModPow(dQ, q);

				// h = qInv * (mP - mQ) Mod p
				h = mP.Subtract(mQ);
				h = h.Multiply(qInv);
				h = h.Mod(p);               // Mod (in Java) returns the positive residual

				// m = h * q + mQ
				m = h.Multiply(q);
				m = m.Add(mQ);

				return m;
			}

			return input.ModPow(key.Exponent, key.Modulus);
		}
	}
}
