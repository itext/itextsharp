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
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Tsp
{
	/**
	 * Base class for an RFC 3161 Time Stamp Request.
	 */
	public class TimeStampRequest
		: X509ExtensionBase
	{
		private TimeStampReq req;
		private X509Extensions extensions;

		public TimeStampRequest(
			TimeStampReq req)
		{
			this.req = req;
			this.extensions = req.Extensions;
		}

		/**
		* Create a TimeStampRequest from the past in byte array.
		*
		* @param req byte array containing the request.
		* @throws IOException if the request is malformed.
		*/
		public TimeStampRequest(
			byte[] req)
			: this(new Asn1InputStream(req))
		{
		}

		/**
		* Create a TimeStampRequest from the past in input stream.
		*
		* @param in input stream containing the request.
		* @throws IOException if the request is malformed.
		*/
		public TimeStampRequest(
			Stream input)
			: this(new Asn1InputStream(input))
		{
		}

		private TimeStampRequest(
			Asn1InputStream str)
		{
			try
			{
				this.req = TimeStampReq.GetInstance(str.ReadObject());
			}
			catch (InvalidCastException e)
			{
				throw new IOException("malformed request: " + e);
			}
			catch (ArgumentException e)
			{
				throw new IOException("malformed request: " + e);
			}
		}

		public int Version
		{
			get { return req.Version.Value.IntValue; }
		}

		public string MessageImprintAlgOid
		{
			get { return req.MessageImprint.HashAlgorithm.ObjectID.Id; }
		}

		public byte[] GetMessageImprintDigest()
		{
			return req.MessageImprint.GetHashedMessage();
		}

		public string ReqPolicy
		{
			get
			{
				return req.ReqPolicy == null
					?	null
					:	req.ReqPolicy.Id;
			}
		}

		public BigInteger Nonce
		{
			get
			{
				return req.Nonce == null
					?	null
					:	req.Nonce.Value;
			}
		}

		public bool CertReq
		{
			get
			{
				return req.CertReq == null
					?	false
					:	req.CertReq.IsTrue;
			}
		}

		/**
		* Validate the timestamp request, checking the digest to see if it is of an
		* accepted type and whether it is of the correct length for the algorithm specified.
		*
		* @param algorithms a set of string OIDS giving accepted algorithms.
		* @param policies if non-null a set of policies we are willing to sign under.
		* @param extensions if non-null a set of extensions we are willing to accept.
		* @throws TspException if the request is invalid, or processing fails.
		*/
		public void Validate(
			IList algorithms,
			IList policies,
			IList extensions)
		{
			if (!algorithms.Contains(this.MessageImprintAlgOid))
			{
				throw new TspValidationException("request contains unknown algorithm.", PkiFailureInfo.BadAlg);
			}

			if (policies != null && this.ReqPolicy != null && !policies.Contains(this.ReqPolicy))
			{
				throw new TspValidationException("request contains unknown policy.", PkiFailureInfo.UnacceptedPolicy);
			}

			if (this.Extensions != null && extensions != null)
			{
				foreach (DerObjectIdentifier oid in this.Extensions.ExtensionOids)
				{
					if (!extensions.Contains(oid.Id))
					{
						throw new TspValidationException("request contains unknown extension.",
							PkiFailureInfo.UnacceptedExtension);
					}
				}
			}

			int digestLength = TspUtil.GetDigestLength(this.MessageImprintAlgOid);

			if (digestLength != this.GetMessageImprintDigest().Length)
			{
				throw new TspValidationException("imprint digest the wrong length.",
					PkiFailureInfo.BadDataFormat);
			}
		}

		/**
		 * return the ASN.1 encoded representation of this object.
		 */
		public byte[] GetEncoded()
		{
			return req.GetEncoded();
		}

		internal X509Extensions Extensions
		{
			get { return req.Extensions; }
		}
		
		public virtual bool HasExtensions
		{
			get { return extensions != null; }
		}

		public virtual X509Extension GetExtension(DerObjectIdentifier oid)
		{
			return extensions == null ? null : extensions.GetExtension(oid);
		}

		public virtual IList GetExtensionOids()
		{
			return TspUtil.GetExtensionOids(extensions);
		}

		protected override X509Extensions GetX509Extensions()
		{
			return Extensions;
		}
	}
}
