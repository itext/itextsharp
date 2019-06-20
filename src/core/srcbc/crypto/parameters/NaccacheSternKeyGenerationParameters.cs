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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
	/**
	 * Parameters for NaccacheStern public private key generation. For details on
	 * this cipher, please see
	 *
	 * http://www.gemplus.com/smart/rd/publications/pdf/NS98pkcs.pdf
	 */
	public class NaccacheSternKeyGenerationParameters : KeyGenerationParameters
	{
		// private BigInteger publicExponent;
		private readonly int certainty;
		private readonly int countSmallPrimes;
		private bool debug;

		/**
		 * Parameters for generating a NaccacheStern KeyPair.
		 *
		 * @param random
		 *            The source of randomness
		 * @param strength
		 *            The desired strength of the Key in Bits
		 * @param certainty
		 *            the probability that the generated primes are not really prime
		 *            as integer: 2^(-certainty) is then the probability
		 * @param countSmallPrimes
		 *            How many small key factors are desired
		 */
		public NaccacheSternKeyGenerationParameters(
			SecureRandom	random,
			int				strength,
			int				certainty,
			int				countSmallPrimes)
			: this(random, strength, certainty, countSmallPrimes, false)
		{
		}

		/**
		 * Parameters for a NaccacheStern KeyPair.
		 *
		 * @param random
		 *            The source of randomness
		 * @param strength
		 *            The desired strength of the Key in Bits
		 * @param certainty
		 *            the probability that the generated primes are not really prime
		 *            as integer: 2^(-certainty) is then the probability
		 * @param cntSmallPrimes
		 *            How many small key factors are desired
		 * @param debug
		 *            Turn debugging on or off (reveals secret information, use with
		 *            caution)
		 */
		public NaccacheSternKeyGenerationParameters(SecureRandom random,
			int		strength,
			int		certainty,
			int		countSmallPrimes,
			bool	debug)
			: base(random, strength)
		{
			if (countSmallPrimes % 2 == 1)
			{
				throw new ArgumentException("countSmallPrimes must be a multiple of 2");
			}
			if (countSmallPrimes < 30)
			{
				throw new ArgumentException("countSmallPrimes must be >= 30 for security reasons");
			}
			this.certainty = certainty;
			this.countSmallPrimes = countSmallPrimes;
			this.debug = debug;
		}

		/**
		 * @return Returns the certainty.
		 */
		public int Certainty
		{
			get { return certainty; }
		}

		/**
		 * @return Returns the countSmallPrimes.
		 */
		public int CountSmallPrimes
		{
			get { return countSmallPrimes; }
		}

		public bool IsDebug
		{
			get { return debug; }
		}
	}
}
