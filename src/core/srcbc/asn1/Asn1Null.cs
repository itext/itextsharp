using System;

namespace Org.BouncyCastle.Asn1
{
    /**
     * A Null object.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class Asn1Null
        : Asn1Object
    {
        internal Asn1Null()
        {
        }

		public override string ToString()
		{
			return "NULL";
		}
    }
}
