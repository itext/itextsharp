using System;

namespace Org.BouncyCastle.Asn1.Kisa
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class KisaObjectIdentifiers
	{
		public static readonly DerObjectIdentifier IdSeedCbc = new DerObjectIdentifier("1.2.410.200004.1.4");
		public static readonly DerObjectIdentifier IdNpkiAppCmsSeedWrap = new DerObjectIdentifier("1.2.410.200004.7.1.1.1");
	}
}
