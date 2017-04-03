using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IStreamGenerator
	{
		void Close();
	}
}
