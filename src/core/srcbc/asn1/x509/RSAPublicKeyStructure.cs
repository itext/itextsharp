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
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

using System;
using System.Collections;

namespace Org.BouncyCastle.Asn1.X509
{
    public class RsaPublicKeyStructure
        : Asn1Encodable
    {
        private BigInteger modulus;
        private BigInteger publicExponent;

		public static RsaPublicKeyStructure GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static RsaPublicKeyStructure GetInstance(
            object obj)
        {
            if (obj == null || obj is RsaPublicKeyStructure)
            {
                return (RsaPublicKeyStructure) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new RsaPublicKeyStructure((Asn1Sequence) obj);
            }

			throw new ArgumentException("Invalid RsaPublicKeyStructure: " + obj.GetType().Name);
        }

		public RsaPublicKeyStructure(
            BigInteger	modulus,
            BigInteger	publicExponent)
        {
			if (modulus == null)
				throw new ArgumentNullException("modulus");
			if (publicExponent == null)
				throw new ArgumentNullException("publicExponent");
			if (modulus.SignValue <= 0)
				throw new ArgumentException("Not a valid RSA modulus", "modulus");
			if (publicExponent.SignValue <= 0)
				throw new ArgumentException("Not a valid RSA public exponent", "publicExponent");

            this.modulus = modulus;
            this.publicExponent = publicExponent;
        }

		private RsaPublicKeyStructure(
            Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			// Note: we are accepting technically incorrect (i.e. negative) values here
			modulus = DerInteger.GetInstance(seq[0]).PositiveValue;
			publicExponent = DerInteger.GetInstance(seq[1]).PositiveValue;
		}

		public BigInteger Modulus
        {
            get { return modulus; }
        }

		public BigInteger PublicExponent
        {
            get { return publicExponent; }
        }

		/**
         * This outputs the key in Pkcs1v2 format.
         * <pre>
         *      RSAPublicKey ::= Sequence {
         *                          modulus Integer, -- n
         *                          publicExponent Integer, -- e
         *                      }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(
				new DerInteger(Modulus),
				new DerInteger(PublicExponent));
        }
    }
}
