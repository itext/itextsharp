/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections.Generic;

namespace iTextSharp.tool.xml.util {
    public class SortUtil {

        /// <summary>
        /// A stable sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="comparison"></param>
        public static void InsertionSort<T>(IList<T> list, Comparison<T> comparison) {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparison == null)
                throw new ArgumentNullException("comparison");
            
            int count = list.Count;
            for (int j = 1; j < count; j++) {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--) {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }

        public static void InsertionSort<T>(IList<T> list) where T : IComparable<T> {
            InsertionSort(list, delegate(T o1, T o2) { return o1.CompareTo(o2); });
        }

        public static void MergeSort<T>(List<T> list, Comparison<T> comparison) {
            MergeSort(list, 0, list.Count - 1, comparison);
        }

        public static void MergeSort<T>(List<T> list) where T : IComparable<T> {
            MergeSort(list, delegate(T o1, T o2) { return o1.CompareTo(o2); });
        }

        private static void MergeSort<T>(List<T> list, int left, int right, Comparison<T> comparison) {
            if (right > left) {
                int mid = (right + left) / 2;
                MergeSort(list, left, mid, comparison);
                MergeSort(list, (mid + 1), right, comparison);

                Merge(list, left, (mid + 1), right, comparison);
            }
        }

        private static void Merge<T>(List<T> list, int left, int mid, int right, Comparison<T> comparison) {
            int eol = (mid - 1), num = (right - left + 1);

            List<T> temp = new List<T>(num);

            while ((left <= eol) && (mid <= right)) {
                if (comparison(list[left], list[mid]) <= 0)
                    temp.Add(list[left++]);
                else
                    temp.Add(list[mid++]);
            }

            while (left <= eol)
                temp.Add(list[left++]);

            while (mid <= right)
                temp.Add(list[mid++]);

            for (int i = num - 1; i >= 0; i--) {
                list[right--] = temp[i];
            }

            temp = null;
        }
    }
}
