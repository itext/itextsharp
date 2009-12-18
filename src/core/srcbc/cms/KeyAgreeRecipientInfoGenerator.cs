using System;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
	internal class KeyAgreeRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private DerObjectIdentifier			algorithmOID;
		private OriginatorIdentifierOrKey	originator;
		private TbsCertificateStructure		recipientTbsCert;
		private Asn1OctetString				ukm;
		private DerObjectIdentifier			wrapAlgorithmOID;
		private KeyParameter				wrapKey;

		internal KeyAgreeRecipientInfoGenerator()
		{
		}
		
		internal DerObjectIdentifier AlgorithmOid
		{
			set { this.algorithmOID = value; }
		}
		
		internal OriginatorIdentifierOrKey Originator
		{
			set { this.originator = value; }
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
			}
		}

		internal Asn1OctetString UKM
		{
			set { this.ukm = value; }
		}

		internal DerObjectIdentifier WrapAlgorithmOid
		{
			set { this.wrapAlgorithmOID = value; }
		}

		internal KeyParameter WrapKey
		{
			set { this.wrapKey = value; }
		}

		public RecipientInfo Generate(KeyParameter key, SecureRandom random)
		{
			byte[] keyBytes = key.GetKey();

			DerSequence paramSeq = new DerSequence(
				wrapAlgorithmOID,
				DerNull.Instance);
			AlgorithmIdentifier keyEncAlg = new AlgorithmIdentifier(algorithmOID, paramSeq);

 			IssuerAndSerialNumber issuerSerial = new IssuerAndSerialNumber(
				recipientTbsCert.Issuer, recipientTbsCert.SerialNumber.Value);			

			IWrapper keyWrapper = Helper.CreateWrapper(wrapAlgorithmOID.Id);
			keyWrapper.Init(true, new ParametersWithRandom(wrapKey, random));
        	Asn1OctetString encKey = new DerOctetString(
				keyWrapper.Wrap(keyBytes, 0, keyBytes.Length));

			RecipientEncryptedKey rKey = new RecipientEncryptedKey(
				new KeyAgreeRecipientIdentifier(issuerSerial),
				encKey);

			return new RecipientInfo(new KeyAgreeRecipientInfo(originator, ukm,
				keyEncAlg, new DerSequence(rKey)));
		}
	}
}
