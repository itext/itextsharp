using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public sealed class EnumerableProxy
		: IEnumerable
	{
		private readonly IEnumerable inner;

		public EnumerableProxy(
			IEnumerable inner)
		{
			if (inner == null)
				throw new ArgumentNullException("inner");

			this.inner = inner;
		}

		public IEnumerator GetEnumerator()
		{
			return inner.GetEnumerator();
		}
	}
}
