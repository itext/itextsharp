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
		
		private KekIdentifier	secKeyId;
		// TODO Can get this from wrapKey?		
		private string			wrapAlgorithm;
		private KeyParameter	wrapKey;

		// Derived
		private AlgorithmIdentifier keyEncAlg;

		internal KekRecipientInfoGenerator()
		{
		}

		internal KekIdentifier KekIdentifier
		{
			set { this.secKeyId = value; }
		}

		internal string WrapAlgorithm
		{
			set { this.wrapAlgorithm = value; }
		}

		internal KeyParameter WrapKey
		{
			set
			{
				this.wrapKey = value;
				this.keyEncAlg = DetermineKeyEncAlg(wrapAlgorithm, wrapKey);
			}
		}

		public RecipientInfo Generate(KeyParameter key, SecureRandom random)
		{
			byte[] keyBytes = key.GetKey();

			IWrapper keyWrapper = Helper.CreateWrapper(	keyEncAlg.ObjectID.Id);
			keyWrapper.Init(true, new ParametersWithRandom(wrapKey, random));
        	Asn1OctetString encKey = new DerOctetString(
				keyWrapper.Wrap(keyBytes, 0, keyBytes.Length));

			return new RecipientInfo(new KekRecipientInfo(secKeyId, keyEncAlg, encKey));
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
