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
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms
{
	/**
	 * Default signed attributes generator.
	 */
	public class DefaultSignedAttributeTableGenerator
		: CmsAttributeTableGenerator
	{
		private readonly IDictionary table;

		/**
		 * Initialise to use all defaults
		 */
		public DefaultSignedAttributeTableGenerator()
		{
			table = Platform.CreateHashtable();
		}

		/**
		 * Initialise with some extra attributes or overrides.
		 *
		 * @param attributeTable initial attribute table to use.
		 */
		public DefaultSignedAttributeTableGenerator(
			AttributeTable attributeTable)
		{
			if (attributeTable != null)
			{
				table = attributeTable.ToDictionary();
			}
			else
			{
				table = Platform.CreateHashtable();
			}
		}

#if SILVERLIGHT
		/**
		 * Create a standard attribute table from the passed in parameters - this will
		 * normally include contentType, signingTime, and messageDigest. If the constructor
		 * using an AttributeTable was used, entries in it for contentType, signingTime, and
		 * messageDigest will override the generated ones.
		 *
		 * @param parameters source parameters for table generation.
		 *
		 * @return a filled in Hashtable of attributes.
		 */
		protected virtual IDictionary createStandardAttributeTable(
			IDictionary parameters)
		{
            IDictionary std = Platform.CreateHashtable(table);
            DoCreateStandardAttributeTable(parameters, std);
            return std;
		}
#else
        /**
		 * Create a standard attribute table from the passed in parameters - this will
		 * normally include contentType, signingTime, and messageDigest. If the constructor
		 * using an AttributeTable was used, entries in it for contentType, signingTime, and
		 * messageDigest will override the generated ones.
		 *
		 * @param parameters source parameters for table generation.
		 *
		 * @return a filled in Hashtable of attributes.
		 */
		protected virtual Hashtable createStandardAttributeTable(
			IDictionary parameters)
		{
            Hashtable std = new Hashtable(table);
            DoCreateStandardAttributeTable(parameters, std);
			return std;
		}
#endif

        private void DoCreateStandardAttributeTable(IDictionary parameters, IDictionary std)
        {
            // contentType will be absent if we're trying to generate a counter signature.
            if (parameters.Contains(CmsAttributeTableParameter.ContentType))
            {
                if (!std.Contains(CmsAttributes.ContentType))
                {
                    DerObjectIdentifier contentType = (DerObjectIdentifier)
                        parameters[CmsAttributeTableParameter.ContentType];
                    Asn1.Cms.Attribute attr = new Asn1.Cms.Attribute(CmsAttributes.ContentType,
                        new DerSet(contentType));
                    std[attr.AttrType] = attr;
                }
            }

            if (!std.Contains(CmsAttributes.SigningTime))
            {
                Asn1.Cms.Attribute attr = new Asn1.Cms.Attribute(CmsAttributes.SigningTime,
                    new DerSet(new Time(DateTime.UtcNow)));
                std[attr.AttrType] = attr;
            }

            if (!std.Contains(CmsAttributes.MessageDigest))
            {
                byte[] messageDigest = (byte[])parameters[CmsAttributeTableParameter.Digest];
                Asn1.Cms.Attribute attr = new Asn1.Cms.Attribute(CmsAttributes.MessageDigest,
                    new DerSet(new DerOctetString(messageDigest)));
                std[attr.AttrType] = attr;
            }
        }

        /**
		 * @param parameters source parameters
		 * @return the populated attribute table
		 */
		public virtual AttributeTable GetAttributes(
			IDictionary parameters)
		{
            IDictionary table = createStandardAttributeTable(parameters);
			return new AttributeTable(table);
		}
	}
}
