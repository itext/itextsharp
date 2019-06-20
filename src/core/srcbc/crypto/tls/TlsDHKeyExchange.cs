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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

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
			if (agreementCredentials == null)
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
			return TlsDHUtilities.CalculateDHBasicAgreement(publicKey, privateKey);
		}

		protected virtual AsymmetricCipherKeyPair GenerateDHKeyPair(DHParameters dhParams)
		{
			return TlsDHUtilities.GenerateDHKeyPair(context.SecureRandom, dhParams);
		}

		protected virtual void GenerateEphemeralClientKeyExchange(DHParameters dhParams, Stream output)
		{
			this.dhAgreeClientPrivateKey = TlsDHUtilities.GenerateEphemeralClientKeyExchange(
				context.SecureRandom, dhParams, output);
		}

		protected virtual DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
		{
			return TlsDHUtilities.ValidateDHPublicKey(key);
		}
	}
}
