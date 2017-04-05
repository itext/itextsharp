using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class MqvPublicParameters
		: ICipherParameters
	{
		private readonly ECPublicKeyParameters staticPublicKey;
		private readonly ECPublicKeyParameters ephemeralPublicKey;

		public MqvPublicParameters(
			ECPublicKeyParameters	staticPublicKey,
			ECPublicKeyParameters	ephemeralPublicKey)
		{
			this.staticPublicKey = staticPublicKey;
			this.ephemeralPublicKey = ephemeralPublicKey;
		}

		public ECPublicKeyParameters StaticPublicKey
		{
			get { return staticPublicKey; }
		}

		public ECPublicKeyParameters EphemeralPublicKey
		{
			get { return ephemeralPublicKey; }
		}
	}
}
