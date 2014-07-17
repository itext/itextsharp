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
    }
}
