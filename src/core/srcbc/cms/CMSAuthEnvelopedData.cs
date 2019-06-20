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
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Cms
{
	/**
	* containing class for an CMS AuthEnveloped Data object
	*/
	internal class CmsAuthEnvelopedData
	{
		internal RecipientInformationStore recipientInfoStore;
		internal ContentInfo contentInfo;

		private OriginatorInfo      originator;
		private AlgorithmIdentifier authEncAlg;
		private Asn1Set             authAttrs;
		private byte[]              mac;
		private Asn1Set             unauthAttrs;
	
		public CmsAuthEnvelopedData(
			byte[] authEnvData)
			: this(CmsUtilities.ReadContentInfo(authEnvData))
		{
		}

		public CmsAuthEnvelopedData(
			Stream authEnvData)
			: this(CmsUtilities.ReadContentInfo(authEnvData))
		{
		}

		public CmsAuthEnvelopedData(
			ContentInfo contentInfo)
		{
			this.contentInfo = contentInfo;

			AuthEnvelopedData authEnvData = AuthEnvelopedData.GetInstance(contentInfo.Content);

			this.originator = authEnvData.OriginatorInfo;

			//
	        // read the recipients
	        //
	        Asn1Set recipientInfos = authEnvData.RecipientInfos;

			//
			// read the auth-encrypted content info
			//
			EncryptedContentInfo authEncInfo = authEnvData.AuthEncryptedContentInfo;
			this.authEncAlg = authEncInfo.ContentEncryptionAlgorithm;
			CmsSecureReadable secureReadable = new AuthEnvelopedSecureReadable(this);

			//
			// build the RecipientInformationStore
			//
			this.recipientInfoStore = CmsEnvelopedHelper.BuildRecipientInformationStore(
				recipientInfos, secureReadable);

			// FIXME These need to be passed to the AEAD cipher as AAD (Additional Authenticated Data)
			this.authAttrs = authEnvData.AuthAttrs;
			this.mac = authEnvData.Mac.GetOctets();
			this.unauthAttrs = authEnvData.UnauthAttrs;
		}

		private class AuthEnvelopedSecureReadable : CmsSecureReadable
		{
			private readonly CmsAuthEnvelopedData parent;

			internal AuthEnvelopedSecureReadable(CmsAuthEnvelopedData parent)
			{
				this.parent = parent;
			}

			public AlgorithmIdentifier Algorithm
			{
				get { return parent.authEncAlg; }
			}

			public object CryptoObject
			{
				get { return null; }
			}

			public CmsReadable GetReadable(KeyParameter key)
			{
				// TODO Create AEAD cipher instance to decrypt and calculate tag ( MAC)
				throw new CmsException("AuthEnveloped data decryption not yet implemented");

//				RFC 5084 ASN.1 Module
//				-- Parameters for AlgorithmIdentifier
//				
//				CCMParameters ::= SEQUENCE {
//				  aes-nonce         OCTET STRING (SIZE(7..13)),
//				  aes-ICVlen        AES-CCM-ICVlen DEFAULT 12 }
//				
//				AES-CCM-ICVlen ::= INTEGER (4 | 6 | 8 | 10 | 12 | 14 | 16)
//				
//				GCMParameters ::= SEQUENCE {
//				  aes-nonce        OCTET STRING, -- recommended size is 12 octets
//				  aes-ICVlen       AES-GCM-ICVlen DEFAULT 12 }
//				
//				AES-GCM-ICVlen ::= INTEGER (12 | 13 | 14 | 15 | 16)
			}            
		}
	}
}
