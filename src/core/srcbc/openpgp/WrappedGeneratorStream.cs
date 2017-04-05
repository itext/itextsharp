using System;
using System.IO;

using Org.BouncyCastle.Asn1.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class WrappedGeneratorStream
		: FilterStream
	{
		private readonly IStreamGenerator gen;

		public WrappedGeneratorStream(
			IStreamGenerator	gen,
			Stream				str)
			: base(str)
		{
			this.gen = gen;
		}

		public override void Close()
		{
			gen.Close();
		}
	}
}
