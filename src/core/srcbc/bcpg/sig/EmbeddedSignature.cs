using System;

namespace Org.BouncyCastle.Bcpg.Sig
{
	/**
	 * Packet embedded signature
	 */
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class EmbeddedSignature
		: SignatureSubpacket
	{
		public EmbeddedSignature(
			bool	critical,
			byte[]	data)
			: base(SignatureSubpacketTag.EmbeddedSignature, critical, data)
		{
		}
	}
}
