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
using Org.BouncyCastle.Asn1.Cms.Ecc;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
	internal class KeyAgreeRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private DerObjectIdentifier			keyAgreementOID;
		private DerObjectIdentifier			keyEncryptionOID;
		private IList					    recipientCerts;
		private AsymmetricCipherKeyPair		senderKeyPair;

		internal KeyAgreeRecipientInfoGenerator()
		{
		}

		internal DerObjectIdentifier KeyAgreementOID
		{
			set { this.keyAgreementOID = value; }
		}

		internal DerObjectIdentifier KeyEncryptionOID
		{
			set { this.keyEncryptionOID = value; }
		}

		internal ICollection RecipientCerts
		{
			set { this.recipientCerts = Platform.CreateArrayList(value); }
		}

		internal AsymmetricCipherKeyPair SenderKeyPair
		{
			set { this.senderKeyPair = value; }
		}

		public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
		{
			byte[] keyBytes = contentEncryptionKey.GetKey();

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
			if (keyAgreementOID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
			{
				try
				{
					IAsymmetricCipherKeyPairGenerator ephemKPG =
						GeneratorUtilities.GetKeyPairGenerator(keyAgreementOID);
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
				keyEncryptionOID,
				DerNull.Instance);
			AlgorithmIdentifier keyEncAlg = new AlgorithmIdentifier(keyAgreementOID, paramSeq);


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
				if (keyAgreementOID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
				{
					recipientPublicParams = new MqvPublicParameters(
						(ECPublicKeyParameters)recipientPublicParams,
						(ECPublicKeyParameters)recipientPublicParams);
				}

				// Use key agreement to choose a wrap key for this recipient
				IBasicAgreement keyAgreement = AgreementUtilities.GetBasicAgreementWithKdf(
					keyAgreementOID, keyEncryptionOID.Id);
				keyAgreement.Init(new ParametersWithRandom(senderPrivateParams, random));
				BigInteger agreedValue = keyAgreement.CalculateAgreement(recipientPublicParams);

				int keyEncryptionKeySize = GeneratorUtilities.GetDefaultKeySize(keyEncryptionOID) / 8;
				byte[] keyEncryptionKeyBytes = X9IntegerConverter.IntegerToBytes(agreedValue, keyEncryptionKeySize);
				KeyParameter keyEncryptionKey = ParameterUtilities.CreateKeyParameter(
					keyEncryptionOID, keyEncryptionKeyBytes);

				// Wrap the content encryption key with the agreement key
				IWrapper keyWrapper = Helper.CreateWrapper(keyEncryptionOID.Id);
				keyWrapper.Init(true, new ParametersWithRandom(keyEncryptionKey, random));
				byte[] encryptedKeyBytes = keyWrapper.Wrap(keyBytes, 0, keyBytes.Length);

	        	Asn1OctetString encryptedKey = new DerOctetString(encryptedKeyBytes);

				recipientEncryptedKeys.Add(new RecipientEncryptedKey(karid, encryptedKey));
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
