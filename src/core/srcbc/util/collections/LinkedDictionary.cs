using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	public class LinkedDictionary
		: IDictionary
	{
		internal readonly IDictionary hash = new Hashtable();
		internal readonly IList keys = new ArrayList();

		public LinkedDictionary()
		{
		}

		public virtual void Add(object k, object v)
		{
			hash.Add(k, v);
			keys.Add(k);
		}

		public virtual void Clear()
		{
			hash.Clear();
			keys.Clear();
		}

		public virtual bool Contains(object k)
		{
			return hash.Contains(k);
		}

		public virtual void CopyTo(Array array, int index)
		{
			foreach (object k in keys)
			{
				array.SetValue(hash[k], index++);
			}
		}

		public virtual int Count
		{
			get { return hash.Count; }
		}

#if NET_1_1
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
#endif

		public virtual IDictionaryEnumerator GetEnumerator()
		{
			return new LinkedDictionaryEnumerator(this);
		}

		public virtual void Remove(object k)
		{
			hash.Remove(k);
			keys.Remove(k);
		}

		public virtual bool IsFixedSize
		{
			get { return false; }
		}

		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		public virtual bool IsSynchronized
		{
			get { return false; }
		}

		public virtual object SyncRoot
		{
			get { return false; }
		}

		public virtual ICollection Keys
		{
			get { return new ArrayList(keys); }
		}

		public virtual ICollection Values
		{
			// NB: Order has to be the same as for Keys property
			get
			{
				ArrayList values = new ArrayList(keys.Count);
				foreach (object k in keys)
				{
					values.Add(hash[k]);
				}
				return values;
			}
		}

		public virtual object this[object k]
		{
			get
			{
				return hash[k];
			}
			set
			{
				if (!hash.Contains(k))
					keys.Add(k);
				hash[k] = value;
			}
		}
	}

	internal class LinkedDictionaryEnumerator : IDictionaryEnumerator
	{
		private readonly LinkedDictionary parent;
		private int pos = -1;

		internal LinkedDictionaryEnumerator(LinkedDictionary parent)
		{
			this.parent = parent;
		}

		public object Current
		{
			get { return Entry; }
		}

		public DictionaryEntry Entry
		{
			get
			{
				object k = CurrentKey;
				return new DictionaryEntry(k, parent.hash[k]);
			}
		}

		public object Key
		{
			get
			{
				return CurrentKey;
			}
		}

		public bool MoveNext()
		{
			if (pos >= parent.keys.Count)
				return false;
			return ++pos < parent.keys.Count;
		}

		public void Reset()
		{
			this.pos = -1;
		}

		public object Value
		{
			get
			{
				return parent.hash[CurrentKey];
			}
		}

		private object CurrentKey
		{
			get
			{
				if (pos < 0 || pos >= parent.keys.Count)
					throw new InvalidOperationException();
				return parent.keys[pos];
			}
		}
	}
}
