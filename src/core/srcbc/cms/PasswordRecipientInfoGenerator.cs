using System;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms
{
	internal class PasswordRecipientInfoGenerator : RecipientInfoGenerator
	{
		private static readonly CmsEnvelopedHelper Helper = CmsEnvelopedHelper.Instance;

		private AlgorithmIdentifier	derivationAlg;
		// TODO Can get this from wrapKey?		
		private string			wrapAlgorithm;
		private KeyParameter	wrapKey;

		internal PasswordRecipientInfoGenerator()
		{
		}

		internal AlgorithmIdentifier DerivationAlg
		{
			set { this.derivationAlg = value; }
		}

		internal string WrapAlgorithm
		{
			set { this.wrapAlgorithm = value; }
		}

		internal KeyParameter WrapKey
		{
			set { this.wrapKey = value; }
		}

		public RecipientInfo Generate(KeyParameter key, SecureRandom random)
		{
			byte[] keyBytes = key.GetKey();

			string rfc3211WrapperName = Helper.GetRfc3211WrapperName(wrapAlgorithm);
			IWrapper keyWrapper = Helper.CreateWrapper(rfc3211WrapperName);

			// Note: In Java build, the IV is automatically generated in JCE layer
			int ivLength = rfc3211WrapperName.StartsWith("DESEDE") ? 8 : 16;
			byte[] iv = new byte[ivLength];
			random.NextBytes(iv);

			ICipherParameters parameters = new ParametersWithIV(wrapKey, iv);
			keyWrapper.Init(true, new ParametersWithRandom(parameters, random));
        	Asn1OctetString encKey = new DerOctetString(
				keyWrapper.Wrap(keyBytes, 0, keyBytes.Length));

			DerSequence seq = new DerSequence(
				new DerObjectIdentifier(wrapAlgorithm),
				new DerOctetString(iv));

			AlgorithmIdentifier keyEncAlg = new AlgorithmIdentifier(
				PkcsObjectIdentifiers.IdAlgPwriKek, seq);

			return new RecipientInfo(new PasswordRecipientInfo(
				derivationAlg, keyEncAlg, encKey));
		}
	}
}
