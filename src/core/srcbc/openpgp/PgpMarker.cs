using System;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
	/// A PGP marker packet - in general these should be ignored other than where
	/// the idea is to preserve the original input stream.
	/// </remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class PgpMarker
		: PgpObject
    {
        private readonly MarkerPacket p;

		public PgpMarker(
            BcpgInputStream bcpgIn)
        {
            p = (MarkerPacket) bcpgIn.ReadPacket();
        }
	}
}
