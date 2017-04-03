using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PgpExperimental
		: PgpObject
	{
		private readonly ExperimentalPacket p;

		public PgpExperimental(
			BcpgInputStream bcpgIn)
		{
			p = (ExperimentalPacket) bcpgIn.ReadPacket();
		}
	}
}
