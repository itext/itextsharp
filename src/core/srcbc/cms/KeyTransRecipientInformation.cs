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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Asn1Pkcs = Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
    /**
    * the KeyTransRecipientInformation class for a recipient who has been sent a secret
    * key encrypted using their public key that needs to be used to
    * extract the message.
    */
    public class KeyTransRecipientInformation
        : RecipientInformation
    {
        private KeyTransRecipientInfo info;

		internal KeyTransRecipientInformation(
			KeyTransRecipientInfo	info,
			CmsSecureReadable		secureReadable)
			: base(info.KeyEncryptionAlgorithm, secureReadable)
		{
            this.info = info;
            this.rid = new RecipientID();

			RecipientIdentifier r = info.RecipientIdentifier;

			try
            {
                if (r.IsTagged)
                {
                    Asn1OctetString octs = Asn1OctetString.GetInstance(r.ID);

					rid.SubjectKeyIdentifier = octs.GetOctets();
                }
                else
                {
                    IssuerAndSerialNumber iAnds = IssuerAndSerialNumber.GetInstance(r.ID);

					rid.Issuer = iAnds.Name;
                    rid.SerialNumber = iAnds.SerialNumber.Value;
                }
            }
            catch (IOException)
            {
                throw new ArgumentException("invalid rid in KeyTransRecipientInformation");
            }
        }

		private string GetExchangeEncryptionAlgorithmName(
			DerObjectIdentifier oid)
		{
			if (Asn1Pkcs.PkcsObjectIdentifiers.RsaEncryption.Equals(oid))
			{
				return "RSA//PKCS1Padding";
			}

			return oid.Id;
		}

		internal KeyParameter UnwrapKey(ICipherParameters key)
		{
			byte[] encryptedKey = info.EncryptedKey.GetOctets();
			string keyExchangeAlgorithm = GetExchangeEncryptionAlgorithmName(keyEncAlg.ObjectID);

			try
			{
				IWrapper keyWrapper = WrapperUtilities.GetWrapper(keyExchangeAlgorithm);
				keyWrapper.Init(false, key);

				// FIXME Support for MAC algorithm parameters similar to cipher parameters
				return ParameterUtilities.CreateKeyParameter(
					GetContentAlgorithmName(), keyWrapper.Unwrap(encryptedKey, 0, encryptedKey.Length));
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e)
			{
				throw new CmsException("key invalid in message.", e);
			}
//			catch (IllegalBlockSizeException e)
			catch (DataLengthException e)
			{
				throw new CmsException("illegal blocksize in message.", e);
			}
//			catch (BadPaddingException e)
			catch (InvalidCipherTextException e)
			{
				throw new CmsException("bad padding in message.", e);
			}
		}
		
		/**
        * decrypt the content and return it as a byte array.
        */
        public override CmsTypedStream GetContentStream(
            ICipherParameters key)
        {
			KeyParameter sKey = UnwrapKey(key);

			return GetContentFromSessionKey(sKey);
		}
    }
}
