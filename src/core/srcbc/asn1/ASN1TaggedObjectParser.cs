using System;

namespace Org.BouncyCastle.Asn1
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface Asn1TaggedObjectParser
		: IAsn1Convertible
	{
		int TagNo { get; }

		IAsn1Convertible GetObjectParser(int tag, bool isExplicit);
	}
}
