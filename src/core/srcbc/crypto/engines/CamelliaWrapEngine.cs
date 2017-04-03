using System;

namespace Org.BouncyCastle.Crypto.Engines
{
	/// <remarks>
	/// An implementation of the Camellia key wrapper based on RFC 3657/RFC 3394.
	/// <p/>
	/// For further details see: <a href="http://www.ietf.org/rfc/rfc3657.txt">http://www.ietf.org/rfc/rfc3657.txt</a>.
	/// </remarks>
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class CamelliaWrapEngine
		: Rfc3394WrapEngine
	{
		public CamelliaWrapEngine()
			: base(new CamelliaEngine())
		{
		}
	}
}
