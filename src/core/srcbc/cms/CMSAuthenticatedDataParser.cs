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
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Cms
{
	/**
	* Parsing class for an CMS Authenticated Data object from an input stream.
	* <p>
	* Note: that because we are in a streaming mode only one recipient can be tried and it is important
	* that the methods on the parser are called in the appropriate order.
	* </p>
	* <p>
	* Example of use - assuming the first recipient matches the private key we have.
	* <pre>
	*      CMSAuthenticatedDataParser     ad = new CMSAuthenticatedDataParser(inputStream);
	*
	*      RecipientInformationStore  recipients = ad.getRecipientInfos();
	*
	*      Collection  c = recipients.getRecipients();
	*      Iterator    it = c.iterator();
	*
	*      if (it.hasNext())
	*      {
	*          RecipientInformation   recipient = (RecipientInformation)it.next();
	*
	*          CMSTypedStream recData = recipient.getContentStream(privateKey, "BC");
	*
	*          processDataStream(recData.getContentStream());
	*
	*          if (!Arrays.equals(ad.getMac(), recipient.getMac())
	*          {
	*              System.err.println("Data corrupted!!!!");
	*          }
	*      }
	*  </pre>
	*  Note: this class does not introduce buffering - if you are processing large files you should create
	*  the parser with:
	*  <pre>
	*          CMSAuthenticatedDataParser     ep = new CMSAuthenticatedDataParser(new BufferedInputStream(inputStream, bufSize));
	*  </pre>
	*  where bufSize is a suitably large buffer size.
	* </p>
	*/
	public class CmsAuthenticatedDataParser
		: CmsContentInfoParser
	{
		internal RecipientInformationStore	_recipientInfoStore;
		internal AuthenticatedDataParser	authData;

		private AlgorithmIdentifier			macAlg;
		private byte[]						mac;
		private Asn1.Cms.AttributeTable		authAttrs;
		private Asn1.Cms.AttributeTable		unauthAttrs;

		private bool authAttrNotRead;
		private bool unauthAttrNotRead;

		public CmsAuthenticatedDataParser(
			byte[] envelopedData)
			: this(new MemoryStream(envelopedData, false))
		{
		}

		public CmsAuthenticatedDataParser(
			Stream envelopedData)
			: base(envelopedData)
		{
			this.authAttrNotRead = true;
			this.authData = new AuthenticatedDataParser(
				(Asn1SequenceParser)contentInfo.GetContent(Asn1Tags.Sequence));

			// TODO Validate version?
			//DerInteger version = this.authData.getVersion();

			//
			// read the recipients
			//
			Asn1Set recipientInfos = Asn1Set.GetInstance(authData.GetRecipientInfos().ToAsn1Object());

			this.macAlg = authData.GetMacAlgorithm();

			//
			// read the authenticated content info
			//
			ContentInfoParser data = authData.GetEnapsulatedContentInfo();
			CmsReadable readable = new CmsProcessableInputStream(
				((Asn1OctetStringParser)data.GetContent(Asn1Tags.OctetString)).GetOctetStream());
			CmsSecureReadable secureReadable = new CmsEnvelopedHelper.CmsAuthenticatedSecureReadable(
				this.macAlg, readable);

			//
			// build the RecipientInformationStore
			//
			this._recipientInfoStore = CmsEnvelopedHelper.BuildRecipientInformationStore(
				recipientInfos, secureReadable);
		}

		public AlgorithmIdentifier MacAlgorithmID
		{
			get { return macAlg; }
		}

		/**
		* return the object identifier for the mac algorithm.
		*/
		public string MacAlgOid
		{
			get { return macAlg.ObjectID.Id; }
		}


		/**
		 * return the ASN.1 encoded encryption algorithm parameters, or null if
		 * there aren't any.
		 */
		public Asn1Object MacAlgParams
		{
			get
			{
				Asn1Encodable ae = macAlg.Parameters;

				return ae == null ? null : ae.ToAsn1Object();
			}
		}

		/**
		* return a store of the intended recipients for this message
		*/
		public RecipientInformationStore GetRecipientInfos()
		{
			return _recipientInfoStore;
		}

		public byte[] GetMac()
		{
			if (mac == null)
			{
				GetAuthAttrs();
				mac = authData.GetMac().GetOctets();
			}
			return Arrays.Clone(mac);
		}

		/**
		* return a table of the unauthenticated attributes indexed by
		* the OID of the attribute.
		* @exception java.io.IOException
		*/
		public Asn1.Cms.AttributeTable GetAuthAttrs()
		{
			if (authAttrs == null && authAttrNotRead)
			{
				Asn1SetParser s = authData.GetAuthAttrs();

				authAttrNotRead = false;

				if (s != null)
				{
					Asn1EncodableVector v = new Asn1EncodableVector();

					IAsn1Convertible o;
					while ((o = s.ReadObject()) != null)
					{
						Asn1SequenceParser seq = (Asn1SequenceParser)o;

						v.Add(seq.ToAsn1Object());
					}

					authAttrs = new Asn1.Cms.AttributeTable(new DerSet(v));
				}
			}

			return authAttrs;
		}

		/**
		* return a table of the unauthenticated attributes indexed by
		* the OID of the attribute.
		* @exception java.io.IOException
		*/
		public Asn1.Cms.AttributeTable GetUnauthAttrs()
		{
			if (unauthAttrs == null && unauthAttrNotRead)
			{
				Asn1SetParser s = authData.GetUnauthAttrs();

				unauthAttrNotRead = false;

				if (s != null)
				{
					Asn1EncodableVector v = new Asn1EncodableVector();

					IAsn1Convertible o;
					while ((o = s.ReadObject()) != null)
					{
						Asn1SequenceParser seq = (Asn1SequenceParser)o;

						v.Add(seq.ToAsn1Object());
					}

					unauthAttrs = new Asn1.Cms.AttributeTable(new DerSet(v));
				}
			}

			return unauthAttrs;
		}
	}
}
