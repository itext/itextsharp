using System;

namespace Org.BouncyCastle.OpenSsl
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IPasswordFinder
	{
		char[] GetPassword();
	}
}
