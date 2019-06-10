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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms
{
    /**
    * containing class for an CMS Enveloped Data object
    */
    public class CmsEnvelopedData
    {
        internal RecipientInformationStore	recipientInfoStore;
        internal ContentInfo				contentInfo;

		private AlgorithmIdentifier	encAlg;
        private Asn1Set				unprotectedAttributes;

		public CmsEnvelopedData(
            byte[] envelopedData)
            : this(CmsUtilities.ReadContentInfo(envelopedData))
        {
        }

        public CmsEnvelopedData(
            Stream envelopedData)
            : this(CmsUtilities.ReadContentInfo(envelopedData))
        {
        }

        public CmsEnvelopedData(
            ContentInfo contentInfo)
        {
            this.contentInfo = contentInfo;

			EnvelopedData envData = EnvelopedData.GetInstance(contentInfo.Content);

			//
			// read the recipients
			//
			Asn1Set recipientInfos = envData.RecipientInfos;

			//
			// read the encrypted content info
			//
			EncryptedContentInfo encInfo = envData.EncryptedContentInfo;
			this.encAlg = encInfo.ContentEncryptionAlgorithm;
			CmsReadable readable = new CmsProcessableByteArray(encInfo.EncryptedContent.GetOctets());
			CmsSecureReadable secureReadable = new CmsEnvelopedHelper.CmsEnvelopedSecureReadable(
				this.encAlg, readable);

			//
			// build the RecipientInformationStore
			//
			this.recipientInfoStore = CmsEnvelopedHelper.BuildRecipientInformationStore(
				recipientInfos, secureReadable);

			this.unprotectedAttributes = envData.UnprotectedAttrs;
        }

		public AlgorithmIdentifier EncryptionAlgorithmID
		{
			get { return encAlg; }
		}

		/**
        * return the object identifier for the content encryption algorithm.
        */
        public string EncryptionAlgOid
        {
			get { return encAlg.ObjectID.Id; }
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
        * return a table of the unprotected attributes indexed by
        * the OID of the attribute.
        */
        public Asn1.Cms.AttributeTable GetUnprotectedAttributes()
        {
            if (unprotectedAttributes == null)
                return null;

			return new Asn1.Cms.AttributeTable(unprotectedAttributes);
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
