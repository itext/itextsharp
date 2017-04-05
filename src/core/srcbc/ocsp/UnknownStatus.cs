using System;

namespace Org.BouncyCastle.Ocsp
{
	/**
	 * wrapper for the UnknownInfo object
	 */
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class UnknownStatus
		: CertificateStatus
	{
		public UnknownStatus()
		{
		}
	}
}
