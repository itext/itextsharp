using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;

namespace Org.BouncyCastle.Crypto.Tls
{
    /**
    * ECDHE key exchange (see RFC 4492)
    */
    internal class TlsECDheKeyExchange : TlsECKeyExchange
    {
        internal TlsECDheKeyExchange(TlsProtocolHandler handler, ICertificateVerifyer verifyer,
            TlsKeyExchangeAlgorithm keyExchange,
            // TODO Replace with an interface e.g. TlsClientAuth
            Certificate clientCert, AsymmetricKeyParameter clientPrivateKey)
            : base(handler, verifyer, keyExchange, clientCert, clientPrivateKey)
        {
        }

        public override void SkipServerCertificate()
        {
            handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
        }

        public override void SkipServerKeyExchange()
        {
            handler.FailWithError(AlertLevel.fatal, AlertDescription.unexpected_message);
        }

        public override void ProcessServerKeyExchange(Stream input, SecurityParameters securityParameters)
        {
            ISigner signer = InitSigner(tlsSigner, securityParameters);
            Stream sigIn = new SignerStream(input, signer, null);

            ECCurveType curveType = (ECCurveType)TlsUtilities.ReadUint8(sigIn);
            ECDomainParameters curve_params;

            //  Currently, we only support named curves
            if (curveType == ECCurveType.named_curve)
            {
                NamedCurve namedCurve = (NamedCurve)TlsUtilities.ReadUint16(sigIn);

                // TODO Check namedCurve is one we offered?

                curve_params = NamedCurveHelper.GetECParameters(namedCurve);
            }
            else
            {
                // TODO Add support for explicit curve parameters (read from sigIn)

                handler.FailWithError(AlertLevel.fatal, AlertDescription.handshake_failure);
                return;
            }

            byte[] publicBytes = TlsUtilities.ReadOpaque8(sigIn);

            byte[] sigByte = TlsUtilities.ReadOpaque16(input);
            if (!signer.VerifySignature(sigByte))
            {
                handler.FailWithError(AlertLevel.fatal, AlertDescription.bad_certificate);
            }

            // TODO Check curve_params not null

            ECPoint Q = curve_params.Curve.DecodePoint(publicBytes);

            serverEphemeralPublicKey = new ECPublicKeyParameters(Q, curve_params);
        }

        public override void GenerateClientKeyExchange(Stream output)
        {
            clientEphemeralKeyPair = GenerateECKeyPair(serverEphemeralPublicKey.Parameters);
            byte[] keData = ExternalizeKey((ECPublicKeyParameters)clientEphemeralKeyPair.Public);
            TlsUtilities.WriteUint24(keData.Length + 1, output);
            TlsUtilities.WriteOpaque8(keData, output);
        }

        public override byte[] GeneratePremasterSecret()
        {
            return CalculateECDhePreMasterSecret((ECPublicKeyParameters)serverEphemeralPublicKey,
            clientEphemeralKeyPair.Private);
        }

        protected virtual ISigner InitSigner(TlsSigner tlsSigner, SecurityParameters securityParameters)
        {
            ISigner signer = tlsSigner.CreateVerifyer(this.serverPublicKey);
            signer.BlockUpdate(securityParameters.clientRandom, 0, securityParameters.clientRandom.Length);
            signer.BlockUpdate(securityParameters.serverRandom, 0, securityParameters.serverRandom.Length);
            return signer;
        }

        //public override void ProcessServerCertificateRequest(ClientCertificateType[] certificateTypes,
        //    IList certificateAuthorities)
        //{
        //}

        //public bool SendCertificateVerify()
        //{
        //    return true;
        //}
    }
}
