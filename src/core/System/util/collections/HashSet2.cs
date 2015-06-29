/*
 * $Id$
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

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

        public bool AddAndCheck(T item)
        {
            Object o = null;
            if (set.TryGetValue(item, out o)) {
                return false;
            }
            set[item] = null;
            return true;

        }

        virtual public void Add(T item){
            
            set[item] = null;           

        }

        virtual public void AddAll(IEnumerable<T> set)
        {
            foreach (T item in set)
                Add(item);
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

        virtual public bool IsEmpty()
        {
            return set.Count == 0;
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

        virtual internal Dictionary<T, object> InternalSet
        {
            get { return set; }
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
