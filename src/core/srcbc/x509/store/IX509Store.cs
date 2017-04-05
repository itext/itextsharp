using System;
using System.Collections;

namespace Org.BouncyCastle.X509.Store
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IX509Store
	{
//		void Init(IX509StoreParameters parameters);
		ICollection GetMatches(IX509Selector selector);
	}
}
