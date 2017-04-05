using System;

namespace Org.BouncyCastle.Asn1
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IAsn1ApplicationSpecificParser
    	: IAsn1Convertible
	{
    	IAsn1Convertible ReadObject();
	}
}
