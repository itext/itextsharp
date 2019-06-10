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
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
    /// <remarks>
    /// General class for generating a CMS enveloped-data message.
    ///
    /// A simple example of usage.
    ///
    /// <pre>
    ///      CmsEnvelopedDataGenerator  fact = new CmsEnvelopedDataGenerator();
    ///
    ///      fact.AddKeyTransRecipient(cert);
    ///
    ///      CmsEnvelopedData         data = fact.Generate(content, algorithm);
    /// </pre>
    /// </remarks>
    public class CmsEnvelopedDataGenerator
		: CmsEnvelopedGenerator
    {
		public CmsEnvelopedDataGenerator()
        {
        }

		/// <summary>Constructor allowing specific source of randomness</summary>
		/// <param name="rand">Instance of <c>SecureRandom</c> to use.</param>
		public CmsEnvelopedDataGenerator(
			SecureRandom rand)
			: base(rand)
		{
		}

		/// <summary>
		/// Generate an enveloped object that contains a CMS Enveloped Data
		/// object using the passed in key generator.
		/// </summary>
        private CmsEnvelopedData Generate(
            CmsProcessable		content,
            string				encryptionOid,
            CipherKeyGenerator	keyGen)
        {
            AlgorithmIdentifier encAlgId = null;
			KeyParameter encKey;
            Asn1OctetString encContent;

			try
			{
				byte[] encKeyBytes = keyGen.GenerateKey();
				encKey = ParameterUtilities.CreateKeyParameter(encryptionOid, encKeyBytes);

				Asn1Encodable asn1Params = GenerateAsn1Parameters(encryptionOid, encKeyBytes);

				ICipherParameters cipherParameters;
				encAlgId = GetAlgorithmIdentifier(
					encryptionOid, encKey, asn1Params, out cipherParameters);

				IBufferedCipher cipher = CipherUtilities.GetCipher(encryptionOid);
				cipher.Init(true, new ParametersWithRandom(cipherParameters, rand));

				MemoryStream bOut = new MemoryStream();
				CipherStream cOut = new CipherStream(bOut, null, cipher);

				content.Write(cOut);

				cOut.Close();

				encContent = new BerOctetString(bOut.ToArray());
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
				throw new CmsException("exception decoding algorithm parameters.", e);
			}


			Asn1EncodableVector recipientInfos = new Asn1EncodableVector();

            foreach (RecipientInfoGenerator rig in recipientInfoGenerators)
            {
                try
                {
                    recipientInfos.Add(rig.Generate(encKey, rand));
                }
                catch (InvalidKeyException e)
                {
                    throw new CmsException("key inappropriate for algorithm.", e);
                }
                catch (GeneralSecurityException e)
                {
                    throw new CmsException("error making encrypted content.", e);
                }
            }

            EncryptedContentInfo eci = new EncryptedContentInfo(
                CmsObjectIdentifiers.Data,
                encAlgId,
                encContent);

			Asn1Set unprotectedAttrSet = null;
            if (unprotectedAttributeGenerator != null)
            {
                Asn1.Cms.AttributeTable attrTable = unprotectedAttributeGenerator.GetAttributes(Platform.CreateHashtable());

                unprotectedAttrSet = new BerSet(attrTable.ToAsn1EncodableVector());
            }

			ContentInfo contentInfo = new ContentInfo(
                CmsObjectIdentifiers.EnvelopedData,
                new EnvelopedData(null, new DerSet(recipientInfos), eci, unprotectedAttrSet));

            return new CmsEnvelopedData(contentInfo);
        }

		/// <summary>Generate an enveloped object that contains an CMS Enveloped Data object.</summary>
        public CmsEnvelopedData Generate(
            CmsProcessable	content,
            string			encryptionOid)
        {
            try
            {
				CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

				keyGen.Init(new KeyGenerationParameters(rand, keyGen.DefaultStrength));

				return Generate(content, encryptionOid, keyGen);
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("can't find key generation algorithm.", e);
            }
        }

		/// <summary>Generate an enveloped object that contains an CMS Enveloped Data object.</summary>
        public CmsEnvelopedData Generate(
            CmsProcessable  content,
            string          encryptionOid,
            int             keySize)
        {
            try
            {
				CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

				keyGen.Init(new KeyGenerationParameters(rand, keySize));

				return Generate(content, encryptionOid, keyGen);
            }
            catch (SecurityUtilityException e)
            {
                throw new CmsException("can't find key generation algorithm.", e);
            }
        }
    }
}
