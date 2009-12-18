using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
	internal class KeyTransRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private TbsCertificateStructure	recipientTbsCert;
		private AsymmetricKeyParameter	recipientPublicKey;
		private Asn1OctetString			subjectKeyIdentifier;

		// Derived fields
		private SubjectPublicKeyInfo info;
		
		internal KeyTransRecipientInfoGenerator()
		{
		}
		
		internal X509Certificate RecipientCert
		{
			set
			{
				try
				{
					this.recipientTbsCert = TbsCertificateStructure.GetInstance(
						Asn1Object.FromByteArray(value.GetTbsCertificate()));
				}
				catch (Exception)
				{
					throw new ArgumentException("can't extract TBS structure from this cert");
				}
				
				this.recipientPublicKey = value.GetPublicKey();
				this.info = recipientTbsCert.SubjectPublicKeyInfo;
			}
		}
		
		internal AsymmetricKeyParameter RecipientPublicKey
		{
			set
			{
				this.recipientPublicKey = value;

				try
				{
					info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(
						recipientPublicKey);
				}
				catch (IOException)
				{
					throw new ArgumentException("can't extract key algorithm from this key");
				}
			}
		}
		
		internal Asn1OctetString SubjectKeyIdentifier
		{
			set { this.subjectKeyIdentifier = value; }
		}

		public RecipientInfo Generate(KeyParameter key, SecureRandom random)
		{
			byte[] keyBytes = key.GetKey();
			AlgorithmIdentifier keyEncAlg = info.AlgorithmID;

			IWrapper keyWrapper = Helper.CreateWrapper(keyEncAlg.ObjectID.Id);
			keyWrapper.Init(true, new ParametersWithRandom(recipientPublicKey, random));
			Asn1OctetString encKey = new DerOctetString(
				keyWrapper.Wrap(keyBytes, 0, keyBytes.Length));

			RecipientIdentifier recipId;
			if (recipientTbsCert != null)
			{
				IssuerAndSerialNumber issuerAndSerial = new IssuerAndSerialNumber(
					recipientTbsCert.Issuer, recipientTbsCert.SerialNumber.Value);
				recipId = new RecipientIdentifier(issuerAndSerial);
			}
			else
			{
				recipId = new RecipientIdentifier(subjectKeyIdentifier);
			}

			return new RecipientInfo(new KeyTransRecipientInfo(recipId, keyEncAlg, encKey));
		}
	}
}
