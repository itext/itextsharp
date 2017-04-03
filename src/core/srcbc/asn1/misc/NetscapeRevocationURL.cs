using System;
using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Misc
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class NetscapeRevocationUrl
        : DerIA5String
    {
        public NetscapeRevocationUrl(DerIA5String str)
			: base(str.GetString())
        {
        }

        public override string ToString()
        {
            return "NetscapeRevocationUrl: " + this.GetString();
        }
    }
}
