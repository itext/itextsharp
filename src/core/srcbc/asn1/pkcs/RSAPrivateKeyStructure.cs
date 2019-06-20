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
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Pkcs
{
    public class RsaPrivateKeyStructure
        : Asn1Encodable
    {
        private readonly BigInteger	modulus;
        private readonly BigInteger	publicExponent;
        private readonly BigInteger	privateExponent;
        private readonly BigInteger	prime1;
        private readonly BigInteger	prime2;
        private readonly BigInteger	exponent1;
        private readonly BigInteger	exponent2;
        private readonly BigInteger	coefficient;

		public RsaPrivateKeyStructure(
            BigInteger	modulus,
            BigInteger	publicExponent,
            BigInteger	privateExponent,
            BigInteger	prime1,
            BigInteger	prime2,
            BigInteger	exponent1,
            BigInteger	exponent2,
            BigInteger	coefficient)
        {
            this.modulus = modulus;
            this.publicExponent = publicExponent;
            this.privateExponent = privateExponent;
            this.prime1 = prime1;
            this.prime2 = prime2;
            this.exponent1 = exponent1;
            this.exponent2 = exponent2;
            this.coefficient = coefficient;
        }

		public RsaPrivateKeyStructure(
            Asn1Sequence seq)
        {
			BigInteger version = ((DerInteger) seq[0]).Value;
			if (version.IntValue != 0)
                throw new ArgumentException("wrong version for RSA private key");

			modulus = ((DerInteger) seq[1]).Value;
			publicExponent = ((DerInteger) seq[2]).Value;
			privateExponent = ((DerInteger) seq[3]).Value;
			prime1 = ((DerInteger) seq[4]).Value;
			prime2 = ((DerInteger) seq[5]).Value;
			exponent1 = ((DerInteger) seq[6]).Value;
			exponent2 = ((DerInteger) seq[7]).Value;
			coefficient = ((DerInteger) seq[8]).Value;
		}

		public BigInteger Modulus
		{
			get { return modulus; }
		}

		public BigInteger PublicExponent
		{
			get { return publicExponent; }
		}

		public BigInteger PrivateExponent
		{
			get { return privateExponent; }
		}

		public BigInteger Prime1
		{
			get { return prime1; }
		}

		public BigInteger Prime2
		{
			get { return prime2; }
		}

		public BigInteger Exponent1
		{
			get { return exponent1; }
		}

		public BigInteger Exponent2
		{
			get { return exponent2; }
		}

		public BigInteger Coefficient
		{
			get { return coefficient; }
		}

		/**
         * This outputs the key in Pkcs1v2 format.
         * <pre>
         *      RsaPrivateKey ::= Sequence {
         *                          version Version,
         *                          modulus Integer, -- n
         *                          publicExponent Integer, -- e
         *                          privateExponent Integer, -- d
         *                          prime1 Integer, -- p
         *                          prime2 Integer, -- q
         *                          exponent1 Integer, -- d mod (p-1)
         *                          exponent2 Integer, -- d mod (q-1)
         *                          coefficient Integer -- (inverse of q) mod p
         *                      }
         *
         *      Version ::= Integer
         * </pre>
         * <p>This routine is written to output Pkcs1 version 0, private keys.</p>
         */
        public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(
				new DerInteger(0), // version
				new DerInteger(Modulus),
				new DerInteger(PublicExponent),
				new DerInteger(PrivateExponent),
				new DerInteger(Prime1),
				new DerInteger(Prime2),
				new DerInteger(Exponent1),
				new DerInteger(Exponent2),
				new DerInteger(Coefficient));
        }
    }
}
