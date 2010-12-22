using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	public abstract class UnmodifiableDictionary
		: IDictionary
	{
		protected UnmodifiableDictionary()
		{
		}

		public void Add(object k, object v)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public abstract bool Contains(object k);

		public abstract void CopyTo(Array array, int index);

		public abstract int Count { get; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public abstract IDictionaryEnumerator GetEnumerator();

		public void Remove(object k)
		{
			throw new NotSupportedException();
		}

		public bool IsFixedSize
		{
			get { return true; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public abstract bool IsSynchronized { get; }

		public abstract object SyncRoot { get; }

		public abstract ICollection Keys { get; }

		public abstract ICollection Values { get; }

		public object this[object k]
		{
			get { return GetValue(k); }
			set { throw new NotSupportedException(); }
		}

		protected abstract object GetValue(object k);
	}
}
