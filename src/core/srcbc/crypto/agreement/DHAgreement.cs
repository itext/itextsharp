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
using System.Diagnostics;

using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement
{
	/**
	 * a Diffie-Hellman key exchange engine.
	 * <p>
	 * note: This uses MTI/A0 key agreement in order to make the key agreement
	 * secure against passive attacks. If you're doing Diffie-Hellman and both
	 * parties have long term public keys you should look at using this. For
	 * further information have a look at RFC 2631.</p>
	 * <p>
	 * It's possible to extend this to more than two parties as well, for the moment
	 * that is left as an exercise for the reader.</p>
	 */
	public class DHAgreement
	{
		private DHPrivateKeyParameters  key;
		private DHParameters			dhParams;
		private BigInteger				privateValue;
		private SecureRandom			random;

		public void Init(
			ICipherParameters parameters)
		{
			AsymmetricKeyParameter kParam;
			if (parameters is ParametersWithRandom)
			{
				ParametersWithRandom rParam = (ParametersWithRandom)parameters;

				this.random = rParam.Random;
				kParam = (AsymmetricKeyParameter)rParam.Parameters;
			}
			else
			{
				this.random = new SecureRandom();
				kParam = (AsymmetricKeyParameter)parameters;
			}

			if (!(kParam is DHPrivateKeyParameters))
			{
				throw new ArgumentException("DHEngine expects DHPrivateKeyParameters");
			}

			this.key = (DHPrivateKeyParameters)kParam;
			this.dhParams = key.Parameters;
		}

		/**
		 * calculate our initial message.
		 */
		public BigInteger CalculateMessage()
		{
			DHKeyPairGenerator dhGen = new DHKeyPairGenerator();
			dhGen.Init(new DHKeyGenerationParameters(random, dhParams));
			AsymmetricCipherKeyPair dhPair = dhGen.GenerateKeyPair();

			this.privateValue = ((DHPrivateKeyParameters)dhPair.Private).X;

			return ((DHPublicKeyParameters)dhPair.Public).Y;
		}

		/**
		 * given a message from a given party and the corresponding public key
		 * calculate the next message in the agreement sequence. In this case
		 * this will represent the shared secret.
		 */
		public BigInteger CalculateAgreement(
			DHPublicKeyParameters	pub,
			BigInteger				message)
		{
			if (pub == null)
				throw new ArgumentNullException("pub");
			if (message == null)
				throw new ArgumentNullException("message");

			if (!pub.Parameters.Equals(dhParams))
			{
				throw new ArgumentException("Diffie-Hellman public key has wrong parameters.");
			}

			BigInteger p = dhParams.P;

			return message.ModPow(key.X, p).Multiply(pub.Y.ModPow(privateValue, p)).Mod(p);
		}
	}
}
