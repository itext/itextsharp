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
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Cms
{
	class CmsEnvelopedHelper
	{
		internal static readonly CmsEnvelopedHelper Instance = new CmsEnvelopedHelper();

		private static readonly IDictionary KeySizes = Platform.CreateHashtable();
		private static readonly IDictionary BaseCipherNames = Platform.CreateHashtable();

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
				catch (SecurityUtilityException)
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

		internal static RecipientInformationStore BuildRecipientInformationStore(
			Asn1Set recipientInfos, CmsSecureReadable secureReadable)
		{
			IList infos = Platform.CreateArrayList();
			for (int i = 0; i != recipientInfos.Count; i++)
			{
				RecipientInfo info = RecipientInfo.GetInstance(recipientInfos[i]);

				ReadRecipientInfo(infos, info, secureReadable);
			}
			return new RecipientInformationStore(infos);
		}

		private static void ReadRecipientInfo(
			IList infos, RecipientInfo info, CmsSecureReadable secureReadable)
		{
			Asn1Encodable recipInfo = info.Info;
			if (recipInfo is KeyTransRecipientInfo)
			{
				infos.Add(new KeyTransRecipientInformation((KeyTransRecipientInfo)recipInfo, secureReadable));
			}
			else if (recipInfo is KekRecipientInfo)
			{
				infos.Add(new KekRecipientInformation((KekRecipientInfo)recipInfo, secureReadable));
			}
			else if (recipInfo is KeyAgreeRecipientInfo)
			{
				KeyAgreeRecipientInformation.ReadRecipientInfo(infos, (KeyAgreeRecipientInfo)recipInfo, secureReadable);
			}
			else if (recipInfo is PasswordRecipientInfo)
			{
				infos.Add(new PasswordRecipientInformation((PasswordRecipientInfo)recipInfo, secureReadable));
			}
		}

		internal class CmsAuthenticatedSecureReadable : CmsSecureReadable
		{
			private AlgorithmIdentifier algorithm;
			private IMac mac;
			private CmsReadable readable;

			internal CmsAuthenticatedSecureReadable(AlgorithmIdentifier algorithm, CmsReadable readable)
			{
				this.algorithm = algorithm;
				this.readable = readable;
			}

			public AlgorithmIdentifier Algorithm
			{
				get { return this.algorithm; }
			}

			public object CryptoObject
			{
				get { return this.mac; }
			}

			public CmsReadable GetReadable(KeyParameter sKey)
			{
				string macAlg = this.algorithm.ObjectID.Id;
//				Asn1Object sParams = this.algorithm.Parameters.ToAsn1Object();

				try
				{
					this.mac = MacUtilities.GetMac(macAlg);

					// FIXME Support for MAC algorithm parameters similar to cipher parameters
//						ASN1Object sParams = (ASN1Object)macAlg.getParameters();
//
//						if (sParams != null && !(sParams instanceof ASN1Null))
//						{
//							AlgorithmParameters params = CMSEnvelopedHelper.INSTANCE.createAlgorithmParameters(macAlg.getObjectId().getId(), provider);
//
//							params.init(sParams.getEncoded(), "ASN.1");
//
//							mac.init(sKey, params.getParameterSpec(IvParameterSpec.class));
//						}
//						else
					{
						mac.Init(sKey);
					}

//						Asn1Object asn1Params = asn1Enc == null ? null : asn1Enc.ToAsn1Object();
//
//						ICipherParameters cipherParameters = sKey;
//
//						if (asn1Params != null && !(asn1Params is Asn1Null))
//						{
//							cipherParameters = ParameterUtilities.GetCipherParameters(
//							macAlg.ObjectID, cipherParameters, asn1Params);
//						}
//						else
//						{
//							string alg = macAlg.ObjectID.Id;
//							if (alg.Equals(CmsEnvelopedDataGenerator.DesEde3Cbc)
//								|| alg.Equals(CmsEnvelopedDataGenerator.IdeaCbc)
//								|| alg.Equals(CmsEnvelopedDataGenerator.Cast5Cbc))
//							{
//								cipherParameters = new ParametersWithIV(cipherParameters, new byte[8]);
//							}
//						}
//
//						mac.Init(cipherParameters);
				}
				catch (SecurityUtilityException e)
				{
					throw new CmsException("couldn't create cipher.", e);
				}
				catch (InvalidKeyException e)
				{
					throw new CmsException("key invalid in message.", e);
				}
				catch (IOException e)
				{
					throw new CmsException("error decoding algorithm parameters.", e);
				}

				try
				{
					return new CmsProcessableInputStream(
						new TeeInputStream(
							readable.GetInputStream(),
							new MacOutputStream(this.mac)));
				}
				catch (IOException e)
				{
					throw new CmsException("error reading content.", e);
				}
			}
		}

		internal class CmsEnvelopedSecureReadable : CmsSecureReadable
		{
			private AlgorithmIdentifier algorithm;
			private IBufferedCipher cipher;
			private CmsReadable readable;

			internal CmsEnvelopedSecureReadable(AlgorithmIdentifier algorithm, CmsReadable readable)
			{
				this.algorithm = algorithm;
				this.readable = readable;
			}

			public AlgorithmIdentifier Algorithm
			{
				get { return this.algorithm; }
			}

			public object CryptoObject
			{
				get { return this.cipher; }
			}

			public CmsReadable GetReadable(KeyParameter sKey)
			{
				try
				{
					this.cipher =  CipherUtilities.GetCipher(this.algorithm.ObjectID);

					Asn1Encodable asn1Enc = this.algorithm.Parameters;
					Asn1Object asn1Params = asn1Enc == null ? null : asn1Enc.ToAsn1Object();

					ICipherParameters cipherParameters = sKey;

					if (asn1Params != null && !(asn1Params is Asn1Null))
					{
						cipherParameters = ParameterUtilities.GetCipherParameters(
							this.algorithm.ObjectID, cipherParameters, asn1Params);
					}
					else
					{
						string alg = this.algorithm.ObjectID.Id;
						if (alg.Equals(CmsEnvelopedDataGenerator.DesEde3Cbc)
							|| alg.Equals(CmsEnvelopedDataGenerator.IdeaCbc)
							|| alg.Equals(CmsEnvelopedDataGenerator.Cast5Cbc))
						{
							cipherParameters = new ParametersWithIV(cipherParameters, new byte[8]);
						}
					}

					cipher.Init(false, cipherParameters);
				}
				catch (SecurityUtilityException e)
				{
					throw new CmsException("couldn't create cipher.", e);
				}
				catch (InvalidKeyException e)
				{
					throw new CmsException("key invalid in message.", e);
				}
				catch (IOException e)
				{
					throw new CmsException("error decoding algorithm parameters.", e);
				}

				try
				{
					return new CmsProcessableInputStream(
						new CipherStream(readable.GetInputStream(), cipher, null));
				}
				catch (IOException e)
				{
					throw new CmsException("error reading content.", e);
				}
			}
		}
	}
}