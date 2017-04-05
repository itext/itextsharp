using System;
using System.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Base class for a PGP object.</remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public abstract class BcpgObject
    {
        public virtual byte[] GetEncoded()
        {
            MemoryStream bOut = new MemoryStream();
            BcpgOutputStream pOut = new BcpgOutputStream(bOut);

			pOut.WriteObject(this);

			return bOut.ToArray();
        }

		public abstract void Encode(BcpgOutputStream bcpgOut);
    }
}

