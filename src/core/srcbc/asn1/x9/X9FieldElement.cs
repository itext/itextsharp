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

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Asn1.X9
{
    /**
     * Class for processing an ECFieldElement as a DER object.
     */
    public class X9FieldElement
        : Asn1Encodable
    {
		private ECFieldElement f;

		public X9FieldElement(
			ECFieldElement f)
		{
			this.f = f;
		}

		public X9FieldElement(
			BigInteger		p,
			Asn1OctetString	s)
			: this(new FpFieldElement(p, new BigInteger(1, s.GetOctets())))
		{
		}

		public X9FieldElement(
			int				m,
			int				k1,
			int				k2,
			int				k3,
			Asn1OctetString	s)
			: this(new F2mFieldElement(m, k1, k2, k3, new BigInteger(1, s.GetOctets())))
		{
		}

		public ECFieldElement Value
        {
            get { return f; }
        }

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * <pre>
		 *  FieldElement ::= OCTET STRING
		 * </pre>
		 * <p>
		 * <ol>
		 * <li> if <i>q</i> is an odd prime then the field element is
		 * processed as an Integer and converted to an octet string
		 * according to x 9.62 4.3.1.</li>
		 * <li> if <i>q</i> is 2<sup>m</sup> then the bit string
		 * contained in the field element is converted into an octet
		 * string with the same ordering padded at the front if necessary.
		 * </li>
		 * </ol>
		 * </p>
		 */
		public override Asn1Object ToAsn1Object()
		{
			int byteCount = X9IntegerConverter.GetByteLength(f);
			byte[] paddedBigInteger = X9IntegerConverter.IntegerToBytes(f.ToBigInteger(), byteCount);

			return new DerOctetString(paddedBigInteger);
		}
    }
}
