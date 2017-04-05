using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class TlsFatalAlert
		: IOException
	{
		private readonly AlertDescription alertDescription;

		public TlsFatalAlert(AlertDescription alertDescription)
		{
			this.alertDescription = alertDescription;
		}

		public AlertDescription AlertDescription
		{
			get { return alertDescription; }
		}
	}
}
