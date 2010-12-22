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
		KE_ECDH_ECDSA = 13,
		KE_ECDHE_ECDSA = 14,
		KE_ECDH_RSA = 15,
		KE_ECDHE_RSA = 16,
		//KE_ECDH_anon = 17,
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

		void GenerateClientKeyExchange(Stream output);// throws IOException;

		byte[] GeneratePremasterSecret();// throws IOException;
	}
}
