using System;

namespace Org.BouncyCastle.Asn1
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class BerApplicationSpecific
		: DerApplicationSpecific
	{
		public BerApplicationSpecific(
			int					tagNo,
			Asn1EncodableVector	vec)
			: base(tagNo, vec)
		{
		}
	}
}
