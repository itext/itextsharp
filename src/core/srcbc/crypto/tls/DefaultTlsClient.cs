using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class DefaultTlsClient
		: TlsClient
	{
		// TODO Add runtime support for this check?
		/*
		* RFC 2246 9. In the absence of an application profile standard specifying otherwise,
		* a TLS compliant application MUST implement the cipher suite
		* TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA.
		*/
		private const int TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000A;
		private const int TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA = 0x000D;
		private const int TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA = 0x0010;
		private const int TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x0013;
		private const int TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x0016;
		
		// RFC 3268
		private const int TLS_RSA_WITH_AES_128_CBC_SHA = 0x002F;
		private const int TLS_DH_DSS_WITH_AES_128_CBC_SHA = 0x0030;
		private const int TLS_DH_RSA_WITH_AES_128_CBC_SHA = 0x0031;
		private const int TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x0032;
		private const int TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x0033;
		private const int TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035;
		private const int TLS_DH_DSS_WITH_AES_256_CBC_SHA = 0x0036;
		private const int TLS_DH_RSA_WITH_AES_256_CBC_SHA = 0x0037;
		private const int TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x0038;
		private const int TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x0039;
		
		// RFC 4279
		//private const int TLS_PSK_WITH_3DES_EDE_CBC_SHA = 0x008B;
		//private const int TLS_PSK_WITH_AES_128_CBC_SHA = 0x008C;
		//private const int TLS_PSK_WITH_AES_256_CBC_SHA = 0x008D;
		//private const int TLS_DHE_PSK_WITH_3DES_EDE_CBC_SHA = 0x008F;
		//private const int TLS_DHE_PSK_WITH_AES_128_CBC_SHA = 0x0090;
		//private const int TLS_DHE_PSK_WITH_AES_256_CBC_SHA = 0x0091;
		//private const int TLS_RSA_PSK_WITH_3DES_EDE_CBC_SHA = 0x0093;
		//private const int TLS_RSA_PSK_WITH_AES_128_CBC_SHA = 0x0094;
		//private const int TLS_RSA_PSK_WITH_AES_256_CBC_SHA = 0x0095;

		// RFC 5054
		private const int TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA = 0xC01A;
		private const int TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA = 0xC01B;
		private const int TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA = 0xC01C;
		private const int TLS_SRP_SHA_WITH_AES_128_CBC_SHA = 0xC01D;
		private const int TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA = 0xC01E;
		private const int TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA = 0xC01F;
		private const int TLS_SRP_SHA_WITH_AES_256_CBC_SHA = 0xC020;
		private const int TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA = 0xC021;
		private const int TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA = 0xC022;

		private ICertificateVerifyer verifyer;

		private TlsProtocolHandler handler;

		// (Optional) details for client-side authentication
		private Certificate clientCert = new Certificate(new X509CertificateStructure[0]);
		private AsymmetricKeyParameter clientPrivateKey = null;
		private TlsSigner clientSigner = null;

		private int selectedCipherSuite;

		internal DefaultTlsClient(ICertificateVerifyer verifyer)
		{
			this.verifyer = verifyer;
		}

		internal void EnableClientAuthentication(Certificate clientCertificate,
			AsymmetricKeyParameter clientPrivateKey)
		{
			if (clientCertificate == null)
			{
				throw new ArgumentNullException("clientCertificate");
			}
			if (clientCertificate.certs.Length == 0)
			{
				throw new ArgumentException("cannot be empty", "clientCertificate");
			}
			if (clientPrivateKey == null)
			{
				throw new ArgumentNullException("clientPrivateKey");
			}
			if (!clientPrivateKey.IsPrivate)
			{
				throw new ArgumentException("must be private", "clientPrivateKey");
			}

			if (clientPrivateKey is RsaKeyParameters)
			{
				clientSigner = new TlsRsaSigner();
			}
			else if (clientPrivateKey is DsaPrivateKeyParameters)
			{
				clientSigner = new TlsDssSigner();
			}
			else
			{
				throw new ArgumentException("type not supported: "
					+ clientPrivateKey.GetType().FullName, "clientPrivateKey");
			}

			this.clientCert = clientCertificate;
			this.clientPrivateKey = clientPrivateKey;
		}

		public void Init(TlsProtocolHandler handler)
		{
			this.handler = handler;
		}

		public int[] GetCipherSuites()
		{
			return new int[] {
				TLS_DHE_RSA_WITH_AES_256_CBC_SHA,
				TLS_DHE_DSS_WITH_AES_256_CBC_SHA,
				TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
				TLS_DHE_DSS_WITH_AES_128_CBC_SHA,
				TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA,
				TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA,
				TLS_RSA_WITH_AES_256_CBC_SHA,
				TLS_RSA_WITH_AES_128_CBC_SHA,
				TLS_RSA_WITH_3DES_EDE_CBC_SHA,

//	            TLS_DH_RSA_WITH_AES_256_CBC_SHA,
//	            TLS_DH_DSS_WITH_AES_256_CBC_SHA,
//	            TLS_DH_RSA_WITH_AES_128_CBC_SHA,
//	            TLS_DH_DSS_WITH_AES_128_CBC_SHA,
//	            TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA,
//	            TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA,

//	            TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA,
//	            TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA,
//	            TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA,
//	            TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA,
//	            TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA,
//	            TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA,
//	            TLS_SRP_SHA_WITH_AES_256_CBC_SHA,
//	            TLS_SRP_SHA_WITH_AES_128_CBC_SHA,
//	            TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA,
			};
		}

		public Hashtable GenerateClientExtensions()
		{
			// TODO[SRP]
//	        Hashtable clientExtensions = new Hashtable();
//	        ByteArrayOutputStream srpData = new ByteArrayOutputStream();
//	        TlsUtils.writeOpaque8(SRP_identity, srpData);
//	
//	        // TODO[SRP] RFC5054 2.8.1: ExtensionType.srp = 12
//	        clientExtensions.put(Integer.valueOf(12), srpData.toByteArray());
//	        return clientExtensions;
			return null;
		}

		public void NotifySessionID(byte[] sessionID)
		{
			// Currently ignored 
		}

		public void NotifySelectedCipherSuite(int selectedCipherSuite)
		{
			this.selectedCipherSuite = selectedCipherSuite;
		}

		public void ProcessServerExtensions(Hashtable serverExtensions)
		{
			// TODO Validate/process serverExtensions (via client?)
			// TODO[SRP]
		}

		public TlsKeyExchange CreateKeyExchange()
		{
			switch (selectedCipherSuite)
			{
				case TLS_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_RSA_WITH_AES_128_CBC_SHA:
				case TLS_RSA_WITH_AES_256_CBC_SHA:
					return CreateRsaKeyExchange();

				case TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA:
				case TLS_DH_DSS_WITH_AES_128_CBC_SHA:
				case TLS_DH_DSS_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DH_DSS);

				case TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_DH_RSA_WITH_AES_128_CBC_SHA:
				case TLS_DH_RSA_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DH_RSA);

				case TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA:
				case TLS_DHE_DSS_WITH_AES_128_CBC_SHA:
				case TLS_DHE_DSS_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DHE_DSS);

				case TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_DHE_RSA_WITH_AES_128_CBC_SHA:
				case TLS_DHE_RSA_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DHE_RSA);

				case TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_WITH_AES_256_CBC_SHA:
					return CreateSrpExchange(TlsKeyExchangeAlgorithm.KE_SRP);

				case TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA:
					return CreateSrpExchange(TlsKeyExchangeAlgorithm.KE_SRP_RSA);

				case TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA:
					return CreateSrpExchange(TlsKeyExchangeAlgorithm.KE_SRP_DSS);

				default:
					/*
					* Note: internal error here; the TlsProtocolHandler verifies that the
					* server-selected cipher suite was in the list of client-offered cipher
					* suites, so if we now can't produce an implementation, we shouldn't have
					* offered it!
					*/
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
					return null; // Unreachable!
			}
		}

		public void ProcessServerCertificateRequest(byte[] certificateTypes, IList certificateAuthorities)
		{
			// TODO There shouldn't be a certificate request for SRP 

			// TODO Use provided info to choose a certificate in GetCertificate()
		}

		public Certificate GetCertificate()
		{
			return clientCert;
		}

		public byte[] GenerateCertificateSignature(byte[] md5andsha1)
		{
			if (clientSigner == null)
				return null;

			try
			{
				return clientSigner.CalculateRawSignature(clientPrivateKey, md5andsha1);
			}
			catch (CryptoException)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
				return null;
			}
		}

		public TlsCipher CreateCipher(SecurityParameters securityParameters)
		{
			switch (selectedCipherSuite)
			{
				case TLS_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA:
				case TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA:
				case TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA:
				case TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA:
					return CreateDesEdeCipher(24, securityParameters);
				
				case TLS_RSA_WITH_AES_128_CBC_SHA:
				case TLS_DH_DSS_WITH_AES_128_CBC_SHA:
				case TLS_DH_RSA_WITH_AES_128_CBC_SHA:
				case TLS_DHE_DSS_WITH_AES_128_CBC_SHA:
				case TLS_DHE_RSA_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA:
				case TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA:
					return CreateAesCipher(16, securityParameters);
				
				case TLS_RSA_WITH_AES_256_CBC_SHA:
				case TLS_DH_DSS_WITH_AES_256_CBC_SHA:
				case TLS_DH_RSA_WITH_AES_256_CBC_SHA:
				case TLS_DHE_DSS_WITH_AES_256_CBC_SHA:
				case TLS_DHE_RSA_WITH_AES_256_CBC_SHA:
				case TLS_SRP_SHA_WITH_AES_256_CBC_SHA:
				case TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA:
				case TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA:
					return CreateAesCipher(32, securityParameters);
				
				default:
					/*
					* Note: internal error here; the TlsProtocolHandler verifies that the
					* server-selected cipher suite was in the list of client-offered cipher
					* suites, so if we now can't produce an implementation, we shouldn't have
					* offered it!
					*/
					handler.FailWithError(TlsProtocolHandler.AL_fatal,
						TlsProtocolHandler.AP_internal_error);
					return null; // Unreachable!
			}
		}

		private TlsKeyExchange CreateDHKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
		{
			return new TlsDHKeyExchange(handler, verifyer, keyExchange);
		}

		private TlsKeyExchange CreateRsaKeyExchange()
		{
			return new TlsRsaKeyExchange(handler, verifyer);
		}

		private TlsKeyExchange CreateSrpExchange(TlsKeyExchangeAlgorithm keyExchange)
		{
			return new TlsSrpKeyExchange(handler, verifyer, keyExchange);
		}

		private TlsCipher CreateAesCipher(int cipherKeySize, SecurityParameters securityParameters)
		{
			return new TlsBlockCipher(handler, CreateAesBlockCipher(), CreateAesBlockCipher(),
				new Sha1Digest(), new Sha1Digest(), cipherKeySize, securityParameters);
		}

		private TlsCipher CreateDesEdeCipher(int cipherKeySize, SecurityParameters securityParameters)
		{
			return new TlsBlockCipher(handler, CreateDesEdeBlockCipher(), CreateDesEdeBlockCipher(),
				new Sha1Digest(), new Sha1Digest(), cipherKeySize, securityParameters);
		}

		private static IBlockCipher CreateAesBlockCipher()
		{
			return new CbcBlockCipher(new AesFastEngine());
		}

		private static IBlockCipher CreateDesEdeBlockCipher()
		{
			return new CbcBlockCipher(new DesEdeEngine());
		}
	}
}
