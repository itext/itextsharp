using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public sealed class EmptyEnumerable
		: IEnumerable
	{
		public static readonly IEnumerable Instance = new EmptyEnumerable();

		private EmptyEnumerable()
		{
		}

		public IEnumerator GetEnumerator()
		{
			return EmptyEnumerator.Instance;
		}
	}

	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public sealed class EmptyEnumerator
		: IEnumerator
	{
		public static readonly IEnumerator Instance = new EmptyEnumerator();

		private EmptyEnumerator()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}

		public object Current
		{
			get { throw new InvalidOperationException("No elements"); }
		}
	}
}
