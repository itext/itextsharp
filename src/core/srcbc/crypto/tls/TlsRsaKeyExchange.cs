using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// TLS 1.0 RSA key exchange.
	/// </summary>
	internal class TlsRsaKeyExchange
		: TlsKeyExchange
	{
		private TlsProtocolHandler handler;
		private ICertificateVerifyer verifyer;

		private AsymmetricKeyParameter serverPublicKey = null;

		private RsaKeyParameters rsaServerPublicKey = null;

		private byte[] premasterSecret;

		internal TlsRsaKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer)
		{
			this.handler = handler;
			this.verifyer = verifyer;
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

			// TODO Should the 'is' tests be replaces with stricter checks on keyInfo.getAlgorithmId()?

			if (!(this.serverPublicKey is RsaKeyParameters))
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
			}
			ValidateKeyUsage(x509Cert, KeyUsage.KeyEncipherment);
			this.rsaServerPublicKey = ValidateRsaPublicKey((RsaKeyParameters)this.serverPublicKey);

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
			// OK
		}

		public void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
		{
			handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
		}

		public byte[] GenerateClientKeyExchange()
		{
			/*
			* Choose a PremasterSecret and send it encrypted to the server
			*/
			premasterSecret = new byte[48];
			handler.Random.NextBytes(premasterSecret);
			TlsUtilities.WriteVersion(premasterSecret, 0);

			Pkcs1Encoding encoding = new Pkcs1Encoding(new RsaBlindedEngine());
			encoding.Init(true, new ParametersWithRandom(this.rsaServerPublicKey, handler.Random));
			
			try
			{
				return encoding.ProcessBlock(premasterSecret, 0, premasterSecret.Length);
			}
			catch (InvalidCipherTextException)
			{
				/*
				* This should never happen, only during decryption.
				*/
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
				return null; // Unreachable!
			}
		}

		public byte[] GeneratePremasterSecret()
		{
			byte[] tmp = this.premasterSecret;
			this.premasterSecret = null;
			return tmp;
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

//	    private void ProcessRsaServerKeyExchange(Stream input, ISigner signer)
//	    {
//	        Stream sigIn = input;
//	        if (signer != null)
//	        {
//	            sigIn = new SignerStream(input, signer, null);
//	        }
//	
//	        byte[] modulusBytes = TlsUtilities.ReadOpaque16(sigIn);
//	        byte[] exponentBytes = TlsUtilities.ReadOpaque16(sigIn);
//	
//	        if (signer != null)
//	        {
//	            byte[] sigByte = TlsUtilities.ReadOpaque16(input);
//	
//	            if (!signer.VerifySignature(sigByte))
//	            {
//	                handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_certificate);
//	            }
//	        }
//	
//	        BigInteger modulus = new BigInteger(1, modulusBytes);
//	        BigInteger exponent = new BigInteger(1, exponentBytes);
//	
//	        this.rsaServerPublicKey = ValidateRSAPublicKey(new RsaKeyParameters(false, modulus, exponent));
//	    }

		private RsaKeyParameters ValidateRsaPublicKey(RsaKeyParameters key)
		{
			// TODO What is the minimum bit length required?
//			key.Modulus.BitLength;

			if (!key.Exponent.IsProbablePrime(2))
			{
				handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
			}

			return key;
		}
	}
}
