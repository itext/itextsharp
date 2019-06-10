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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.X509
{
	/**
	 * Class for carrying the values in an X.509 Attribute.
	 */
	public class X509Attribute
		: Asn1Encodable
	{
		private readonly AttributeX509 attr;

		/**
		 * @param at an object representing an attribute.
		 */
		internal X509Attribute(
			Asn1Encodable at)
		{
			this.attr = AttributeX509.GetInstance(at);
		}

		/**
		 * Create an X.509 Attribute with the type given by the passed in oid and
		 * the value represented by an ASN.1 Set containing value.
		 *
		 * @param oid type of the attribute
		 * @param value value object to go into the atribute's value set.
		 */
		public X509Attribute(
			string			oid,
			Asn1Encodable	value)
		{
			this.attr = new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value));
		}

		/**
		 * Create an X.59 Attribute with the type given by the passed in oid and the
		 * value represented by an ASN.1 Set containing the objects in value.
		 *
		 * @param oid type of the attribute
		 * @param value vector of values to go in the attribute's value set.
		 */
		public X509Attribute(
			string              oid,
			Asn1EncodableVector value)
		{
			this.attr = new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value));
		}

		public string Oid
		{
			get { return attr.AttrType.Id; }
		}

		public Asn1Encodable[] GetValues()
		{
			Asn1Set s = attr.AttrValues;
			Asn1Encodable[] values = new Asn1Encodable[s.Count];

			for (int i = 0; i != s.Count; i++)
			{
				values[i] = (Asn1Encodable)s[i];
			}

			return values;
		}

		public override Asn1Object ToAsn1Object()
		{
			return attr.ToAsn1Object();
		}
	}
}
