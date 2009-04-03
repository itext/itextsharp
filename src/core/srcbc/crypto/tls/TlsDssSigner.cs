using System;

using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsDssSigner
		: DsaDigestSigner
	{
		internal TlsDssSigner()
        	: base(new DsaSigner(), new Sha1Digest())
		{
		}
	}
}
