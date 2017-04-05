using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface TlsAgreementCredentials : TlsCredentials
	{
		/// <exception cref="IOException"></exception>
		byte[] GenerateAgreement(AsymmetricKeyParameter serverPublicKey);
	}
}
