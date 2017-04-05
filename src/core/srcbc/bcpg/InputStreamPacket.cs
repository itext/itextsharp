using System;

namespace Org.BouncyCastle.Bcpg
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class InputStreamPacket
        : Packet
    {
        private readonly BcpgInputStream bcpgIn;

		public InputStreamPacket(
            BcpgInputStream bcpgIn)
        {
            this.bcpgIn = bcpgIn;
        }

		/// <summary>Note: you can only read from this once...</summary>
		public BcpgInputStream GetInputStream()
        {
            return bcpgIn;
        }
    }
}
