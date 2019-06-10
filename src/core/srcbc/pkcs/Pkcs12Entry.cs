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
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Pkcs
{
    public abstract class Pkcs12Entry
    {
        private readonly IDictionary attributes;

		protected internal Pkcs12Entry(
            IDictionary attributes)
        {
            this.attributes = attributes;

			foreach (DictionaryEntry entry in attributes)
			{
				if (!(entry.Key is string))
					throw new ArgumentException("Attribute keys must be of type: " + typeof(string).FullName, "attributes");
				if (!(entry.Value is Asn1Encodable))
					throw new ArgumentException("Attribute values must be of type: " + typeof(Asn1Encodable).FullName, "attributes");
			}
        }

		[Obsolete("Use 'object[index]' syntax instead")]
		public Asn1Encodable GetBagAttribute(
            DerObjectIdentifier oid)
        {
            return (Asn1Encodable)this.attributes[oid.Id];
        }

		[Obsolete("Use 'object[index]' syntax instead")]
		public Asn1Encodable GetBagAttribute(
            string oid)
        {
            return (Asn1Encodable)this.attributes[oid];
        }

		[Obsolete("Use 'BagAttributeKeys' property")]
        public IEnumerator GetBagAttributeKeys()
        {
            return this.attributes.Keys.GetEnumerator();
        }

		public Asn1Encodable this[
			DerObjectIdentifier oid]
		{
			get { return (Asn1Encodable) this.attributes[oid.Id]; }
		}

		public Asn1Encodable this[
			string oid]
		{
			get { return (Asn1Encodable) this.attributes[oid]; }
		}

		public IEnumerable BagAttributeKeys
		{
			get { return new EnumerableProxy(this.attributes.Keys); }
		}
    }
}
