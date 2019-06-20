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

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* This does your basic RSA Chaum's blinding and unblinding as outlined in
	* "Handbook of Applied Cryptography", page 475. You need to use this if you are
	* trying to get another party to generate signatures without them being aware
	* of the message they are signing.
	*/
	public class RsaBlindingEngine
		: IAsymmetricBlockCipher
	{
		private readonly RsaCoreEngine core = new RsaCoreEngine();

		private RsaKeyParameters key;
		private BigInteger blindingFactor;

		private bool forEncryption;

		public string AlgorithmName
		{
			get { return "RSA"; }
		}

		/**
		* Initialise the blinding engine.
		*
		* @param forEncryption true if we are encrypting (blinding), false otherwise.
		* @param param         the necessary RSA key parameters.
		*/
		public void Init(
			bool				forEncryption,
			ICipherParameters	param)
		{
			RsaBlindingParameters p;

			if (param is ParametersWithRandom)
			{
				ParametersWithRandom rParam = (ParametersWithRandom)param;

				p = (RsaBlindingParameters)rParam.Parameters;
			}
			else
			{
				p = (RsaBlindingParameters)param;
			}

			core.Init(forEncryption, p.PublicKey);

			this.forEncryption = forEncryption;
			this.key = p.PublicKey;
			this.blindingFactor = p.BlindingFactor;
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
			return core.GetInputBlockSize();
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
			return core.GetOutputBlockSize();
		}

		/**
		* Process a single block using the RSA blinding algorithm.
		*
		* @param in    the input array.
		* @param inOff the offset into the input buffer where the data starts.
		* @param inLen the length of the data to be processed.
		* @return the result of the RSA process.
		* @throws DataLengthException the input block is too large.
		*/
		public byte[] ProcessBlock(
			byte[]	inBuf,
			int		inOff,
			int		inLen)
		{
			BigInteger msg = core.ConvertInput(inBuf, inOff, inLen);

			if (forEncryption)
			{
				msg = BlindMessage(msg);
			}
			else
			{
				msg = UnblindMessage(msg);
			}

			return core.ConvertOutput(msg);
		}

		/*
		* Blind message with the blind factor.
		*/
		private BigInteger BlindMessage(
			BigInteger msg)
		{
			BigInteger blindMsg = blindingFactor;
			blindMsg = msg.Multiply(blindMsg.ModPow(key.Exponent, key.Modulus));
			blindMsg = blindMsg.Mod(key.Modulus);

			return blindMsg;
		}

		/*
		* Unblind the message blinded with the blind factor.
		*/
		private BigInteger UnblindMessage(
			BigInteger blindedMsg)
		{
			BigInteger m = key.Modulus;
			BigInteger msg = blindedMsg;
			BigInteger blindFactorInverse = blindingFactor.ModInverse(m);
			msg = msg.Multiply(blindFactorInverse);
			msg = msg.Mod(m);

			return msg;
		}
	}
}
