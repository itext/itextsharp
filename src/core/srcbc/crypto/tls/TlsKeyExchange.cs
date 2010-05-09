using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal enum TlsKeyExchangeAlgorithm : short
	{
		KE_RSA = 1,
		//KE_RSA_EXPORT = 2,
		KE_DHE_DSS = 3,
		//KE_DHE_DSS_EXPORT = 4,
		KE_DHE_RSA = 5,
		//KE_DHE_RSA_EXPORT = 6,
		KE_DH_DSS = 7,
		KE_DH_RSA = 8,
		//KE_DH_anon = 9,
		KE_SRP = 10,
		KE_SRP_DSS = 11,
		KE_SRP_RSA = 12,
	}

	/// <summary>
	/// A generic interface for key exchange implementations in TLS 1.0.
	/// </summary>
	internal interface TlsKeyExchange
	{
		void SkipServerCertificate();// throws IOException;

		void ProcessServerCertificate(Certificate serverCertificate);// throws IOException;

		void SkipServerKeyExchange();// throws IOException;

		void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters);// throws IOException;

		byte[] GenerateClientKeyExchange();// throws IOException;

		byte[] GeneratePremasterSecret();// throws IOException;
	}
}
