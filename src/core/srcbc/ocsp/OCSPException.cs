using System;

namespace Org.BouncyCastle.Ocsp
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class OcspException
		: Exception
	{
		public OcspException()
		{
		}

		public OcspException(
			string message)
			: base(message)
		{
		}

		public OcspException(
			string		message,
			Exception	e)
			: base(message, e)
		{
		}
	}
}
