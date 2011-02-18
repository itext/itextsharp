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
		protected TlsClientContext context;
		protected KeyExchangeAlgorithm keyExchange;
		protected TlsSigner tlsSigner;

		protected AsymmetricKeyParameter serverPublicKey = null;
		protected DHPublicKeyParameters dhAgreeServerPublicKey = null;
		protected TlsAgreementCredentials agreementCredentials;
		protected DHPrivateKeyParameters dhAgreeClientPrivateKey = null;

		internal TlsDHKeyExchange(TlsClientContext context, KeyExchangeAlgorithm keyExchange)
		{
			switch (keyExchange)
			{
				case KeyExchangeAlgorithm.DH_RSA:
				case KeyExchangeAlgorithm.DH_DSS:
					this.tlsSigner = null;
					break;
				case KeyExchangeAlgorithm.DHE_RSA:
					this.tlsSigner = new TlsRsaSigner();
					break;
				case KeyExchangeAlgorithm.DHE_DSS:
					this.tlsSigner = new TlsDssSigner();
					break;
				default:
					throw new ArgumentException("unsupported key exchange algorithm", "keyExchange");
			}

			this.context = context;
			this.keyExchange = keyExchange;
		}

		public virtual void SkipServerCertificate()
		{
			throw new TlsFatalAlert(AlertDescription.unexpected_message);
		}

		public virtual void ProcessServerCertificate(Certificate serverCertificate)
		{
			X509CertificateStructure x509Cert = serverCertificate.certs[0];
			SubjectPublicKeyInfo keyInfo = x509Cert.SubjectPublicKeyInfo;

			try
			{
				this.serverPublicKey = PublicKeyFactory.CreateKey(keyInfo);
			}
			catch (Exception)
			{
				throw new TlsFatalAlert(AlertDescription.unsupported_certificate);
			}

			if (tlsSigner == null)
			{
				try
				{
					this.dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)this.serverPublicKey);
				}
				catch (InvalidCastException)
				{
					throw new TlsFatalAlert(AlertDescription.certificate_unknown);
				}

				TlsUtilities.ValidateKeyUsage(x509Cert, KeyUsage.KeyAgreement);
			}
			else
			{
				if (!tlsSigner.IsValidPublicKey(this.serverPublicKey))
				{
					throw new TlsFatalAlert(AlertDescription.certificate_unknown);
				}

				TlsUtilities.ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
			}

			// TODO
			/*
			* Perform various checks per RFC2246 7.4.2: "Unless otherwise specified, the
			* signing algorithm for the certificate must be the same as the algorithm for the
			* certificate key."
			*/
		}

		public virtual void SkipServerKeyExchange()
		{
			// OK
		}

		public virtual void ProcessServerKeyExchange(Stream input)
		{
			throw new TlsFatalAlert(AlertDescription.unexpected_message);
		}

		public virtual void ValidateCertificateRequest(CertificateRequest certificateRequest)
		{
			ClientCertificateType[] types = certificateRequest.CertificateTypes;
			foreach (ClientCertificateType type in types)
			{
				switch (type)
				{
					case ClientCertificateType.rsa_sign:
					case ClientCertificateType.dss_sign:
					case ClientCertificateType.rsa_fixed_dh:
					case ClientCertificateType.dss_fixed_dh:
					case ClientCertificateType.ecdsa_sign:
						break;
					default:
						throw new TlsFatalAlert(AlertDescription.illegal_parameter);
				}
			}
		}

		public virtual void SkipClientCredentials()
		{
			this.agreementCredentials = null;
		}

		public virtual void ProcessClientCredentials(TlsCredentials clientCredentials)
		{
			if (clientCredentials is TlsAgreementCredentials)
			{
				// TODO Validate client cert has matching parameters (see 'areCompatibleParameters')?

				this.agreementCredentials = (TlsAgreementCredentials)clientCredentials;
			}
			else if (clientCredentials is TlsSignerCredentials)
			{
				// OK
			}
			else
			{
				throw new TlsFatalAlert(AlertDescription.internal_error);
			}
		}

		public virtual void GenerateClientKeyExchange(Stream output)
		{
			/*
			 * RFC 2246 7.4.7.2 If the client certificate already contains a suitable
			 * Diffie-Hellman key, then Yc is implicit and does not need to be sent again. In
			 * this case, the Client Key Exchange message will be sent, but will be empty.
			 */
			if (agreementCredentials != null)
			{
				TlsUtilities.WriteUint24(0, output);
			}
			else
			{
				GenerateEphemeralClientKeyExchange(dhAgreeServerPublicKey.Parameters, output);
			}
		}

        public virtual byte[] GeneratePremasterSecret()
		{
			if (agreementCredentials != null)
			{
				return agreementCredentials.GenerateAgreement(dhAgreeServerPublicKey);
			}

			return CalculateDHBasicAgreement(dhAgreeServerPublicKey, dhAgreeClientPrivateKey);
		}
		
		protected virtual bool AreCompatibleParameters(DHParameters a, DHParameters b)
		{
			return a.P.Equals(b.P) && a.G.Equals(b.G);
		}

		protected virtual byte[] CalculateDHBasicAgreement(DHPublicKeyParameters publicKey,
			DHPrivateKeyParameters privateKey)
		{
			DHBasicAgreement dhAgree = new DHBasicAgreement();
			dhAgree.Init(dhAgreeClientPrivateKey);
			BigInteger agreement = dhAgree.CalculateAgreement(dhAgreeServerPublicKey);
			return BigIntegers.AsUnsignedByteArray(agreement);
		}

		protected virtual AsymmetricCipherKeyPair GenerateDHKeyPair(DHParameters dhParams)
		{
			DHBasicKeyPairGenerator dhGen = new DHBasicKeyPairGenerator();
			dhGen.Init(new DHKeyGenerationParameters(context.SecureRandom, dhParams));
			return dhGen.GenerateKeyPair();
		}

		protected virtual void GenerateEphemeralClientKeyExchange(DHParameters dhParams, Stream output)
		{
			AsymmetricCipherKeyPair dhAgreeClientKeyPair = GenerateDHKeyPair(dhParams);
			this.dhAgreeClientPrivateKey = (DHPrivateKeyParameters)dhAgreeClientKeyPair.Private;

			BigInteger Yc = ((DHPublicKeyParameters)dhAgreeClientKeyPair.Public).Y;
			byte[] keData = BigIntegers.AsUnsignedByteArray(Yc);
			TlsUtilities.WriteUint24(keData.Length + 2, output);
			TlsUtilities.WriteOpaque16(keData, output);
		}

		protected virtual DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
		{
			BigInteger Y = key.Y;
			DHParameters parameters = key.Parameters;
			BigInteger p = parameters.P;
			BigInteger g = parameters.G;

			if (!p.IsProbablePrime(2))
			{
				throw new TlsFatalAlert(AlertDescription.illegal_parameter);
			}
			if (g.CompareTo(BigInteger.Two) < 0 || g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
			{
				throw new TlsFatalAlert(AlertDescription.illegal_parameter);
			}
			if (Y.CompareTo(BigInteger.Two) < 0 || Y.CompareTo(p.Subtract(BigInteger.One)) > 0)
			{
				throw new TlsFatalAlert(AlertDescription.illegal_parameter);
			}

			// TODO See RFC 2631 for more discussion of Diffie-Hellman validation

			return key;
		}
	}
}
