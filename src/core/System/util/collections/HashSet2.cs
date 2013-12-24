using System.Collections;
using System.Collections.Generic;


namespace System.util.collections
{
    public class HashSet2<T>: ICollection<T>
    {
        private Dictionary<T, object> set;

        public HashSet2()
        {
            set = new Dictionary<T, object>();
        }

        public HashSet2(IEnumerable<T> set) : this()
        {
            foreach (T item in set)
                Add(item);
        }
        
        virtual public IEnumerator<T> GetEnumerator()
        {
            return set.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        virtual public void Add(T item)
        {
            set[item] = null;
        }

        virtual public void Clear()
        {
            set.Clear();
        }

        virtual public bool Contains(T item)
        {
            return set.ContainsKey(item);
        }

        virtual public void CopyTo(T[] array, int arrayIndex)
        {
            set.Keys.CopyTo(array, arrayIndex);
        }

        virtual public bool Remove(T item)
        {
            return set.Remove(item);
        }

        virtual public int Count
        {
            get { return set.Count; }
        }
        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        virtual public bool RetainAll(ICollection<T> collection) {
	        bool modified = false;
            List<T> toRemove = new List<T>();
            foreach (T item in this)
                if (!collection.Contains(item))
                    toRemove.Add(item);

            foreach (T item in toRemove) {
                Remove(item);
                modified = true;
            }
	        return modified;
        }
//        public boolean retainAll(Collection<?> c) {
//	        boolean modified = false;
//	        Iterator<E> e = iterator();
//	        while (e.hasNext()) {
//	            if (!c.contains(e.next())) {
//		        e.remove();
//		        modified = true;
//	            }
//	        }
//	        return modified;
//        }
    }
}
