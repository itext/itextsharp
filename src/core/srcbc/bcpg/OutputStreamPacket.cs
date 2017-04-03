using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class OutputStreamPacket
    {
        private readonly BcpgOutputStream bcpgOut;

		internal OutputStreamPacket(
            BcpgOutputStream bcpgOut)
        {
			if (bcpgOut == null)
				throw new ArgumentNullException("bcpgOut");

			this.bcpgOut = bcpgOut;
        }

		public abstract BcpgOutputStream Open();

		public abstract void Close();
    }
}

