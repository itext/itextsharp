using System;
using System.IO;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <remarks>
	/// A manager for ciphersuite. This class does manage all ciphersuites
	/// which are used by MicroTLS.
	/// </remarks>
	public class TlsCipherSuiteManager
	{
		private const int TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000a;
		private const int TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x0013;
		private const int TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x0016;
		private const int TLS_RSA_WITH_AES_128_CBC_SHA = 0x002f;
		private const int TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x0032;
		private const int TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x0033;
		private const int TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035;
		private const int TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x0038;
		private const int TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x0039;

		internal static void WriteCipherSuites(
			Stream outStr)
		{
			int[] suites = new int[]
			{
				TLS_DHE_RSA_WITH_AES_256_CBC_SHA,
				TLS_DHE_DSS_WITH_AES_256_CBC_SHA,
				TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
				TLS_DHE_DSS_WITH_AES_128_CBC_SHA,
				TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA,
				TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA,
				TLS_RSA_WITH_AES_256_CBC_SHA,
				TLS_RSA_WITH_AES_128_CBC_SHA,
				TLS_RSA_WITH_3DES_EDE_CBC_SHA,
			};

			TlsUtilities.WriteUint16(2 * suites.Length, outStr);
			for (int i = 0; i < suites.Length; ++i)
			{
				TlsUtilities.WriteUint16(suites[i], outStr);
			}
		}

		internal static TlsCipherSuite GetCipherSuite(
			int					number,
			TlsProtocolHandler	handler)
		{
			switch (number)
			{
				case TLS_RSA_WITH_3DES_EDE_CBC_SHA:
					return createDesEdeCipherSuite(24, TlsCipherSuite.KE_RSA);

				case TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA:
					return createDesEdeCipherSuite(24, TlsCipherSuite.KE_DHE_DSS);

				case TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA:
					return createDesEdeCipherSuite(24, TlsCipherSuite.KE_DHE_RSA);

				case TLS_RSA_WITH_AES_128_CBC_SHA:
					return createAesCipherSuite(16, TlsCipherSuite.KE_RSA);

				case TLS_DHE_DSS_WITH_AES_128_CBC_SHA:
					return createAesCipherSuite(16, TlsCipherSuite.KE_DHE_DSS);

				case TLS_DHE_RSA_WITH_AES_128_CBC_SHA:
					return createAesCipherSuite(16, TlsCipherSuite.KE_DHE_RSA);

				case TLS_RSA_WITH_AES_256_CBC_SHA:
					return createAesCipherSuite(32, TlsCipherSuite.KE_RSA);

				case TLS_DHE_DSS_WITH_AES_256_CBC_SHA:
					return createAesCipherSuite(32, TlsCipherSuite.KE_DHE_DSS);

				case TLS_DHE_RSA_WITH_AES_256_CBC_SHA:
					return createAesCipherSuite(32, TlsCipherSuite.KE_DHE_RSA);

				default:
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_handshake_failure);

					/*
					* Unreachable Code, failWithError will always throw an exception!
					*/
					return null;
			}
		}

	    private static TlsCipherSuite createAesCipherSuite(int cipherKeySize, short keyExchange)
	    {
	        return new TlsBlockCipherCipherSuite(createAesCipher(), createAesCipher(),
	            new Sha1Digest(), new Sha1Digest(), cipherKeySize, keyExchange);
	    }

	    private static TlsCipherSuite createDesEdeCipherSuite(int cipherKeySize, short keyExchange)
	    {
	        return new TlsBlockCipherCipherSuite(createDesEdeCipher(), createDesEdeCipher(),
	            new Sha1Digest(), new Sha1Digest(), cipherKeySize, keyExchange);
	    }

	    private static CbcBlockCipher createAesCipher()
	    {
	        return new CbcBlockCipher(new AesFastEngine());
	    }

	    private static CbcBlockCipher createDesEdeCipher()
	    {
	        return new CbcBlockCipher(new DesEdeEngine());
	    }
	}
}
