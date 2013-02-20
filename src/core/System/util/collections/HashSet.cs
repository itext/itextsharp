using System.Collections;
using System.Collections.Generic;


namespace System.util.collections
{
    public class HashSet<T>: ICollection<T>
    {
        private Dictionary<T, object> set;

        public HashSet()
        {
            set = new Dictionary<T, object>();
        }

        public HashSet(IEnumerable<T> set)
        {
            foreach (T item in set)
                Add(item);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return set.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            set[item] = null;
        }

        public void Clear()
        {
            set.Clear();
        }

        public bool Contains(T item)
        {
            return set.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            set.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return set.Remove(item);
        }

        public int Count
        {
            get { return set.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}