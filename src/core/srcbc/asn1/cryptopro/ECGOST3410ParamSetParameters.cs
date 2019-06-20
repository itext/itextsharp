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

namespace Org.BouncyCastle.Asn1.CryptoPro
{
    public class ECGost3410ParamSetParameters
        : Asn1Encodable
    {
        internal readonly DerInteger p, q, a, b, x, y;

        public static ECGost3410ParamSetParameters GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static ECGost3410ParamSetParameters GetInstance(
            object obj)
        {
            if (obj == null || obj is ECGost3410ParamSetParameters)
            {
                return (ECGost3410ParamSetParameters) obj;
            }

            if (obj is Asn1Sequence)
            {
                return new ECGost3410ParamSetParameters((Asn1Sequence) obj);
            }

            throw new ArgumentException("Invalid GOST3410Parameter: " + obj.GetType().Name);
        }

        public ECGost3410ParamSetParameters(
            BigInteger a,
            BigInteger b,
            BigInteger p,
            BigInteger q,
            int        x,
            BigInteger y)
        {
            this.a = new DerInteger(a);
            this.b = new DerInteger(b);
            this.p = new DerInteger(p);
            this.q = new DerInteger(q);
            this.x = new DerInteger(x);
            this.y = new DerInteger(y);
        }

        public ECGost3410ParamSetParameters(
            Asn1Sequence seq)
        {
			if (seq.Count != 6)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			this.a = DerInteger.GetInstance(seq[0]);
			this.b = DerInteger.GetInstance(seq[1]);
			this.p = DerInteger.GetInstance(seq[2]);
			this.q = DerInteger.GetInstance(seq[3]);
			this.x = DerInteger.GetInstance(seq[4]);
			this.y = DerInteger.GetInstance(seq[5]);
        }

		public BigInteger P
		{
			get { return p.PositiveValue; }
		}

		public BigInteger Q
		{
			get { return q.PositiveValue; }
		}

		public BigInteger A
		{
			get { return a.PositiveValue; }
		}

		public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(a, b, p, q, x, y);
        }
    }
}
