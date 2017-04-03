using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
	/// <remarks>Parameters for mask derivation functions.</remarks>
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class MgfParameters
		: IDerivationParameters
    {
        private readonly byte[] seed;

		public MgfParameters(
            byte[] seed)
			: this(seed, 0, seed.Length)
        {
        }

		public MgfParameters(
            byte[]  seed,
            int     off,
            int     len)
        {
            this.seed = new byte[len];
            Array.Copy(seed, off, this.seed, 0, len);
        }

		public byte[] GetSeed()
        {
            return (byte[]) seed.Clone();
        }
    }
}
