using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// TLS 1.0 DH key exchange.
	/// </summary>
	internal class TlsDHKeyExchange
		: TlsKeyExchange
	{
		private TlsProtocolHandler handler;
		private ICertificateVerifyer verifyer;
		private TlsKeyExchangeAlgorithm keyExchange;
		private TlsSigner tlsSigner;

		private AsymmetricKeyParameter serverPublicKey = null;

		private DHPublicKeyParameters dhAgreeServerPublicKey = null;
		private AsymmetricCipherKeyPair dhAgreeClientKeyPair = null;

		internal TlsDHKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer,
			TlsKeyExchangeAlgorithm keyExchange)
		{
			switch (keyExchange)
			{
				case TlsKeyExchangeAlgorithm.KE_DH_RSA:
				case TlsKeyExchangeAlgorithm.KE_DH_DSS:
					this.tlsSigner = null;
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_RSA:
					this.tlsSigner = new TlsRsaSigner();
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_DSS:
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
			handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
		}

		public void ProcessServerCertificate(Certificate serverCertificate)
		{
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

			// TODO Should the 'is' tests be replaced with stricter checks on keyInfo.getAlgorithmId()?

			switch (this.keyExchange)
			{
				case TlsKeyExchangeAlgorithm.KE_DH_DSS:
					if (!(this.serverPublicKey is DHPublicKeyParameters))
					{
						handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
					}
					// TODO The algorithm used to sign the certificate should be DSS.
//					x509Cert.getSignatureAlgorithm();
					this.dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)this.serverPublicKey);
					break;
				case TlsKeyExchangeAlgorithm.KE_DH_RSA:
					if (!(this.serverPublicKey is DHPublicKeyParameters))
					{
						handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
					}
					// TODO The algorithm used to sign the certificate should be RSA.
//					x509Cert.getSignatureAlgorithm();
					this.dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)this.serverPublicKey);
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_RSA:
					if (!(this.serverPublicKey is RsaKeyParameters))
					{
						handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
					}
					ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_DSS:
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
			if (tlsSigner != null)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
			}
		}

		public void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
		{
			if (tlsSigner == null)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
			}

			Stream sigIn = input;
			ISigner signer = null;

			if (tlsSigner != null)
			{
				signer = InitSigner(tlsSigner, securityParameters);
				sigIn = new SignerStream(input, signer, null);
			}

			byte[] pBytes = TlsUtilities.ReadOpaque16(sigIn);
			byte[] gBytes = TlsUtilities.ReadOpaque16(sigIn);
			byte[] YsBytes = TlsUtilities.ReadOpaque16(sigIn);

			if (signer != null)
			{
				byte[] sigByte = TlsUtilities.ReadOpaque16(input);

				if (!signer.VerifySignature(sigByte))
				{
					handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_certificate);
				}
			}

			BigInteger p = new BigInteger(1, pBytes);
			BigInteger g = new BigInteger(1, gBytes);
			BigInteger Ys = new BigInteger(1, YsBytes);

			this.dhAgreeServerPublicKey = ValidateDHPublicKey(
				new DHPublicKeyParameters(Ys, new DHParameters(p, g)));
		}

		public byte[] GenerateClientKeyExchange()
		{
			// TODO RFC 2246 7.4.72
			/*
			* If the client certificate already contains a suitable Diffie-Hellman key, then
			* Yc is implicit and does not need to be sent again. In this case, the Client Key
			* Exchange message will be sent, but will be empty.
			*/
			//return new byte[0];

			/*
			* Generate a keypair (using parameters from server key) and send the public value
			* to the server.
			*/
			DHBasicKeyPairGenerator dhGen = new DHBasicKeyPairGenerator();
			dhGen.Init(new DHKeyGenerationParameters(handler.Random, dhAgreeServerPublicKey.Parameters));
			this.dhAgreeClientKeyPair = dhGen.GenerateKeyPair();
			BigInteger Yc = ((DHPublicKeyParameters)dhAgreeClientKeyPair.Public).Y;
			return BigIntegers.AsUnsignedByteArray(Yc);
		}

		public byte[] GeneratePremasterSecret()
		{
			/*
			* Diffie-Hellman basic key agreement
			*/
			DHBasicAgreement dhAgree = new DHBasicAgreement();
			dhAgree.Init(dhAgreeClientKeyPair.Private);
			BigInteger agreement = dhAgree.CalculateAgreement(dhAgreeServerPublicKey);
			return BigIntegers.AsUnsignedByteArray(agreement);
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

		private DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
		{
			BigInteger Y = key.Y;
			DHParameters parameters = key.Parameters;
			BigInteger p = parameters.P;
			BigInteger g = parameters.G;

			if (!p.IsProbablePrime(2))
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
			}
			if (g.CompareTo(BigInteger.Two) < 0 || g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
			}
			if (Y.CompareTo(BigInteger.Two) < 0 || Y.CompareTo(p.Subtract(BigInteger.One)) > 0)
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
			}

			// TODO See RFC 2631 for more discussion of Diffie-Hellman validation

			return key;
		}
	}
}
