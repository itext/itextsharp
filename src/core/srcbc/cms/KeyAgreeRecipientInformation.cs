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
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
	/**
	* the RecipientInfo class for a recipient who has been sent a message
	* encrypted using key agreement.
	*/
	public class KeyAgreeRecipientInformation
		: RecipientInformation
	{
		private KeyAgreeRecipientInfo info;
		private Asn1OctetString       encryptedKey;

		internal static void ReadRecipientInfo(IList infos, KeyAgreeRecipientInfo info,
			CmsSecureReadable secureReadable)
		{
			try
			{
				foreach (Asn1Encodable rek in info.RecipientEncryptedKeys)
				{
					RecipientEncryptedKey id = RecipientEncryptedKey.GetInstance(rek.ToAsn1Object());

					RecipientID rid = new RecipientID();

					Asn1.Cms.KeyAgreeRecipientIdentifier karid = id.Identifier;

					Asn1.Cms.IssuerAndSerialNumber iAndSN = karid.IssuerAndSerialNumber;
					if (iAndSN != null)
					{
						rid.Issuer = iAndSN.Name;
						rid.SerialNumber = iAndSN.SerialNumber.Value;
					}
					else
					{
						Asn1.Cms.RecipientKeyIdentifier rKeyID = karid.RKeyID;

						// Note: 'date' and 'other' fields of RecipientKeyIdentifier appear to be only informational 

						rid.SubjectKeyIdentifier = rKeyID.SubjectKeyIdentifier.GetOctets();
					}

					infos.Add(new KeyAgreeRecipientInformation(info, rid, id.EncryptedKey,
						secureReadable));
				}
			}
			catch (IOException e)
			{
				throw new ArgumentException("invalid rid in KeyAgreeRecipientInformation", e);
			}
		}

		internal KeyAgreeRecipientInformation(
			KeyAgreeRecipientInfo	info,
			RecipientID				rid,
			Asn1OctetString			encryptedKey,
			CmsSecureReadable		secureReadable)
			: base(info.KeyEncryptionAlgorithm, secureReadable)
		{
			this.info = info;
			this.rid = rid;
			this.encryptedKey = encryptedKey;
		}

		private AsymmetricKeyParameter GetSenderPublicKey(
			AsymmetricKeyParameter		receiverPrivateKey,
			OriginatorIdentifierOrKey	originator)
		{
			OriginatorPublicKey opk = originator.OriginatorPublicKey;
			if (opk != null)
			{
				return GetPublicKeyFromOriginatorPublicKey(receiverPrivateKey, opk);
			}
			
			OriginatorID origID = new OriginatorID();
			
			Asn1.Cms.IssuerAndSerialNumber iAndSN = originator.IssuerAndSerialNumber;
			if (iAndSN != null)
			{
				origID.Issuer = iAndSN.Name;
				origID.SerialNumber = iAndSN.SerialNumber.Value;
			}
			else
			{
				SubjectKeyIdentifier ski = originator.SubjectKeyIdentifier;

				origID.SubjectKeyIdentifier = ski.GetKeyIdentifier();
			}

			return GetPublicKeyFromOriginatorID(origID);
		}

		private AsymmetricKeyParameter GetPublicKeyFromOriginatorPublicKey(
			AsymmetricKeyParameter	receiverPrivateKey,
			OriginatorPublicKey		originatorPublicKey)
		{
			PrivateKeyInfo privInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(receiverPrivateKey);
			SubjectPublicKeyInfo pubInfo = new SubjectPublicKeyInfo(
				privInfo.AlgorithmID,
				originatorPublicKey.PublicKey.GetBytes());
			return PublicKeyFactory.CreateKey(pubInfo);
		}

		private AsymmetricKeyParameter GetPublicKeyFromOriginatorID(
			OriginatorID origID)
		{
			// TODO Support all alternatives for OriginatorIdentifierOrKey
			// see RFC 3852 6.2.2
			throw new CmsException("No support for 'originator' as IssuerAndSerialNumber or SubjectKeyIdentifier");
		}

		private KeyParameter CalculateAgreedWrapKey(
			string					wrapAlg,
			AsymmetricKeyParameter	senderPublicKey,
			AsymmetricKeyParameter	receiverPrivateKey)
		{
			DerObjectIdentifier agreeAlgID = keyEncAlg.ObjectID;

			ICipherParameters senderPublicParams = senderPublicKey;
			ICipherParameters receiverPrivateParams = receiverPrivateKey;

			if (agreeAlgID.Id.Equals(CmsEnvelopedGenerator.ECMqvSha1Kdf))
			{
				byte[] ukmEncoding = info.UserKeyingMaterial.GetOctets();
				MQVuserKeyingMaterial ukm = MQVuserKeyingMaterial.GetInstance(
					Asn1Object.FromByteArray(ukmEncoding));

				AsymmetricKeyParameter ephemeralKey = GetPublicKeyFromOriginatorPublicKey(
					receiverPrivateKey, ukm.EphemeralPublicKey);

				senderPublicParams = new MqvPublicParameters(
					(ECPublicKeyParameters)senderPublicParams,
					(ECPublicKeyParameters)ephemeralKey);
				receiverPrivateParams = new MqvPrivateParameters(
					(ECPrivateKeyParameters)receiverPrivateParams,
					(ECPrivateKeyParameters)receiverPrivateParams);
			}

			IBasicAgreement agreement = AgreementUtilities.GetBasicAgreementWithKdf(
				agreeAlgID, wrapAlg);
			agreement.Init(receiverPrivateParams);
			BigInteger agreedValue = agreement.CalculateAgreement(senderPublicParams);

			int wrapKeySize = GeneratorUtilities.GetDefaultKeySize(wrapAlg) / 8;
			byte[] wrapKeyBytes = X9IntegerConverter.IntegerToBytes(agreedValue, wrapKeySize);
			return ParameterUtilities.CreateKeyParameter(wrapAlg, wrapKeyBytes);
		}

		private KeyParameter UnwrapSessionKey(
			string			wrapAlg,
			KeyParameter	agreedKey)
		{
			byte[] encKeyOctets = encryptedKey.GetOctets();

			IWrapper keyCipher = WrapperUtilities.GetWrapper(wrapAlg);
			keyCipher.Init(false, agreedKey);
			byte[] sKeyBytes = keyCipher.Unwrap(encKeyOctets, 0, encKeyOctets.Length);
			return ParameterUtilities.CreateKeyParameter(GetContentAlgorithmName(), sKeyBytes);
		}

		internal KeyParameter GetSessionKey(
			AsymmetricKeyParameter receiverPrivateKey)
		{
			try
			{
				string wrapAlg = DerObjectIdentifier.GetInstance(
					Asn1Sequence.GetInstance(keyEncAlg.Parameters)[0]).Id;

				AsymmetricKeyParameter senderPublicKey = GetSenderPublicKey(
					receiverPrivateKey, info.Originator);

				KeyParameter agreedWrapKey = CalculateAgreedWrapKey(wrapAlg,
					senderPublicKey, receiverPrivateKey);

				return UnwrapSessionKey(wrapAlg, agreedWrapKey);
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e)
			{
				throw new CmsException("key invalid in message.", e);
			}
			catch (Exception e)
			{
				throw new CmsException("originator key invalid.", e);
			}
		}

		/**
		* decrypt the content and return an input stream.
		*/
		public override CmsTypedStream GetContentStream(
			ICipherParameters key)
		{
			if (!(key is AsymmetricKeyParameter))
				throw new ArgumentException("KeyAgreement requires asymmetric key", "key");

			AsymmetricKeyParameter receiverPrivateKey = (AsymmetricKeyParameter) key;

			if (!receiverPrivateKey.IsPrivate)
				throw new ArgumentException("Expected private key", "key");

			KeyParameter sKey = GetSessionKey(receiverPrivateKey);

			return GetContentFromSessionKey(sKey);
		}
	}
}
