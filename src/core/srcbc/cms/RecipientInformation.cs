using System;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Cms
{
    public abstract class RecipientInformation
    {
		internal RecipientID			rid = new RecipientID();
        internal AlgorithmIdentifier	encAlg;
        internal AlgorithmIdentifier	macAlg;
		internal AlgorithmIdentifier	authEncAlg;
        internal AlgorithmIdentifier	keyEncAlg;
        internal Stream					data;

		private MacStream	macStream;
		private byte[]		resultMac;

		internal RecipientInformation(
            AlgorithmIdentifier	encAlg,
            AlgorithmIdentifier	macAlg,
			AlgorithmIdentifier	authEncAlg,
            AlgorithmIdentifier	keyEncAlg,
            Stream				data)
        {
			if (!data.CanRead)
				throw new ArgumentException("Expected input stream", "data");

			this.encAlg = encAlg;
			this.macAlg = macAlg;
			this.authEncAlg = authEncAlg;
            this.keyEncAlg = keyEncAlg;
            this.data = data;
        }

		internal AlgorithmIdentifier GetActiveAlgID()
		{
			if (encAlg != null)
				return encAlg;
			if (macAlg != null)
				return macAlg;
			return authEncAlg;
		}

		public RecipientID RecipientID
        {
			get { return rid; }
        }

		public AlgorithmIdentifier KeyEncryptionAlgorithmID
		{
			get { return keyEncAlg; }
		}

		/**
        * return the object identifier for the key encryption algorithm.
        * 
		* @return OID for key encryption algorithm.
        */
        public string KeyEncryptionAlgOid
        {
			get { return keyEncAlg.ObjectID.Id; }
        }

		/**
        * return the ASN.1 encoded key encryption algorithm parameters, or null if
        * there aren't any.
        * 
		* @return ASN.1 encoding of key encryption algorithm parameters.
        */
		public Asn1Object KeyEncryptionAlgParams
		{
			get
			{
				Asn1Encodable ae = keyEncAlg.Parameters;

				return ae == null ? null : ae.ToAsn1Object();
			}
		}

		internal CmsTypedStream GetContentFromSessionKey(
			KeyParameter sKey)
		{
			try
			{
				Stream content = data;

				if (encAlg != null)
				{
					IBufferedCipher cipher =  CipherUtilities.GetCipher(encAlg.ObjectID);

					Asn1Encodable asn1Enc = encAlg.Parameters;
					Asn1Object asn1Params = asn1Enc == null ? null : asn1Enc.ToAsn1Object();

					ICipherParameters cipherParameters = sKey;

					if (asn1Params != null && !(asn1Params is Asn1Null))
					{
						cipherParameters = ParameterUtilities.GetCipherParameters(
							encAlg.ObjectID, cipherParameters, asn1Params);
					}
					else
					{
						string alg = encAlg.ObjectID.Id;
						if (alg.Equals(CmsEnvelopedDataGenerator.DesEde3Cbc)
							|| alg.Equals(CmsEnvelopedDataGenerator.IdeaCbc)
							|| alg.Equals(CmsEnvelopedDataGenerator.Cast5Cbc))
						{
							cipherParameters = new ParametersWithIV(cipherParameters, new byte[8]);
						}
					}

					cipher.Init(false, cipherParameters);

					content = new CipherStream(content, cipher, null);
				}

				// If authenticated, need to wrap in MacStream to calculate MAC
				if (macAlg != null)
				{
					content = this.macStream = CreateMacStream(macAlg, sKey, content);
				}

				if (authEncAlg != null)
				{
					// TODO Create AEAD cipher instance to decrypt and calculate tag ( MAC)
					throw new CmsException("AuthEnveloped data decryption not yet implemented");

//              RFC 5084 ASN.1 Module
//                -- Parameters for AigorithmIdentifier
//
//                CCMParameters ::= SEQUENCE {
//                  aes-nonce         OCTET STRING (SIZE(7..13)),
//                  aes-ICVlen        AES-CCM-ICVlen DEFAULT 12 }
//
//                AES-CCM-ICVlen ::= INTEGER (4 | 6 | 8 | 10 | 12 | 14 | 16)
//
//                GCMParameters ::= SEQUENCE {
//                  aes-nonce        OCTET STRING, -- recommended size is 12 octets
//                  aes-ICVlen       AES-GCM-ICVlen DEFAULT 12 }
//
//                AES-GCM-ICVlen ::= INTEGER (12 | 13 | 14 | 15 | 16)
				}

				return new CmsTypedStream(content);
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
		}

		private static MacStream CreateMacStream(
			AlgorithmIdentifier	macAlg,
			KeyParameter		sKey,
			Stream				inStream)
//		throws NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeyException, InvalidAlgorithmParameterException, IOException, InvalidParameterSpecException
		{
			IMac mac = MacUtilities.GetMac(macAlg.ObjectID);

			// FIXME Support for MAC algorithm parameters similar to cipher parameters
//			ASN1Object sParams = (ASN1Object)macAlg.getParameters();
//			
//			if (sParams != null && !(sParams instanceof ASN1Null))
//			{
//				AlgorithmParameters params = CMSEnvelopedHelper.INSTANCE.createAlgorithmParameters(macAlg.getObjectId().getId(), provider);
//				
//				params.init(sParams.getEncoded(), "ASN.1");
//				
//				mac.init(sKey, params.getParameterSpec(IvParameterSpec.class));
//			}
//			else
			{
				mac.Init(sKey);
			}

//			Asn1Encodable asn1Enc = macAlg.Parameters;
//			Asn1Object asn1Params = asn1Enc == null ? null : asn1Enc.ToAsn1Object();
//
//			ICipherParameters cipherParameters = sKey;
//
//			if (asn1Params != null && !(asn1Params is Asn1Null))
//			{
//				cipherParameters = ParameterUtilities.GetCipherParameters(
//					macAlg.ObjectID, cipherParameters, asn1Params);
//			}
//			else
//			{
//				string alg = macAlg.ObjectID.Id;
//				if (alg.Equals(CmsEnvelopedDataGenerator.DesEde3Cbc)
//					|| alg.Equals(CmsEnvelopedDataGenerator.IdeaCbc)
//					|| alg.Equals(CmsEnvelopedDataGenerator.Cast5Cbc))
//				{
//					cipherParameters = new ParametersWithIV(cipherParameters, new byte[8]);
//				}
//			}
//
//			mac.Init(cipherParameters);

			return new MacStream(inStream, mac, null);
		}
		
		public byte[] GetContent(
            ICipherParameters key)
        {
            try
            {
                if (data is MemoryStream)
                {
//					data.Reset();
					data.Seek(0L, SeekOrigin.Begin);
                }

				return CmsUtilities.StreamToByteArray(GetContentStream(key).ContentStream);
            }
            catch (IOException e)
            {
                throw new Exception("unable to parse internal stream: " + e);
            }
        }

		/**
		* Return the MAC calculated for the content stream. Note: this call is only meaningful once all
		* the content has been read.
		*
		* @return  byte array containing the mac.
		*/
		public byte[] GetMac()
		{
			if (macStream != null && resultMac == null)
			{
				resultMac = MacUtilities.DoFinal(macStream.ReadMac());
			}

			return Arrays.Clone(resultMac);
		}
		
		public abstract CmsTypedStream GetContentStream(ICipherParameters key);
	}
}
