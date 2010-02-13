using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Cms.Ecc;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X9;

namespace Org.BouncyCastle.Cms
{
	internal class KeyAgreeRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private DerObjectIdentifier			algorithmOID;
		private AsymmetricCipherKeyPair		senderKeyPair;
		private ArrayList					recipientCerts;
		private DerObjectIdentifier			wrapAlgorithmOID;

		internal KeyAgreeRecipientInfoGenerator()
		{
		}

		internal DerObjectIdentifier AlgorithmOid
		{
			set { this.algorithmOID = value; }
		}

		internal ICollection RecipientCerts
		{
			set { this.recipientCerts = new ArrayList(value); }
		}

		internal AsymmetricCipherKeyPair SenderKeyPair
		{
			set { this.senderKeyPair = value; }
		}

		internal DerObjectIdentifier WrapAlgorithmOid
		{
			set { this.wrapAlgorithmOID = value; }
		}

		public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
		{
			byte[] keyBytes = contentEncryptionKey.GetKey();

			string agreementAlgorithm = algorithmOID.Id;
			string cekWrapAlgorithm = wrapAlgorithmOID.Id;

			AsymmetricKeyParameter senderPublicKey = senderKeyPair.Public;
			ICipherParameters senderPrivateParams = senderKeyPair.Private;


			OriginatorIdentifierOrKey originator;
			try
			{
				originator = new OriginatorIdentifierOrKey(
					CreateOriginatorPublicKey(senderPublicKey));
			}
			catch (IOException e)
			{
				throw new InvalidKeyException("cannot extract originator public key: " + e);
			}


			Asn1OctetString ukm = null;
			if (agreementAlgorithm.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
			{
				try
				{
					IAsymmetricCipherKeyPairGenerator ephemKPG =
						GeneratorUtilities.GetKeyPairGenerator(agreementAlgorithm);
					ephemKPG.Init(
						((ECPublicKeyParameters)senderPublicKey).CreateKeyGenerationParameters(random));

					AsymmetricCipherKeyPair ephemKP = ephemKPG.GenerateKeyPair();

					ukm = new DerOctetString(
						new MQVuserKeyingMaterial(
							CreateOriginatorPublicKey(ephemKP.Public), null));

					senderPrivateParams = new MqvPrivateParameters(
						(ECPrivateKeyParameters)senderPrivateParams,
						(ECPrivateKeyParameters)ephemKP.Private,
						(ECPublicKeyParameters)ephemKP.Public);
				}
				catch (IOException e)
				{
					throw new InvalidKeyException("cannot extract MQV ephemeral public key: " + e);
				}
				catch (SecurityUtilityException e)
				{
					throw new InvalidKeyException("cannot determine MQV ephemeral key pair parameters from public key: " + e);
				}
			}


			DerSequence paramSeq = new DerSequence(
				wrapAlgorithmOID,
				DerNull.Instance);
			AlgorithmIdentifier keyEncAlg = new AlgorithmIdentifier(algorithmOID, paramSeq);


			Asn1EncodableVector recipientEncryptedKeys = new Asn1EncodableVector();
			foreach (X509Certificate recipientCert in recipientCerts)
			{
				TbsCertificateStructure tbsCert;
				try
				{
					tbsCert = TbsCertificateStructure.GetInstance(
						Asn1Object.FromByteArray(recipientCert.GetTbsCertificate()));
				}
				catch (Exception)
				{
					throw new ArgumentException("can't extract TBS structure from certificate");
				}

				// TODO Should there be a SubjectKeyIdentifier-based alternative?
				IssuerAndSerialNumber issuerSerial = new IssuerAndSerialNumber(
					tbsCert.Issuer, tbsCert.SerialNumber.Value);			
				KeyAgreeRecipientIdentifier karid = new KeyAgreeRecipientIdentifier(issuerSerial);

				ICipherParameters recipientPublicParams = recipientCert.GetPublicKey();
				if (agreementAlgorithm.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
				{
					recipientPublicParams = new MqvPublicParameters(
						(ECPublicKeyParameters)recipientPublicParams,
						(ECPublicKeyParameters)recipientPublicParams);
				}

				// Use key agreement to choose a wrap key for this recipient
				IBasicAgreement agreement = AgreementUtilities.GetBasicAgreementWithKdf(
					agreementAlgorithm, cekWrapAlgorithm);
				agreement.Init(new ParametersWithRandom(senderPrivateParams, random));
				BigInteger agreedValue = agreement.CalculateAgreement(recipientPublicParams);

				int wrapKeySize = GeneratorUtilities.GetDefaultKeySize(cekWrapAlgorithm) / 8;
				byte[] wrapKeyBytes = X9IntegerConverter.IntegerToBytes(agreedValue, wrapKeySize);
				KeyParameter wrapKey = ParameterUtilities.CreateKeyParameter(
					cekWrapAlgorithm, wrapKeyBytes);

				// Wrap the content encryption key with the agreement key
				IWrapper keyWrapper = Helper.CreateWrapper(cekWrapAlgorithm);
				keyWrapper.Init(true, new ParametersWithRandom(wrapKey, random));
				byte[] encKeyBytes = keyWrapper.Wrap(keyBytes, 0, keyBytes.Length);

	        	Asn1OctetString encKey = new DerOctetString(encKeyBytes);

				recipientEncryptedKeys.Add(new RecipientEncryptedKey(karid, encKey));
			}

			return new RecipientInfo(new KeyAgreeRecipientInfo(originator, ukm, keyEncAlg,
				new DerSequence(recipientEncryptedKeys)));
		}

		private static OriginatorPublicKey CreateOriginatorPublicKey(
			AsymmetricKeyParameter publicKey)
		{
			SubjectPublicKeyInfo spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
			return new OriginatorPublicKey(
				new AlgorithmIdentifier(spki.AlgorithmID.ObjectID, DerNull.Instance),
				spki.PublicKeyData.GetBytes());
		}
	}
}
