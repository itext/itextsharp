using System;

using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsRsaSigner
		: GenericSigner
	{
		internal TlsRsaSigner()
        	: base(new Pkcs1Encoding(new RsaBlindedEngine()), new CombinedHash())
		{
		}
	}
}
