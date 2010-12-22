using System;

using Org.BouncyCastle.Crypto.Signers;

namespace Org.BouncyCastle.Crypto.Tls
{
	internal class TlsDssSigner
		: TlsDsaSigner
	{
	    protected override IDsa CreateDsaImpl()
	    {
			return new DsaSigner();
	    }
	}
}
