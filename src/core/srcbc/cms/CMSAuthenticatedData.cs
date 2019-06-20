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
	* containing class for an CMS Authenticated Data object
	*/
	public class CmsAuthenticatedData
	{
		internal RecipientInformationStore recipientInfoStore;
		internal ContentInfo contentInfo;

		private AlgorithmIdentifier macAlg;
		private Asn1Set authAttrs;
		private Asn1Set unauthAttrs;
		private byte[] mac;

		public CmsAuthenticatedData(
			byte[] authData)
			: this(CmsUtilities.ReadContentInfo(authData))
		{
		}

		public CmsAuthenticatedData(
			Stream authData)
			: this(CmsUtilities.ReadContentInfo(authData))
		{
		}

		public CmsAuthenticatedData(
			ContentInfo contentInfo)
		{
			this.contentInfo = contentInfo;

			AuthenticatedData authData = AuthenticatedData.GetInstance(contentInfo.Content);

			//
			// read the recipients
			//
			Asn1Set recipientInfos = authData.RecipientInfos;

			this.macAlg = authData.MacAlgorithm;

			//
			// read the authenticated content info
			//
			ContentInfo encInfo = authData.EncapsulatedContentInfo;
			CmsReadable readable = new CmsProcessableByteArray(
				Asn1OctetString.GetInstance(encInfo.Content).GetOctets());
			CmsSecureReadable secureReadable = new CmsEnvelopedHelper.CmsAuthenticatedSecureReadable(
				this.macAlg, readable);

			//
			// build the RecipientInformationStore
			//
			this.recipientInfoStore = CmsEnvelopedHelper.BuildRecipientInformationStore(
				recipientInfos, secureReadable);

			this.authAttrs = authData.AuthAttrs;
			this.mac = authData.Mac.GetOctets();
			this.unauthAttrs = authData.UnauthAttrs;
		}

		public byte[] GetMac()
		{
			return Arrays.Clone(mac);
		}

		public AlgorithmIdentifier MacAlgorithmID
		{
			get { return macAlg; }
		}

		/**
		* return the object identifier for the content MAC algorithm.
		*/
		public string MacAlgOid
		{
			get { return macAlg.ObjectID.Id; }
		}

		/**
		* return a store of the intended recipients for this message
		*/
		public RecipientInformationStore GetRecipientInfos()
		{
			return recipientInfoStore;
		}

		/**
		 * return the ContentInfo 
		 */
		public ContentInfo ContentInfo
		{
			get { return contentInfo; }
		}

		/**
		* return a table of the digested attributes indexed by
		* the OID of the attribute.
		*/
		public Asn1.Cms.AttributeTable GetAuthAttrs()
		{
			if (authAttrs == null)
				return null;

			return new Asn1.Cms.AttributeTable(authAttrs);
		}

		/**
		* return a table of the undigested attributes indexed by
		* the OID of the attribute.
		*/
		public Asn1.Cms.AttributeTable GetUnauthAttrs()
		{
			if (unauthAttrs == null)
				return null;

			return new Asn1.Cms.AttributeTable(unauthAttrs);
		}

		/**
		* return the ASN.1 encoded representation of this object.
		*/
		public byte[] GetEncoded()
		{
			return contentInfo.GetEncoded();
		}
	}
}
