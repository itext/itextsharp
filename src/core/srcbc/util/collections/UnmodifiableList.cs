using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	public abstract class UnmodifiableList
		: IList
	{
		protected UnmodifiableList()
		{
		}

		public int Add(object o)
		{
			throw new NotSupportedException();
		}
		
		public void Clear()
		{
			throw new NotSupportedException();
		}

		public abstract bool Contains(object o);

		public abstract void CopyTo(Array array, int index);

		public abstract int Count { get; }

		public abstract IEnumerator GetEnumerator();

		public abstract int IndexOf(object o);

		public void Insert(int i, object o)
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

		public void Remove(object o)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int i)
		{
			throw new NotSupportedException();
		}

		public abstract object SyncRoot { get; }
		
		public object this[int i]
		{
			get { return GetValue(i); }
			set { throw new NotSupportedException(); }
		}

		protected abstract object GetValue(int i);
	}
}
