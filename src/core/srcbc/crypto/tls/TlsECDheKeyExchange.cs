/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
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
    internal class TlsECDheKeyExchange : TlsECDHKeyExchange
    {
        internal TlsECDheKeyExchange(TlsClientContext context, KeyExchangeAlgorithm keyExchange)
            : base(context, keyExchange)
        {
        }

        public override void SkipServerKeyExchange()
        {
            throw new TlsFatalAlert(AlertDescription.unexpected_message);
        }

        public override void ProcessServerKeyExchange(Stream input)
        {
            SecurityParameters securityParameters = context.SecurityParameters;

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

                throw new TlsFatalAlert(AlertDescription.handshake_failure);
            }

            byte[] publicBytes = TlsUtilities.ReadOpaque8(sigIn);

            byte[] sigByte = TlsUtilities.ReadOpaque16(input);
            if (!signer.VerifySignature(sigByte))
            {
                throw new TlsFatalAlert(AlertDescription.decrypt_error);
            }

            // TODO Check curve_params not null

            ECPoint Q = curve_params.Curve.DecodePoint(publicBytes);

            this.ecAgreeServerPublicKey = ValidateECPublicKey(new ECPublicKeyParameters(Q, curve_params));
        }
        
        public override void ValidateCertificateRequest(CertificateRequest certificateRequest)
        {
            /*
             * RFC 4492 3. [...] The ECDSA_fixed_ECDH and RSA_fixed_ECDH mechanisms are usable
             * with ECDH_ECDSA and ECDH_RSA. Their use with ECDHE_ECDSA and ECDHE_RSA is
             * prohibited because the use of a long-term ECDH client key would jeopardize the
             * forward secrecy property of these algorithms.
             */
            ClientCertificateType[] types = certificateRequest.CertificateTypes;
            foreach (ClientCertificateType type in types)
            {
                switch (type)
                {
                    case ClientCertificateType.rsa_sign:
                    case ClientCertificateType.dss_sign:
                    case ClientCertificateType.ecdsa_sign:
                        break;
                    default:
                        throw new TlsFatalAlert(AlertDescription.illegal_parameter);
                }
            }
        }
        
        public override void ProcessClientCredentials(TlsCredentials clientCredentials)
        {
            if (clientCredentials is TlsSignerCredentials)
            {
                // OK
            }
            else
            {
                throw new TlsFatalAlert(AlertDescription.internal_error);
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
