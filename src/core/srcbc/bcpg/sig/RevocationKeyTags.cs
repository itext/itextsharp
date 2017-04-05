using System;

namespace Org.BouncyCastle.Bcpg
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum RevocationKeyTag
		: byte
    {
		ClassDefault = 0x80,
		ClassSensitive = 0x40
	}
}
