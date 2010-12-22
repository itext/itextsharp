using System;

using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsECDsaSigner
		: TlsDsaSigner
	{
	    protected override IDsa CreateDsaImpl()
	    {
			return new ECDsaSigner();
	    }
	}
}
