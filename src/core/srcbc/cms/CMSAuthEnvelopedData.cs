using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;

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
			// read the encrypted content info
			//
			EncryptedContentInfo authEncInfo = authEnvData.AuthEncryptedContentInfo;

			this.authEncAlg = authEncInfo.ContentEncryptionAlgorithm;

			//
			// load the RecipientInfoStore
			//
			byte[] contentOctets = authEncInfo.EncryptedContent.GetOctets();
			IList infos = CmsEnvelopedHelper.ReadRecipientInfos(
				authEnvData.RecipientInfos, contentOctets, null, null, authEncAlg);
			this.recipientInfoStore = new RecipientInformationStore(infos);

			// FIXME These need to be passed to the AEAD cipher as AAD (Additional Authenticated Data)
			this.authAttrs = authEnvData.AuthAttrs;

			this.mac = authEnvData.Mac.GetOctets();

			this.unauthAttrs = authEnvData.UnauthAttrs;
		}
	}
}
