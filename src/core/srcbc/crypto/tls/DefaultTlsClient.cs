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
		private ICertificateVerifyer verifyer;

		private TlsProtocolHandler handler;

		// (Optional) details for client-side authentication
		private Certificate clientCert = new Certificate(new X509CertificateStructure[0]);
		private AsymmetricKeyParameter clientPrivateKey = null;
		private TlsSigner clientSigner = null;

		private CipherSuite selectedCipherSuite;

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

		public virtual void Init(TlsProtocolHandler handler)
		{
			this.handler = handler;
		}

        public virtual CipherSuite[] GetCipherSuites()
		{
			return new CipherSuite[] {
				CipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA,
				CipherSuite.TLS_DHE_DSS_WITH_AES_256_CBC_SHA,
				CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
				CipherSuite.TLS_DHE_DSS_WITH_AES_128_CBC_SHA,
				CipherSuite.TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA,
				CipherSuite.TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA,
				CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
				CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
				CipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA,

//	            CipherSuite.TLS_DH_RSA_WITH_AES_256_CBC_SHA,
//	            CipherSuite.TLS_DH_DSS_WITH_AES_256_CBC_SHA,
//	            CipherSuite.TLS_DH_RSA_WITH_AES_128_CBC_SHA,
//	            CipherSuite.TLS_DH_DSS_WITH_AES_128_CBC_SHA,
//	            CipherSuite.TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA,
//	            CipherSuite.TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA,

//	            CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_WITH_AES_256_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_WITH_AES_128_CBC_SHA,
//	            CipherSuite.TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA,
			};
		}

        public virtual CompressionMethod[] GetCompressionMethods()
        {
            return new CompressionMethod[] { CompressionMethod.NULL };
        }

        public virtual IDictionary GenerateClientExtensions()
		{
			// TODO[SRP]
//	        Hashtable clientExtensions = new Hashtable();
//	        ByteArrayOutputStream srpData = new ByteArrayOutputStream();
//	        TlsUtils.writeOpaque8(SRP_identity, srpData);
//
//	        clientExtensions.put(ExtensionType.srp, srpData.toByteArray());
//	        return clientExtensions;
			return null;
		}

        public virtual void NotifySessionID(byte[] sessionID)
		{
			// Currently ignored
		}

        public virtual void NotifySelectedCipherSuite(CipherSuite selectedCipherSuite)
		{
			this.selectedCipherSuite = selectedCipherSuite;
		}

        public virtual void NotifySelectedCompressionMethod(CompressionMethod selectedCompressionMethod)
        {
            // TODO Store and use
        }

        public virtual void NotifySecureRenegotiation(bool secureRenegotiation)
		{
			if (!secureRenegotiation)
			{
				/*
				 * RFC 5746 3.4.
				 * If the extension is not present, the server does not support
				 * secure renegotiation; set secure_renegotiation flag to FALSE.
				 * In this case, some clients may want to terminate the handshake
				 * instead of continuing; see Section 4.1 for discussion.
				 */
//				handler.FailWithError(AlertLevel.fatal, AlertDescription.handshake_failure);
			}
		}

        public virtual void ProcessServerExtensions(IDictionary serverExtensions)
		{
			// TODO Validate/process serverExtensions (via client?)
			// TODO[SRP]
		}

        public virtual TlsKeyExchange CreateKeyExchange()
		{
			switch (selectedCipherSuite)
			{
				case CipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA:
					return CreateRsaKeyExchange();

				case CipherSuite.TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DH_DSS_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DH_DSS_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DH_DSS);

				case CipherSuite.TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DH_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DH_RSA_WITH_AES_256_CBC_SHA:
					return CreateDHKeyExchange(TlsKeyExchangeAlgorithm.KE_DH_RSA);

				case CipherSuite.TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DHE_DSS_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DHE_DSS_WITH_AES_256_CBC_SHA:
					return CreateDheKeyExchange(TlsKeyExchangeAlgorithm.KE_DHE_DSS);

				case CipherSuite.TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA:
					return CreateDheKeyExchange(TlsKeyExchangeAlgorithm.KE_DHE_RSA);

                case CipherSuite.TLS_ECDH_ECDSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA:
                    return CreateECDHKeyExchange(TlsKeyExchangeAlgorithm.KE_ECDH_ECDSA);

                case CipherSuite.TLS_ECDHE_ECDSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA:
                    return CreateECDheKeyExchange(TlsKeyExchangeAlgorithm.KE_ECDHE_ECDSA);

                case CipherSuite.TLS_ECDH_RSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDH_RSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDH_RSA_WITH_AES_256_CBC_SHA:
                    return CreateECDHKeyExchange(TlsKeyExchangeAlgorithm.KE_ECDH_RSA);

                case CipherSuite.TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA:
                    return CreateECDheKeyExchange(TlsKeyExchangeAlgorithm.KE_ECDHE_RSA);

				case CipherSuite.TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_WITH_AES_256_CBC_SHA:
					return CreateSrpKeyExchange(TlsKeyExchangeAlgorithm.KE_SRP);

				case CipherSuite.TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA:
                    return CreateSrpKeyExchange(TlsKeyExchangeAlgorithm.KE_SRP_RSA);

				case CipherSuite.TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA:
                    return CreateSrpKeyExchange(TlsKeyExchangeAlgorithm.KE_SRP_DSS);

				default:
					/*
					* Note: internal error here; the TlsProtocolHandler verifies that the
					* server-selected cipher suite was in the list of client-offered cipher
					* suites, so if we now can't produce an implementation, we shouldn't have
					* offered it!
					*/
					handler.FailWithError(AlertLevel.fatal, AlertDescription.internal_error);
					return null; // Unreachable!
			}
		}

        public virtual void ProcessServerCertificateRequest(ClientCertificateType[] certificateTypes,
			IList certificateAuthorities)
		{
			// TODO There shouldn't be a certificate request for SRP

			// TODO Use provided info to choose a certificate in GetCertificate()
		}

        public virtual Certificate GetCertificate()
		{
			return clientCert;
		}

        public virtual byte[] GenerateCertificateSignature(byte[] md5andsha1)
		{
			if (clientSigner == null)
				return null;

			try
			{
				return clientSigner.CalculateRawSignature(clientPrivateKey, md5andsha1);
			}
			catch (CryptoException)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.internal_error);
				return null;
			}
		}

        public virtual TlsCipher CreateCipher(SecurityParameters securityParameters)
		{
			switch (selectedCipherSuite)
			{
				case CipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDH_ECDSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDHE_ECDSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDH_RSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA:
                case CipherSuite.TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA:
					return CreateDesEdeCipher(24, securityParameters);

				case CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DH_DSS_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DH_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DHE_DSS_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDH_RSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA:
                case CipherSuite.TLS_SRP_SHA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA:
					return CreateAesCipher(16, securityParameters);

				case CipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_DH_DSS_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_DH_RSA_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_DHE_DSS_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA:
                case CipherSuite.TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA:
                case CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA:
                case CipherSuite.TLS_ECDH_RSA_WITH_AES_256_CBC_SHA:
                case CipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA:
                case CipherSuite.TLS_SRP_SHA_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA:
				case CipherSuite.TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA:
					return CreateAesCipher(32, securityParameters);

				default:
					/*
					* Note: internal error here; the TlsProtocolHandler verifies that the
					* server-selected cipher suite was in the list of client-offered cipher
					* suites, so if we now can't produce an implementation, we shouldn't have
					* offered it!
					*/
					handler.FailWithError(AlertLevel.fatal, AlertDescription.internal_error);
					return null; // Unreachable!
			}
		}

		protected virtual TlsKeyExchange CreateDHKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
		{
			return new TlsDHKeyExchange(handler, verifyer, keyExchange);
		}

        protected virtual TlsKeyExchange CreateDheKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
		{
			return new TlsDheKeyExchange(handler, verifyer, keyExchange);
		}

        protected virtual TlsKeyExchange CreateECDHKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
        {
            return new TlsECDHKeyExchange(handler, verifyer, keyExchange, clientCert, clientPrivateKey);
        }

        protected virtual TlsKeyExchange CreateECDheKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
        {
            return new TlsECDheKeyExchange(handler, verifyer, keyExchange, clientCert, clientPrivateKey);
        }

        protected virtual TlsKeyExchange CreateRsaKeyExchange()
		{
			return new TlsRsaKeyExchange(handler, verifyer);
		}

        protected virtual TlsKeyExchange CreateSrpKeyExchange(TlsKeyExchangeAlgorithm keyExchange)
		{
			return new TlsSrpKeyExchange(handler, verifyer, keyExchange);
		}

		protected virtual TlsCipher CreateAesCipher(int cipherKeySize, SecurityParameters securityParameters)
		{
			return new TlsBlockCipher(handler, CreateAesBlockCipher(), CreateAesBlockCipher(),
                CreateSha1Digest(), CreateSha1Digest(), cipherKeySize, securityParameters);
		}

        protected virtual TlsCipher CreateDesEdeCipher(int cipherKeySize, SecurityParameters securityParameters)
		{
			return new TlsBlockCipher(handler, CreateDesEdeBlockCipher(), CreateDesEdeBlockCipher(),
                CreateSha1Digest(), CreateSha1Digest(), cipherKeySize, securityParameters);
		}

		protected virtual IBlockCipher CreateAesBlockCipher()
		{
			return new CbcBlockCipher(new AesFastEngine());
		}

        protected virtual IBlockCipher CreateDesEdeBlockCipher()
		{
			return new CbcBlockCipher(new DesEdeEngine());
		}

        protected virtual IDigest CreateSha1Digest()
        {
            return new Sha1Digest();
        }
    }
}
