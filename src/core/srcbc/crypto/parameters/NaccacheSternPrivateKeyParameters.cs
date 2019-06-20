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

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Parameters
{
	/**
	 * Private key parameters for NaccacheStern cipher. For details on this cipher,
	 * please see
	 *
	 * http://www.gemplus.com/smart/rd/publications/pdf/NS98pkcs.pdf
	 */
	public class NaccacheSternPrivateKeyParameters : NaccacheSternKeyParameters
	{
		private readonly BigInteger phiN;
		private readonly IList smallPrimes;

#if !SILVERLIGHT
        [Obsolete]
        public NaccacheSternPrivateKeyParameters(
            BigInteger g,
            BigInteger n,
            int lowerSigmaBound,
            ArrayList smallPrimes,
            BigInteger phiN)
            : base(true, g, n, lowerSigmaBound)
        {
            this.smallPrimes = smallPrimes;
            this.phiN = phiN;
        }
#endif

		/**
		 * Constructs a NaccacheSternPrivateKey
		 *
		 * @param g
		 *            the public enryption parameter g
		 * @param n
		 *            the public modulus n = p*q
		 * @param lowerSigmaBound
		 *            the public lower sigma bound up to which data can be encrypted
		 * @param smallPrimes
		 *            the small primes, of which sigma is constructed in the right
		 *            order
		 * @param phi_n
		 *            the private modulus phi(n) = (p-1)(q-1)
		 */
		public NaccacheSternPrivateKeyParameters(
			BigInteger	g,
			BigInteger	n,
			int			lowerSigmaBound,
			IList       smallPrimes,
			BigInteger	phiN)
			: base(true, g, n, lowerSigmaBound)
		{
			this.smallPrimes = smallPrimes;
			this.phiN = phiN;
		}

		public BigInteger PhiN
		{
			get { return phiN; }
		}

#if !SILVERLIGHT
        [Obsolete("Use 'SmallPrimesList' instead")]
        public ArrayList SmallPrimes
		{
			get { return new ArrayList(smallPrimes); }
		}
#endif

        public IList SmallPrimesList
        {
            get { return smallPrimes; }
        }
    }
}
