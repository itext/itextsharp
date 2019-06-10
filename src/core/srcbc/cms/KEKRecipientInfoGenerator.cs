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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms
{
	internal class KekRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private KeyParameter	keyEncryptionKey;
		// TODO Can get this from keyEncryptionKey?		
		private string			keyEncryptionKeyOID;
		private KekIdentifier	kekIdentifier;

		// Derived
		private AlgorithmIdentifier keyEncryptionAlgorithm;

		internal KekRecipientInfoGenerator()
		{
		}

		internal KekIdentifier KekIdentifier
		{
			set { this.kekIdentifier = value; }
		}

		internal KeyParameter KeyEncryptionKey
		{
			set
			{
				this.keyEncryptionKey = value;
				this.keyEncryptionAlgorithm = DetermineKeyEncAlg(keyEncryptionKeyOID, keyEncryptionKey);
			}
		}

		internal string KeyEncryptionKeyOID
		{
			set { this.keyEncryptionKeyOID = value; }
		}

		public RecipientInfo Generate(KeyParameter contentEncryptionKey, SecureRandom random)
		{
			byte[] keyBytes = contentEncryptionKey.GetKey();

			IWrapper keyWrapper = Helper.CreateWrapper(keyEncryptionAlgorithm.ObjectID.Id);
			keyWrapper.Init(true, new ParametersWithRandom(keyEncryptionKey, random));
        	Asn1OctetString encryptedKey = new DerOctetString(
				keyWrapper.Wrap(keyBytes, 0, keyBytes.Length));

			return new RecipientInfo(new KekRecipientInfo(kekIdentifier, keyEncryptionAlgorithm, encryptedKey));
		}

		private static AlgorithmIdentifier DetermineKeyEncAlg(
			string algorithm, KeyParameter key)
		{
			if (algorithm.StartsWith("DES"))
			{
				return new AlgorithmIdentifier(
					PkcsObjectIdentifiers.IdAlgCms3DesWrap,
					DerNull.Instance);
			}
			else if (algorithm.StartsWith("RC2"))
			{
				return new AlgorithmIdentifier(
					PkcsObjectIdentifiers.IdAlgCmsRC2Wrap,
					new DerInteger(58));
			}
			else if (algorithm.StartsWith("AES"))
			{
				int length = key.GetKey().Length * 8;
				DerObjectIdentifier wrapOid;

				if (length == 128)
				{
					wrapOid = NistObjectIdentifiers.IdAes128Wrap;
				}
				else if (length == 192)
				{
					wrapOid = NistObjectIdentifiers.IdAes192Wrap;
				}
				else if (length == 256)
				{
					wrapOid = NistObjectIdentifiers.IdAes256Wrap;
				}
				else
				{
					throw new ArgumentException("illegal keysize in AES");
				}

				return new AlgorithmIdentifier(wrapOid);  // parameters absent
			}
			else if (algorithm.StartsWith("SEED"))
			{
				// parameters absent
				return new AlgorithmIdentifier(KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap);
			}
			else if (algorithm.StartsWith("CAMELLIA"))
			{
				int length = key.GetKey().Length * 8;
				DerObjectIdentifier wrapOid;

				if (length == 128)
				{
					wrapOid = NttObjectIdentifiers.IdCamellia128Wrap;
				}
				else if (length == 192)
				{
					wrapOid = NttObjectIdentifiers.IdCamellia192Wrap;
				}
				else if (length == 256)
				{
					wrapOid = NttObjectIdentifiers.IdCamellia256Wrap;
				}
				else
				{
					throw new ArgumentException("illegal keysize in Camellia");
				}

				return new AlgorithmIdentifier(wrapOid); // parameters must be absent
			}
			else
			{
				throw new ArgumentException("unknown algorithm");
			}
		}
	}
}
