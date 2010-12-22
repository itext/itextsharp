using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
     * Base class for EC key exchange algorithms (see RFC 4492)
     */
    internal abstract class TlsECKeyExchange
        : TlsKeyExchange
    {
        protected TlsProtocolHandler handler;
        protected ICertificateVerifyer verifyer;
        protected TlsKeyExchangeAlgorithm keyExchange;
        protected TlsSigner tlsSigner;

        protected AsymmetricKeyParameter serverPublicKey;

        protected AsymmetricCipherKeyPair clientEphemeralKeyPair;
        protected ECPublicKeyParameters serverEphemeralPublicKey;

        protected Certificate clientCert;
        protected AsymmetricKeyParameter clientPrivateKey = null;

        internal TlsECKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer,
            TlsKeyExchangeAlgorithm keyExchange,
            // TODO Replace with an interface e.g. TlsClientAuth
            Certificate clientCert, AsymmetricKeyParameter clientPrivateKey)
        {
            switch (keyExchange)
            {
                case TlsKeyExchangeAlgorithm.KE_ECDHE_RSA:
                    this.tlsSigner = new TlsRsaSigner();
                    break;
                case TlsKeyExchangeAlgorithm.KE_ECDHE_ECDSA:
                    this.tlsSigner = new TlsECDsaSigner();
                    break;
                case TlsKeyExchangeAlgorithm.KE_ECDH_RSA:
                case TlsKeyExchangeAlgorithm.KE_ECDH_ECDSA:
                    this.tlsSigner = null;
                    break;
                default:
                    throw new ArgumentException("unsupported key exchange algorithm", "keyExchange");
            }

            this.handler = handler;
            this.verifyer = verifyer;
            this.keyExchange = keyExchange;
            this.clientCert = clientCert;
            this.clientPrivateKey = clientPrivateKey;
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
                case TlsKeyExchangeAlgorithm.KE_ECDH_ECDSA:
                    if (!(this.serverPublicKey is ECPublicKeyParameters))
                    {
                        handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
                    }
                    ValidateKeyUsage(x509Cert, KeyUsage.KeyAgreement);
                    // TODO The algorithm used to sign the certificate should be ECDSA.
                    //x509Cert.getSignatureAlgorithm();
                    break;
                case TlsKeyExchangeAlgorithm.KE_ECDHE_ECDSA:
                    if (!(this.serverPublicKey is ECPublicKeyParameters))
                    {
                        handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
                    }
                    ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
                    break;
                case TlsKeyExchangeAlgorithm.KE_ECDH_RSA:
                    if (!(this.serverPublicKey is ECPublicKeyParameters))
                    {
                        handler.FailWithError(AlertLevel.fatal, AlertDescription.certificate_unknown);
                    }
                    ValidateKeyUsage(x509Cert, KeyUsage.KeyAgreement);
                    // TODO The algorithm used to sign the certificate should be RSA.
                    //x509Cert.getSignatureAlgorithm();
                    break;
                case TlsKeyExchangeAlgorithm.KE_ECDHE_RSA:
                    if (!(this.serverPublicKey is RsaKeyParameters))
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

        public abstract void SkipServerKeyExchange();

        public abstract void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters);

        public abstract void GenerateClientKeyExchange(Stream output);

        public abstract byte[] GeneratePremasterSecret();

        protected virtual AsymmetricCipherKeyPair GenerateECKeyPair(ECDomainParameters parameters)
        {
            ECKeyPairGenerator keyPairGenerator = new ECKeyPairGenerator();
            ECKeyGenerationParameters keyGenerationParameters = new ECKeyGenerationParameters(
                parameters, handler.Random);

            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        protected virtual byte[] ExternalizeKey(ECPublicKeyParameters keyParameters)
        {
            // TODO Potentially would like to be able to get the compressed encoding
            ECPoint ecPoint = keyParameters.Q;
            return ecPoint.GetEncoded();
        }

        protected virtual byte[] CalculateECDhePreMasterSecret(ECPublicKeyParameters publicKey,
            ICipherParameters privateKey)
        {
            ECDHBasicAgreement basicAgreement = new ECDHBasicAgreement();
            basicAgreement.Init(privateKey);
            BigInteger agreement = basicAgreement.CalculateAgreement(publicKey);
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
    }
}
