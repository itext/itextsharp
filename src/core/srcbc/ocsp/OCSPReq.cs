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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Ocsp
{
	/**
	 * <pre>
	 * OcspRequest     ::=     SEQUENCE {
	 *       tbsRequest                  TBSRequest,
	 *       optionalSignature   [0]     EXPLICIT Signature OPTIONAL }
	 *
	 *   TBSRequest      ::=     SEQUENCE {
	 *       version             [0]     EXPLICIT Version DEFAULT v1,
	 *       requestorName       [1]     EXPLICIT GeneralName OPTIONAL,
	 *       requestList                 SEQUENCE OF Request,
	 *       requestExtensions   [2]     EXPLICIT Extensions OPTIONAL }
	 *
	 *   Signature       ::=     SEQUENCE {
	 *       signatureAlgorithm      AlgorithmIdentifier,
	 *       signature               BIT STRING,
	 *       certs               [0] EXPLICIT SEQUENCE OF Certificate OPTIONAL}
	 *
	 *   Version         ::=             INTEGER  {  v1(0) }
	 *
	 *   Request         ::=     SEQUENCE {
	 *       reqCert                     CertID,
	 *       singleRequestExtensions     [0] EXPLICIT Extensions OPTIONAL }
	 *
	 *   CertID          ::=     SEQUENCE {
	 *       hashAlgorithm       AlgorithmIdentifier,
	 *       issuerNameHash      OCTET STRING, -- Hash of Issuer's DN
	 *       issuerKeyHash       OCTET STRING, -- Hash of Issuers public key
	 *       serialNumber        CertificateSerialNumber }
	 * </pre>
	 */
	public class OcspReq
		: X509ExtensionBase
	{
		private OcspRequest req;

		public OcspReq(
			OcspRequest req)
		{
			this.req = req;
		}

		public OcspReq(
			byte[] req)
			: this(new Asn1InputStream(req))
		{
		}

		public OcspReq(
			Stream inStr)
			: this(new Asn1InputStream(inStr))
		{
		}

		private OcspReq(
			Asn1InputStream aIn)
		{
			try
			{
				this.req = OcspRequest.GetInstance(aIn.ReadObject());
			}
			catch (ArgumentException e)
			{
				throw new IOException("malformed request: " + e.Message);
			}
			catch (InvalidCastException e)
			{
				throw new IOException("malformed request: " + e.Message);
			}
		}

		/**
		 * Return the DER encoding of the tbsRequest field.
		 * @return DER encoding of tbsRequest
		 * @throws OcspException in the event of an encoding error.
		 */
		public byte[] GetTbsRequest()
		{
			try
			{
				return req.TbsRequest.GetEncoded();
			}
			catch (IOException e)
			{
				throw new OcspException("problem encoding tbsRequest", e);
			}
		}

		public int Version
		{
			get { return req.TbsRequest.Version.Value.IntValue + 1; }
		}

		public GeneralName RequestorName
		{
			get { return GeneralName.GetInstance(req.TbsRequest.RequestorName); }
		}

		public Req[] GetRequestList()
		{
			Asn1Sequence seq = req.TbsRequest.RequestList;
			Req[] requests = new Req[seq.Count];

			for (int i = 0; i != requests.Length; i++)
			{
				requests[i] = new Req(Request.GetInstance(seq[i]));
			}

			return requests;
		}

		public X509Extensions RequestExtensions
		{
			get { return X509Extensions.GetInstance(req.TbsRequest.RequestExtensions); }
		}

		protected override X509Extensions GetX509Extensions()
		{
			return RequestExtensions;
		}

		/**
		 * return the object identifier representing the signature algorithm
		 */
		public string SignatureAlgOid
		{
			get
			{
				if (!this.IsSigned)
					return null;

				return req.OptionalSignature.SignatureAlgorithm.ObjectID.Id;
			}
		}

		public byte[] GetSignature()
		{
			if (!this.IsSigned)
				return null;

			return req.OptionalSignature.SignatureValue.GetBytes();
		}

		private IList GetCertList()
		{
			// load the certificates if we have any

			IList certs = Platform.CreateArrayList();
			Asn1Sequence s = req.OptionalSignature.Certs;

			if (s != null)
			{
				foreach (Asn1Encodable ae in s)
				{
					try
					{
						certs.Add(new X509CertificateParser().ReadCertificate(ae.GetEncoded()));
					}
					catch (Exception e)
					{
						throw new OcspException("can't re-encode certificate!", e);
					}
				}
			}

			return certs;
		}

		public X509Certificate[] GetCerts()
		{
			if (!this.IsSigned)
				return null;

			IList certs = this.GetCertList();
            X509Certificate[] result = new X509Certificate[certs.Count];
            for (int i = 0; i < certs.Count; ++i)
            {
                result[i] = (X509Certificate)certs[i];
            }
            return result;
		}

		/**
		 * If the request is signed return a possibly empty CertStore containing the certificates in the
		 * request. If the request is not signed the method returns null.
		 *
		 * @return null if not signed, a CertStore otherwise
		 * @throws OcspException
		 */
		public IX509Store GetCertificates(
			string type)
		{
			if (!this.IsSigned)
				return null;

			try
			{
				return X509StoreFactory.Create(
					"Certificate/" + type,
					new X509CollectionStoreParameters(this.GetCertList()));
			}
			catch (Exception e)
			{
				throw new OcspException("can't setup the CertStore", e);
			}
		}

		/**
		 * Return whether or not this request is signed.
		 *
		 * @return true if signed false otherwise.
		 */
		public bool IsSigned
		{
			get { return req.OptionalSignature != null; }
		}

		/**
		 * Verify the signature against the TBSRequest object we contain.
		 */
		public bool Verify(
			AsymmetricKeyParameter publicKey)
		{
			if (!this.IsSigned)
				throw new OcspException("attempt to Verify signature on unsigned object");

			try
			{
				ISigner signature = SignerUtilities.GetSigner(this.SignatureAlgOid);

				signature.Init(false, publicKey);

				byte[] encoded = req.TbsRequest.GetEncoded();

				signature.BlockUpdate(encoded, 0, encoded.Length);

				return signature.VerifySignature(this.GetSignature());
			}
			catch (Exception e)
			{
				throw new OcspException("exception processing sig: " + e, e);
			}
		}

		/**
		 * return the ASN.1 encoded representation of this object.
		 */
		public byte[] GetEncoded()
		{
			return req.GetEncoded();
		}
	}
}
