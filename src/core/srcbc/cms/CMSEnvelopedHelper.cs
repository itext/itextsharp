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
	class CmsEnvelopedHelper
	{
		internal static readonly CmsEnvelopedHelper Instance = new CmsEnvelopedHelper();

		private static readonly IDictionary KeySizes = new Hashtable();
		private static readonly IDictionary BaseCipherNames = new Hashtable();

		static CmsEnvelopedHelper()
		{
			KeySizes.Add(CmsEnvelopedGenerator.DesEde3Cbc, 192);
			KeySizes.Add(CmsEnvelopedGenerator.Aes128Cbc, 128);
			KeySizes.Add(CmsEnvelopedGenerator.Aes192Cbc, 192);
			KeySizes.Add(CmsEnvelopedGenerator.Aes256Cbc, 256);

			BaseCipherNames.Add(CmsEnvelopedGenerator.DesEde3Cbc,  "DESEDE");
			BaseCipherNames.Add(CmsEnvelopedGenerator.Aes128Cbc,  "AES");
			BaseCipherNames.Add(CmsEnvelopedGenerator.Aes192Cbc,  "AES");
			BaseCipherNames.Add(CmsEnvelopedGenerator.Aes256Cbc,  "AES");
		}

		private string GetAsymmetricEncryptionAlgName(
			string encryptionAlgOid)
		{
			if (Asn1.Pkcs.PkcsObjectIdentifiers.RsaEncryption.Id.Equals(encryptionAlgOid))
			{
				return "RSA/ECB/PKCS1Padding";
			}

			return encryptionAlgOid;    
		}

		internal IBufferedCipher CreateAsymmetricCipher(
			string encryptionOid)
		{
			string asymName = GetAsymmetricEncryptionAlgName(encryptionOid);
			if (!asymName.Equals(encryptionOid))
			{
				try
				{
					return CipherUtilities.GetCipher(asymName);
				}
				catch (NoSuchAlgorithmException e)
				{
					// Ignore
				}
			}
			return CipherUtilities.GetCipher(encryptionOid);
		}

		internal IWrapper CreateWrapper(
			string encryptionOid)
		{
			try
			{
				return WrapperUtilities.GetWrapper(encryptionOid);
			}
			catch (SecurityUtilityException)
			{
				return WrapperUtilities.GetWrapper(GetAsymmetricEncryptionAlgName(encryptionOid));
			}
		}

		internal string GetRfc3211WrapperName(
			string oid)
		{
			if (oid == null)
				throw new ArgumentNullException("oid");

			string alg = (string) BaseCipherNames[oid];

			if (alg == null)
				throw new ArgumentException("no name for " + oid, "oid");

			return alg + "RFC3211Wrap";
		}

		internal int GetKeySize(
			string oid)
		{
			if (!KeySizes.Contains(oid))
			{
				throw new ArgumentException("no keysize for " + oid, "oid");
			}

			return (int) KeySizes[oid];
		}

		internal static IList ReadRecipientInfos(
			Asn1Set				recipientInfos,
			byte[]				contentOctets,
			AlgorithmIdentifier	encAlg,
			AlgorithmIdentifier	macAlg,
			AlgorithmIdentifier	authEncAlg)
		{
			IList infos = new ArrayList();
			foreach (Asn1Encodable ae in recipientInfos)
            {
                RecipientInfo info = RecipientInfo.GetInstance(ae);
				MemoryStream contentStream = new MemoryStream(contentOctets, false);

				ReadRecipientInfo(infos, info, contentStream, encAlg, macAlg, authEncAlg);
			}
			return infos;
		}
		
		internal static IList ReadRecipientInfos(
			IEnumerable			recipientInfoIter,
			Stream				contentStream,
			AlgorithmIdentifier	encAlg,
			AlgorithmIdentifier	macAlg,
			AlgorithmIdentifier	authEncAlg)
		{
			IList infos = new ArrayList();
			foreach (RecipientInfo info in recipientInfoIter)
			{
				ReadRecipientInfo(infos, info, contentStream, encAlg, macAlg, authEncAlg);
			}
			return infos;
		}

		private static void ReadRecipientInfo(
			IList				infos,
			RecipientInfo		info,
			Stream				contentStream,
			AlgorithmIdentifier	encAlg,
			AlgorithmIdentifier	macAlg,
			AlgorithmIdentifier	authEncAlg)
		{
			Asn1Encodable recipInfo = info.Info;
			if (recipInfo is KeyTransRecipientInfo)
			{
				infos.Add(new KeyTransRecipientInformation(
					(KeyTransRecipientInfo)recipInfo, encAlg, macAlg, authEncAlg, contentStream));
			}
			else if (recipInfo is KekRecipientInfo)
			{
				infos.Add(new KekRecipientInformation(
					(KekRecipientInfo)recipInfo, encAlg, macAlg, authEncAlg, contentStream));
			}
			else if (recipInfo is KeyAgreeRecipientInfo)
			{
				KeyAgreeRecipientInformation.ReadRecipientInfo(infos,
					(KeyAgreeRecipientInfo)recipInfo, encAlg, macAlg, authEncAlg, contentStream);
			}
			else if (recipInfo is PasswordRecipientInfo)
			{
				infos.Add(new PasswordRecipientInformation(
					(PasswordRecipientInfo)recipInfo, encAlg, macAlg, authEncAlg, contentStream));
			}
		}
	}
}
