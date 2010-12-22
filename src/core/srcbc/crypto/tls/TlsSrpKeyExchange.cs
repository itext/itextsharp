using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// TLS 1.1 SRP key exchange.
	/// </summary>
	internal class TlsSrpKeyExchange
		: TlsKeyExchange
	{
		protected TlsProtocolHandler handler;
        protected ICertificateVerifyer verifyer;
        protected TlsKeyExchangeAlgorithm keyExchange;
        protected TlsSigner tlsSigner;

        protected AsymmetricKeyParameter serverPublicKey = null;

		// TODO Need a way of providing these
        protected byte[] SRP_identity = null;
        protected byte[] SRP_password = null;

        protected byte[] s = null;
        protected BigInteger B = null;
        protected Srp6Client srpClient = new Srp6Client();

		internal TlsSrpKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer,
			TlsKeyExchangeAlgorithm keyExchange)
		{
			switch (keyExchange)
			{
				case TlsKeyExchangeAlgorithm.KE_SRP:
					this.tlsSigner = null;
					break;
				case TlsKeyExchangeAlgorithm.KE_SRP_RSA:
					this.tlsSigner = new TlsRsaSigner();
					break;
				case TlsKeyExchangeAlgorithm.KE_SRP_DSS:
					this.tlsSigner = new TlsDssSigner();
					break;
				default:
					throw new ArgumentException("unsupported key exchange algorithm", "keyExchange");
			}

			this.handler = handler;
			this.verifyer = verifyer;
			this.keyExchange = keyExchange;
		}

		public virtual void SkipServerCertificate()
		{
			if (tlsSigner != null)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
			}
		}

		public virtual void ProcessServerCertificate(Certificate serverCertificate)
		{
			if (tlsSigner == null)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
			}

			X509CertificateStructure x509Cert = serverCertificate.certs[0];
			SubjectPublicKeyInfo keyInfo = x509Cert.SubjectPublicKeyInfo;

			try
			{
				this.serverPublicKey = PublicKeyFactory.CreateKey(keyInfo);
			}
//			catch (RuntimeException)
			catch (Exception)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.unsupported_certificate);
			}

			// Sanity check the PublicKeyFactory
			if (this.serverPublicKey.IsPrivate)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.internal_error);
			}

			// TODO
			/*
			* Perform various checks per RFC2246 7.4.2: "Unless otherwise specified, the
			* signing algorithm for the certificate must be the same as the algorithm for the
			* certificate key."
			*/
			switch (this.keyExchange)
			{
				case TlsKeyExchangeAlgorithm.KE_SRP_RSA:
				if (!(this.serverPublicKey is RsaKeyParameters))
				{
					handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
				}
				ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
				break;
			case TlsKeyExchangeAlgorithm.KE_SRP_DSS:
				if (!(this.serverPublicKey is DsaPublicKeyParameters))
				{
					handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
				}
				break;
			default:
				handler.FailWithError(AlertLevel.fatal, AlertDescription.unsupported_certificate);
				break;
			}

			/*
			* Verify them.
			*/
			if (!this.verifyer.IsValid(serverCertificate.GetCerts()))
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.user_canceled);
			}
		}

		public virtual void SkipServerKeyExchange()
		{
			handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
		}

		public virtual void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
		{
			Stream sigIn = input;
			ISigner signer = null;

			if (tlsSigner != null)
			{
				signer = InitSigner(tlsSigner, securityParameters);
				sigIn = new SignerStream(input, signer, null);
			}

			byte[] NBytes = TlsUtilities.ReadOpaque16(sigIn);
			byte[] gBytes = TlsUtilities.ReadOpaque16(sigIn);
			byte[] sBytes = TlsUtilities.ReadOpaque8(sigIn);
			byte[] BBytes = TlsUtilities.ReadOpaque16(sigIn);

			if (signer != null)
			{
				byte[] sigByte = TlsUtilities.ReadOpaque16(input);

				if (!signer.VerifySignature(sigByte))
				{
					handler.FailWithError(AlertLevel.fatal, AlertDescription.bad_certificate);
				}
			}

			BigInteger N = new BigInteger(1, NBytes);
			BigInteger g = new BigInteger(1, gBytes);

			// TODO Validate group parameters (see RFC 5054)
			//handler.FailWithError(AlertLevel.fatal, AlertDescription.insufficient_security);

			this.s = sBytes;

			/*
			* RFC 5054 2.5.3: The client MUST abort the handshake with an "illegal_parameter"
			* alert if B % N = 0.
			*/
			try
			{
				this.B = Srp6Utilities.ValidatePublicValue(N, new BigInteger(1, BBytes));
			}
			catch (CryptoException)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.illegal_parameter);
			}

			this.srpClient.Init(N, g, new Sha1Digest(), handler.Random);
		}

        public virtual void GenerateClientKeyExchange(Stream output)
		{
			byte[] keData = BigIntegers.AsUnsignedByteArray(srpClient.GenerateClientCredentials(s,
				this.SRP_identity, this.SRP_password));
            TlsUtilities.WriteUint24(keData.Length + 2, output);
            TlsUtilities.WriteOpaque16(keData, output);
		}

		public virtual byte[] GeneratePremasterSecret()
		{
			try
			{
				// TODO Check if this needs to be a fixed size
				return BigIntegers.AsUnsignedByteArray(srpClient.CalculateSecret(B));
			}
			catch (CryptoException)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.illegal_parameter);
				return null; // Unreachable!
			}
		}

		protected virtual void ValidateKeyUsage(X509CertificateStructure c, int keyUsageBits)
		{
			X509Extensions exts = c.TbsCertificate.Extensions;
			if (exts != null)
			{
				X509Extension ext = exts.GetExtension(X509Extensions.KeyUsage);
				if (ext != null)
				{
					DerBitString ku = KeyUsage.GetInstance(ext);
					int bits = ku.GetBytes()[0];
					if ((bits & keyUsageBits) != keyUsageBits)
					{
						handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
					}
				}
			}
		}

        protected virtual ISigner InitSigner(TlsSigner tlsSigner, SecurityParameters securityParameters)
		{
			ISigner signer = tlsSigner.CreateVerifyer(this.serverPublicKey);
			signer.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
			signer.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
			return signer;
		}
	}
}
