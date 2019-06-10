/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.Collections;

namespace Org.BouncyCastle.Utilities.Collections
{
	public class LinkedDictionary
		: IDictionary
	{
		internal readonly IDictionary hash = Platform.CreateHashtable();
		internal readonly IList keys = Platform.CreateArrayList();

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

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

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
            get { return Platform.CreateArrayList(keys); }
		}

		public virtual ICollection Values
		{
			// NB: Order has to be the same as for Keys property
			get
			{
                IList values = Platform.CreateArrayList(keys.Count);
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

		public virtual object Current
		{
			get { return Entry; }
		}

		public virtual DictionaryEntry Entry
		{
			get
			{
				object k = CurrentKey;
				return new DictionaryEntry(k, parent.hash[k]);
			}
		}

		public virtual object Key
		{
			get
			{
				return CurrentKey;
			}
		}

		public virtual bool MoveNext()
		{
			if (pos >= parent.keys.Count)
				return false;
			return ++pos < parent.keys.Count;
		}

		public virtual void Reset()
		{
			this.pos = -1;
		}

		public virtual object Value
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
