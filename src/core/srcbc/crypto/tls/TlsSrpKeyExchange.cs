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
		private TlsProtocolHandler handler;
		private ICertificateVerifyer verifyer;
		private TlsKeyExchangeAlgorithm keyExchange;
		private TlsSigner tlsSigner;

		private AsymmetricKeyParameter serverPublicKey = null;

		// TODO Need a way of providing these
		private byte[] SRP_identity = null;
		private byte[] SRP_password = null;

		private byte[] s = null;
		private BigInteger B = null;
		private Srp6Client srpClient = new Srp6Client();

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

		public void SkipServerCertificate()
		{
			if (tlsSigner != null)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
			}
		}

		public void ProcessServerCertificate(Certificate serverCertificate)
		{
			if (tlsSigner == null)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
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
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unsupported_certificate);
			}

			// Sanity check the PublicKeyFactory
			if (this.serverPublicKey.IsPrivate)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
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
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
				}
				ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
				break;
			case TlsKeyExchangeAlgorithm.KE_SRP_DSS:
				if (!(this.serverPublicKey is DsaPublicKeyParameters))
				{
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
				}
				break;
			default:
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unsupported_certificate);
				break;
			}

			/*
			* Verify them.
			*/
			if (!this.verifyer.IsValid(serverCertificate.GetCerts()))
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_user_canceled);
			}
		}

		public void SkipServerKeyExchange()
		{
			handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
		}

		public void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
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
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_certificate);
				}
			}

			BigInteger N = new BigInteger(1, NBytes);
			BigInteger g = new BigInteger(1, gBytes);

			// TODO Validate group parameters (see RFC 5054)
			//handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_insufficient_security);

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
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
			}

			this.srpClient.Init(N, g, new Sha1Digest(), handler.Random);
		}

		public byte[] GenerateClientKeyExchange()
		{
			return BigIntegers.AsUnsignedByteArray(srpClient.GenerateClientCredentials(s,
				this.SRP_identity, this.SRP_password));
		}

		public byte[] GeneratePremasterSecret()
		{
			try
			{
				// TODO Check if this needs to be a fixed size
				return BigIntegers.AsUnsignedByteArray(srpClient.CalculateSecret(B));
			}
			catch (CryptoException)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
				return null; // Unreachable!
			}
		}
		
		private void ValidateKeyUsage(X509CertificateStructure c, int keyUsageBits)
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
						handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
					}
				}
			}
		}

		private ISigner InitSigner(TlsSigner tlsSigner, SecurityParameters securityParameters)
		{
			ISigner signer = tlsSigner.CreateVerifyer(this.serverPublicKey);
			signer.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
			signer.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
			return signer;
		}
	}
}
