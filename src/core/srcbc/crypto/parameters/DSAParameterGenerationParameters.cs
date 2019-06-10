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

using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DsaParameterGenerationParameters
    {
        public const int DigitalSignatureUsage = 1;
        public const int KeyEstablishmentUsage = 2;

        private readonly int l;
        private readonly int n;
        private readonly int certainty;
        private readonly SecureRandom random;
        private readonly int usageIndex;

        /**
         * Construct without a usage index, this will do a random construction of G.
         *
         * @param L desired length of prime P in bits (the effective key size).
         * @param N desired length of prime Q in bits.
         * @param certainty certainty level for prime number generation.
         * @param random the source of randomness to use.
         */
        public DsaParameterGenerationParameters(int L, int N, int certainty, SecureRandom random)
            : this(L, N, certainty, random, -1)
        {
        }

        /**
         * Construct for a specific usage index - this has the effect of using verifiable canonical generation of G.
         *
         * @param L desired length of prime P in bits (the effective key size).
         * @param N desired length of prime Q in bits.
         * @param certainty certainty level for prime number generation.
         * @param random the source of randomness to use.
         * @param usageIndex a valid usage index.
         */
        public DsaParameterGenerationParameters(int L, int N, int certainty, SecureRandom random, int usageIndex)
        {
            this.l = L;
            this.n = N;
            this.certainty = certainty;
            this.random = random;
            this.usageIndex = usageIndex;
        }

        public virtual int L
        {
            get { return l; }
        }

        public virtual int N
        {
            get { return n; }
        }

        public virtual int UsageIndex
        {
            get { return usageIndex; }
        }

        public virtual int Certainty
        {
            get { return certainty; }
        }

        public virtual SecureRandom Random
        {
            get { return random; }
        }
    }
}
