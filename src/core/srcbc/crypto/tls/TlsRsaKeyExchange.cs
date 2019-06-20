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
		protected TlsClientContext context;

		protected AsymmetricKeyParameter serverPublicKey = null;

        protected RsaKeyParameters rsaServerPublicKey = null;

        protected byte[] premasterSecret;

		internal TlsRsaKeyExchange(TlsClientContext context)
		{
			this.context = context;
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
//			catch (RuntimeException)
			catch (Exception)
			{
				throw new TlsFatalAlert(AlertDescription.unsupported_certificate);
			}

			// Sanity check the PublicKeyFactory
			if (this.serverPublicKey.IsPrivate)
			{
				throw new TlsFatalAlert(AlertDescription.internal_error);
			}

			this.rsaServerPublicKey = ValidateRsaPublicKey((RsaKeyParameters)this.serverPublicKey);

			TlsUtilities.ValidateKeyUsage(x509Cert, KeyUsage.KeyEncipherment);

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
					case ClientCertificateType.ecdsa_sign:
						break;
					default:
						throw new TlsFatalAlert(AlertDescription.illegal_parameter);
				}
			}
		}

		public virtual void SkipClientCredentials()
		{
			// OK
		}

		public virtual void ProcessClientCredentials(TlsCredentials clientCredentials)
		{
			if (!(clientCredentials is TlsSignerCredentials))
			{
				throw new TlsFatalAlert(AlertDescription.internal_error);
			}
		}
		
        public virtual void GenerateClientKeyExchange(Stream output)
		{
			this.premasterSecret = TlsRsaUtilities.GenerateEncryptedPreMasterSecret(
				context.SecureRandom, this.rsaServerPublicKey, output);
		}

		public virtual byte[] GeneratePremasterSecret()
		{
			byte[] tmp = this.premasterSecret;
			this.premasterSecret = null;
			return tmp;
		}

    	// Would be needed to process RSA_EXPORT server key exchange
//	    protected virtual void ProcessRsaServerKeyExchange(Stream input, ISigner signer)
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
//	                handler.FailWithError(AlertLevel.fatal, AlertDescription.decrypt_error);
//	            }
//	        }
//
//	        BigInteger modulus = new BigInteger(1, modulusBytes);
//	        BigInteger exponent = new BigInteger(1, exponentBytes);
//
//	        this.rsaServerPublicKey = ValidateRSAPublicKey(new RsaKeyParameters(false, modulus, exponent));
//	    }

        protected virtual RsaKeyParameters ValidateRsaPublicKey(RsaKeyParameters key)
		{
			// TODO What is the minimum bit length required?
//			key.Modulus.BitLength;

			if (!key.Exponent.IsProbablePrime(2))
			{
				throw new TlsFatalAlert(AlertDescription.illegal_parameter);
			}

			return key;
		}
	}
}
