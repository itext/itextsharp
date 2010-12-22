using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
    * ECDH key exchange (see RFC 4492)
    */
    internal class TlsECDHKeyExchange : TlsECKeyExchange
    {
        protected bool usingFixedAuthentication;

        internal TlsECDHKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer,
            TlsKeyExchangeAlgorithm keyExchange,
            // TODO Replace with an interface e.g. TlsClientAuth
            Certificate clientCert, AsymmetricKeyParameter clientPrivateKey)
            : base(handler, verifyer, keyExchange, clientCert, clientPrivateKey)
        {
        }

        public void skipServerCertificate()
        {
            handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
        }

        public override void SkipServerKeyExchange()
        {
            // do nothing
        }

        public override void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
        {
            handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
        }

        public override void GenerateClientKeyExchange(Stream output)
        {
            if (usingFixedAuthentication)
            {
                TlsUtilities.WriteUint24(0, output);
            }
            else
            {
                clientEphemeralKeyPair = GenerateECKeyPair(((ECKeyParameters)serverPublicKey).Parameters);
                byte[] keData = ExternalizeKey((ECPublicKeyParameters)clientEphemeralKeyPair.Public);
                TlsUtilities.WriteUint24(keData.Length + 1, output);
                TlsUtilities.WriteOpaque8(keData, output);
            }
        }

        public override byte[] GeneratePremasterSecret()
        {
            ICipherParameters privateKey = null;
            if (usingFixedAuthentication)
            {
                privateKey = this.clientPrivateKey;
            }
            else
            {
                privateKey = clientEphemeralKeyPair.Private;
            }
            return CalculateECDhePreMasterSecret((ECPublicKeyParameters)serverPublicKey, privateKey);
        }

        // TODO
        //public override void ProcessServerCertificateRequest(ClientCertificateType[] certificateTypes,
        //    IList certificateAuthorities)
        //{
        //    usingFixedAuthentication = false;
        //    bool fixedAuthenticationOfferedByServer = IsECDsaFixedOfferedByServer(certificateTypes);
        //    if (fixedAuthenticationOfferedByServer && clientPrivateKey != null
        //        && serverPublicKey != null && serverPublicKey is ECPublicKeyParameters
        //        && clientPrivateKey is ECKeyParameters)
        //    {
        //        ECPublicKeyParameters ecPublicKeyParameters = (ECPublicKeyParameters)serverPublicKey;
        //        ECKeyParameters ecClientPrivateKey = (ECKeyParameters)clientPrivateKey;

        //        if (ecPublicKeyParameters.Parameters.Curve.Equals(ecClientPrivateKey.Parameters.Curve))
        //        {
        //            usingFixedAuthentication = true;
        //        }
        //        // todo RSA_fixed_ECDE
        //    }
        //}

        //public override bool SendCertificateVerify()
        //{
        //    return !usingFixedAuthentication;
        //}

        //protected virtual bool IsECDsaFixedOfferedByServer(ClientCertificateType[] certificateTypes)
        //{
        //    bool fixedAuthenticationOfferedByServer = false;
        //    for (int i = 0; i < certificateTypes.Length; i++)
        //    {
        //        if (certificateTypes[i] == ClientCertificateType.ecdsa_fixed_ecdh)
        //        {
        //            fixedAuthenticationOfferedByServer = true;
        //            break;
        //        }
        //    }
        //    return fixedAuthenticationOfferedByServer;
        //}
    }
}
