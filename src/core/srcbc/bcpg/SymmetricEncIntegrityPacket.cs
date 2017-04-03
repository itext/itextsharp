using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class SymmetricEncIntegrityPacket
		: InputStreamPacket
	{
		internal readonly int version;

		internal SymmetricEncIntegrityPacket(
			BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
			version = bcpgIn.ReadByte();
        }
    }
}
