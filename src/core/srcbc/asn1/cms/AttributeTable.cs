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
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class AttributeTable
    {
        private readonly IDictionary attributes;

#if !SILVERLIGHT
        [Obsolete]
        public AttributeTable(
            Hashtable attrs)
        {
            this.attributes = Platform.CreateHashtable(attrs);
        }
#endif

        public AttributeTable(
            IDictionary attrs)
        {
            this.attributes = Platform.CreateHashtable(attrs);
        }

        public AttributeTable(
            Asn1EncodableVector v)
        {
            this.attributes = Platform.CreateHashtable(v.Count);

			foreach (Asn1Encodable o in v)
            {
                Attribute a = Attribute.GetInstance(o);

				AddAttribute(a);
            }
        }

        public AttributeTable(
            Asn1Set s)
        {
            this.attributes = Platform.CreateHashtable(s.Count);

			for (int i = 0; i != s.Count; i++)
            {
                Attribute a = Attribute.GetInstance(s[i]);

                AddAttribute(a);
            }
        }

		public AttributeTable(
			Attributes attrs)
			: this(Asn1Set.GetInstance(attrs.ToAsn1Object()))
		{
		}

		private void AddAttribute(
            Attribute a)
        {
			DerObjectIdentifier oid = a.AttrType;
            object obj = attributes[oid];

            if (obj == null)
            {
                attributes[oid] = a;
            }
            else
            {
                IList v;

                if (obj is Attribute)
                {
                    v = Platform.CreateArrayList();

                    v.Add(obj);
                    v.Add(a);
                }
                else
                {
                    v = (IList) obj;

                    v.Add(a);
                }

                attributes[oid] = v;
            }
        }

		/// <summary>Return the first attribute matching the given OBJECT IDENTIFIER</summary>
		public Attribute this[DerObjectIdentifier oid]
		{
			get
			{
				object obj = attributes[oid];

				if (obj is IList)
				{
					return (Attribute)((IList)obj)[0];
				}

				return (Attribute) obj;
			}
		}

		[Obsolete("Use 'object[oid]' syntax instead")]
        public Attribute Get(
            DerObjectIdentifier oid)
        {
			return this[oid];
        }

		/**
        * Return all the attributes matching the OBJECT IDENTIFIER oid. The vector will be
        * empty if there are no attributes of the required type present.
        *
        * @param oid type of attribute required.
        * @return a vector of all the attributes found of type oid.
        */
        public Asn1EncodableVector GetAll(
            DerObjectIdentifier oid)
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            object obj = attributes[oid];

			if (obj is IList)
            {
                foreach (Attribute a in (IList)obj)
                {
                    v.Add(a);
                }
            }
            else if (obj != null)
            {
                v.Add((Attribute) obj);
            }

			return v;
        }

		public int Count
		{
			get
			{
				int total = 0;

				foreach (object o in attributes.Values)
				{
					if (o is IList)
					{
						total += ((IList)o).Count;
					}
					else
					{
						++total;
					}
				}

				return total;
			}
		}

        public IDictionary ToDictionary()
        {
            return Platform.CreateHashtable(attributes);
        }

#if !SILVERLIGHT
        [Obsolete("Use 'ToDictionary' instead")]
		public Hashtable ToHashtable()
        {
            return new Hashtable(attributes);
        }
#endif

		public Asn1EncodableVector ToAsn1EncodableVector()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			foreach (object obj in attributes.Values)
            {
                if (obj is IList)
                {
                    foreach (object el in (IList)obj)
                    {
                        v.Add(Attribute.GetInstance(el));
                    }
                }
                else
                {
                    v.Add(Attribute.GetInstance(obj));
                }
            }

			return v;
        }

		public Attributes ToAttributes()
		{
			return new Attributes(this.ToAsn1EncodableVector());
		}

		/**
		 * Return a new table with the passed in attribute added.
		 *
		 * @param attrType
		 * @param attrValue
		 * @return
		 */
		public AttributeTable Add(DerObjectIdentifier attrType, Asn1Encodable attrValue)
		{
			AttributeTable newTable = new AttributeTable(attributes);

			newTable.AddAttribute(new Attribute(attrType, new DerSet(attrValue)));

			return newTable;
		}

		public AttributeTable Remove(DerObjectIdentifier attrType)
		{
			AttributeTable newTable = new AttributeTable(attributes);

			newTable.attributes.Remove(attrType);

			return newTable;
		}
    }
}
