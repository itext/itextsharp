using System;

namespace Org.BouncyCastle.X509.Store
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IX509Selector
#if !SILVERLIGHT
		: ICloneable
#endif
	{
#if SILVERLIGHT
        object Clone();
#endif
        bool Match(object obj);
	}
}
