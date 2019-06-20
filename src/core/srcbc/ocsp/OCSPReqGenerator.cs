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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Ocsp
{
	public class OcspReqGenerator
	{
		private IList			list = Platform.CreateArrayList();
		private GeneralName		requestorName = null;
		private X509Extensions	requestExtensions = null;

		private class RequestObject
		{
			internal CertificateID certId;
			internal X509Extensions extensions;

			public RequestObject(
				CertificateID	certId,
				X509Extensions	extensions)
			{
				this.certId = certId;
				this.extensions = extensions;
			}

			public Request ToRequest()
			{
				return new Request(certId.ToAsn1Object(), extensions);
			}
		}

		/**
		 * Add a request for the given CertificateID.
		 *
		 * @param certId certificate ID of interest
		 */
		public void AddRequest(
			CertificateID certId)
		{
			list.Add(new RequestObject(certId, null));
		}

		/**
		 * Add a request with extensions
		 *
		 * @param certId certificate ID of interest
		 * @param singleRequestExtensions the extensions to attach to the request
		 */
		public void AddRequest(
			CertificateID   certId,
			X509Extensions  singleRequestExtensions)
		{
			list.Add(new RequestObject(certId, singleRequestExtensions));
		}

		/**
		* Set the requestor name to the passed in X509Principal
		*
		* @param requestorName a X509Principal representing the requestor name.
		*/
		public void SetRequestorName(
		    X509Name requestorName)
		{
		    try
		    {
		        this.requestorName = new GeneralName(GeneralName.DirectoryName, requestorName);
		    }
		    catch (Exception e)
		    {
		        throw new ArgumentException("cannot encode principal", e);
		    }
		}

		public void SetRequestorName(
			GeneralName requestorName)
		{
			this.requestorName = requestorName;
		}

		public void SetRequestExtensions(
			X509Extensions requestExtensions)
		{
			this.requestExtensions = requestExtensions;
		}

		private OcspReq GenerateRequest(
			DerObjectIdentifier		signingAlgorithm,
			AsymmetricKeyParameter	privateKey,
			X509Certificate[]		chain,
			SecureRandom			random)
		{
			Asn1EncodableVector requests = new Asn1EncodableVector();

			foreach (RequestObject reqObj in list)
			{
				try
				{
					requests.Add(reqObj.ToRequest());
				}
				catch (Exception e)
				{
					throw new OcspException("exception creating Request", e);
				}
			}

			TbsRequest tbsReq = new TbsRequest(requestorName, new DerSequence(requests), requestExtensions);

			ISigner sig = null;
			Signature signature = null;

			if (signingAlgorithm != null)
			{
				if (requestorName == null)
				{
					throw new OcspException("requestorName must be specified if request is signed.");
				}

				try
				{
					sig = SignerUtilities.GetSigner(signingAlgorithm.Id);
					if (random != null)
					{
						sig.Init(true, new ParametersWithRandom(privateKey, random));
					}
					else
					{
						sig.Init(true, privateKey);
					}
				}
				catch (Exception e)
				{
					throw new OcspException("exception creating signature: " + e, e);
				}

				DerBitString bitSig = null;

				try
				{
					byte[] encoded = tbsReq.GetEncoded();
					sig.BlockUpdate(encoded, 0, encoded.Length);

					bitSig = new DerBitString(sig.GenerateSignature());
				}
				catch (Exception e)
				{
					throw new OcspException("exception processing TBSRequest: " + e, e);
				}

				AlgorithmIdentifier sigAlgId = new AlgorithmIdentifier(signingAlgorithm, DerNull.Instance);

				if (chain != null && chain.Length > 0)
				{
					Asn1EncodableVector v = new Asn1EncodableVector();
					try
					{
						for (int i = 0; i != chain.Length; i++)
						{
							v.Add(
								X509CertificateStructure.GetInstance(
									Asn1Object.FromByteArray(chain[i].GetEncoded())));
						}
					}
					catch (IOException e)
					{
						throw new OcspException("error processing certs", e);
					}
					catch (CertificateEncodingException e)
					{
						throw new OcspException("error encoding certs", e);
					}

					signature = new Signature(sigAlgId, bitSig, new DerSequence(v));
				}
				else
				{
					signature = new Signature(sigAlgId, bitSig);
				}
			}

			return new OcspReq(new OcspRequest(tbsReq, signature));
		}

		/**
		 * Generate an unsigned request
		 *
		 * @return the OcspReq
		 * @throws OcspException
		 */
		public OcspReq Generate()
		{
			return GenerateRequest(null, null, null, null);
		}

		public OcspReq Generate(
			string					signingAlgorithm,
			AsymmetricKeyParameter	privateKey,
			X509Certificate[]		chain)
		{
			return Generate(signingAlgorithm, privateKey, chain, null);
		}

		public OcspReq Generate(
			string					signingAlgorithm,
			AsymmetricKeyParameter	privateKey,
			X509Certificate[]		chain,
			SecureRandom			random)
		{
			if (signingAlgorithm == null)
				throw new ArgumentException("no signing algorithm specified");

			try
			{
				DerObjectIdentifier oid = OcspUtilities.GetAlgorithmOid(signingAlgorithm);

				return GenerateRequest(oid, privateKey, chain, random);
			}
			catch (ArgumentException)
			{
				throw new ArgumentException("unknown signing algorithm specified: " + signingAlgorithm);
			}
		}

		/**
		 * Return an IEnumerable of the signature names supported by the generator.
		 *
		 * @return an IEnumerable containing recognised names.
		 */
		public IEnumerable SignatureAlgNames
		{
			get { return OcspUtilities.AlgNames; }
		}
	}
}
