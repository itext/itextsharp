using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
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
		protected TlsProtocolHandler handler;
		protected ICertificateVerifyer verifyer;
		protected TlsKeyExchangeAlgorithm keyExchange;
		protected TlsSigner tlsSigner;

		protected AsymmetricKeyParameter serverPublicKey = null;

		protected DHPublicKeyParameters dhAgreeServerPublicKey = null;
		protected AsymmetricCipherKeyPair dhAgreeClientKeyPair = null;

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

		public virtual void SkipServerCertificate()
		{
			handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
		}

		public virtual void ProcessServerCertificate(Certificate serverCertificate)
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

			// TODO Should the 'is' tests be replaced with stricter checks on keyInfo.getAlgorithmId()?

			switch (this.keyExchange)
			{
				case TlsKeyExchangeAlgorithm.KE_DH_DSS:
					if (!(this.serverPublicKey is DHPublicKeyParameters))
					{
						handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
					}
					ValidateKeyUsage(x509Cert, KeyUsage.KeyAgreement);
					// TODO The algorithm used to sign the certificate should be DSS.
//					x509Cert.getSignatureAlgorithm();
					this.dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)this.serverPublicKey);
					break;
				case TlsKeyExchangeAlgorithm.KE_DH_RSA:
					if (!(this.serverPublicKey is DHPublicKeyParameters))
					{
						handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
					}
					ValidateKeyUsage(x509Cert, KeyUsage.KeyAgreement);
					// TODO The algorithm used to sign the certificate should be RSA.
//					x509Cert.getSignatureAlgorithm();
					this.dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)this.serverPublicKey);
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_RSA:
					if (!(this.serverPublicKey is RsaKeyParameters))
					{
						handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
					}
					ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
					break;
				case TlsKeyExchangeAlgorithm.KE_DHE_DSS:
					if (!(this.serverPublicKey is DsaPublicKeyParameters))
					{
						handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
					}
					ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
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
			// OK
		}

		public virtual void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
		{
			handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
		}

		public virtual void GenerateClientKeyExchange(Stream output)
		{
			// TODO RFC 2246 7.4.72
			/*
			* If the client certificate already contains a suitable Diffie-Hellman key, then
			* Yc is implicit and does not need to be sent again. In this case, the Client Key
			* Exchange message will be sent, but will be empty.
			*/
			//TlsUtilities.WriteUint24(0, os);

			/*
			* Generate a keypair (using parameters from server key) and send the public value
			* to the server.
			*/
			DHBasicKeyPairGenerator dhGen = new DHBasicKeyPairGenerator();
			dhGen.Init(new DHKeyGenerationParameters(handler.Random, dhAgreeServerPublicKey.Parameters));
			this.dhAgreeClientKeyPair = dhGen.GenerateKeyPair();
			BigInteger Yc = ((DHPublicKeyParameters)dhAgreeClientKeyPair.Public).Y;
			byte[] keData = BigIntegers.AsUnsignedByteArray(Yc);
            TlsUtilities.WriteUint24(keData.Length + 2, output);
            TlsUtilities.WriteOpaque16(keData, output);
		}

        public virtual byte[] GeneratePremasterSecret()
		{
			/*
			* Diffie-Hellman basic key agreement
			*/
			DHBasicAgreement dhAgree = new DHBasicAgreement();
			dhAgree.Init(dhAgreeClientKeyPair.Private);
			BigInteger agreement = dhAgree.CalculateAgreement(dhAgreeServerPublicKey);
			return BigIntegers.AsUnsignedByteArray(agreement);
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

		protected virtual DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
		{
			BigInteger Y = key.Y;
			DHParameters parameters = key.Parameters;
			BigInteger p = parameters.P;
			BigInteger g = parameters.G;

			if (!p.IsProbablePrime(2))
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.illegal_parameter);
			}
			if (g.CompareTo(BigInteger.Two) < 0 || g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.illegal_parameter);
			}
			if (Y.CompareTo(BigInteger.Two) < 0 || Y.CompareTo(p.Subtract(BigInteger.One)) > 0)
			{
				handler.FailWithError(AlertLevel.fatal, AlertDescription.illegal_parameter);
			}

			// TODO See RFC 2631 for more discussion of Diffie-Hellman validation

			return key;
		}
	}
}
