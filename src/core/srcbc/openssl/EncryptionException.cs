using System;
using System.IO;

namespace Org.BouncyCastle.Security
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT)
    [Serializable]
#endif
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class EncryptionException
		: IOException
	{
		public EncryptionException(
			string message)
			: base(message)
		{
		}

		public EncryptionException(
			string		message,
			Exception	exception)
			: base(message, exception)
		{
		}
	}
}
