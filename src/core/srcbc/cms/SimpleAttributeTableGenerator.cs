using System;
using System.Collections;

using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Cms
{
	/**
	 * Basic generator that just returns a preconstructed attribute table
	 */
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class SimpleAttributeTableGenerator
		: CmsAttributeTableGenerator
	{
		private readonly AttributeTable attributes;

		public SimpleAttributeTableGenerator(
			AttributeTable attributes)
		{
			this.attributes = attributes;
		}

		public virtual AttributeTable GetAttributes(
			IDictionary parameters)
		{
			return attributes;
		}
	}
}
