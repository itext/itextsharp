using System;
using System.IO;

using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms
{
    /**
    * the RecipientInfo class for a recipient who has been sent a message
    * encrypted using a secret key known to the other side.
    */
    public class KekRecipientInformation
        : RecipientInformation
    {
        private KekRecipientInfo info;

		[Obsolete]
		public KekRecipientInformation(
            KekRecipientInfo	info,
            AlgorithmIdentifier	encAlg,
            Stream				data)
			: this(info, encAlg, null, null, data)
		{
		}

		[Obsolete]
		public KekRecipientInformation(
            KekRecipientInfo	info,
            AlgorithmIdentifier	encAlg,
            AlgorithmIdentifier	macAlg,
            Stream				data)
			: this(info, encAlg, macAlg, null, data)
		{
		}

		public KekRecipientInformation(
            KekRecipientInfo	info,
            AlgorithmIdentifier	encAlg,
            AlgorithmIdentifier	macAlg,
            AlgorithmIdentifier	authEncAlg,
            Stream				data)
            : base(encAlg, macAlg, authEncAlg, info.KeyEncryptionAlgorithm, data)
        {
            this.info = info;
            this.rid = new RecipientID();

			KekIdentifier kekId = info.KekID;

			rid.KeyIdentifier = kekId.KeyIdentifier.GetOctets();
        }

		/**
        * decrypt the content and return an input stream.
        */
        public override CmsTypedStream GetContentStream(
            ICipherParameters key)
        {
			try
			{
				byte[] encryptedKey = info.EncryptedKey.GetOctets();
				IWrapper keyWrapper = WrapperUtilities.GetWrapper(keyEncAlg.ObjectID.Id);

				keyWrapper.Init(false, key);

				AlgorithmIdentifier aid = GetActiveAlgID();
				KeyParameter sKey = ParameterUtilities.CreateKeyParameter(
					aid.ObjectID, keyWrapper.Unwrap(encryptedKey, 0, encryptedKey.Length));

				return GetContentFromSessionKey(sKey);
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e)
			{
				throw new CmsException("key invalid in message.", e);
			}
        }
    }
}
