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

namespace Org.BouncyCastle.Asn1.X9
{
	public class DHDomainParameters
		: Asn1Encodable
	{
		private readonly DerInteger p, g, q, j;
		private readonly DHValidationParms validationParms;

		public static DHDomainParameters GetInstance(Asn1TaggedObject obj, bool isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		public static DHDomainParameters GetInstance(object obj)
		{
			if (obj == null || obj is DHDomainParameters)
				return (DHDomainParameters)obj;

			if (obj is Asn1Sequence)
				return new DHDomainParameters((Asn1Sequence)obj);

			throw new ArgumentException("Invalid DHDomainParameters: " + obj.GetType().FullName, "obj");
		}

		public DHDomainParameters(DerInteger p, DerInteger g, DerInteger q, DerInteger j,
			DHValidationParms validationParms)
		{
			if (p == null)
				throw new ArgumentNullException("p");
			if (g == null)
				throw new ArgumentNullException("g");
			if (q == null)
				throw new ArgumentNullException("q");

			this.p = p;
			this.g = g;
			this.q = q;
			this.j = j;
			this.validationParms = validationParms;
		}

		private DHDomainParameters(Asn1Sequence seq)
		{
			if (seq.Count < 3 || seq.Count > 5)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			IEnumerator e = seq.GetEnumerator();
			this.p = DerInteger.GetInstance(GetNext(e));
			this.g = DerInteger.GetInstance(GetNext(e));
			this.q = DerInteger.GetInstance(GetNext(e));

			Asn1Encodable next = GetNext(e);

			if (next != null && next is DerInteger)
			{
				this.j = DerInteger.GetInstance(next);
				next = GetNext(e);
			}

			if (next != null)
			{
				this.validationParms = DHValidationParms.GetInstance(next.ToAsn1Object());
			}
		}

		private static Asn1Encodable GetNext(IEnumerator e)
		{
			return e.MoveNext() ? (Asn1Encodable)e.Current : null;
		}

		public DerInteger P
		{
			get { return this.p; }
		}

		public DerInteger G
		{
			get { return this.g; }
		}

		public DerInteger Q
		{
			get { return this.q; }
		}

		public DerInteger J
		{
			get { return this.j; }
		}

		public DHValidationParms ValidationParms
		{
			get { return this.validationParms; }
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(p, g, q);

			if (this.j != null)
			{
				v.Add(this.j);
			}

			if (this.validationParms != null)
			{
				v.Add(this.validationParms);
			}

			return new DerSequence(v);
		}
	}
}
