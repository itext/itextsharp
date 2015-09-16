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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.util.collections {

    /// <summary>
    /// Provides the base class for a generic read-only dictionary.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of keys in the dictionary.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of values in the dictionary.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// An instance of the <b>ReadOnlyDictionary</b> generic class is
    /// always read-only. A dictionary that is read-only is simply a
    /// dictionary with a wrapper that prevents modifying the
    /// dictionary; therefore, if changes are made to the underlying
    /// dictionary, the read-only dictionary reflects those changes. 
    /// See <see cref="Dictionary{TKey,TValue}"/> for a modifiable version of 
    /// this class.
    /// </para>
    /// <para>
    /// <b>Notes to Implementers</b> This base class is provided to 
    /// make it easier for implementers to create a generic read-only
    /// custom dictionary. Implementers are encouraged to extend this
    /// base class instead of creating their own. 
    /// </para>
    /// </remarks>
    [Serializable]
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
        ICollection {
        private readonly IDictionary<TKey, TValue> source;
        private object syncRoot;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:ReadOnlyDictionary`2" /> class that wraps
        /// the supplied <paramref name="dictionaryToWrap"/>.
        /// </summary>
        /// <param name="dictionaryToWrap">The <see cref="T:IDictionary`2" />
        /// that will be wrapped.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when the dictionary is null.
        /// </exception>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionaryToWrap) {
            if (dictionaryToWrap == null) {
                throw new ArgumentNullException("dictionaryToWrap");
            }

            this.source = dictionaryToWrap;
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the
        /// <see cref="T:ReadOnlyDictionary`2"></see>.
        /// </summary>
        /// <value>The number of key/value pairs.</value>
        /// <returns>The number of key/value pairs contained in the
        /// <see cref="T:ReadOnlyDictionary`2"></see>.</returns>
        virtual public int Count {
            get { return this.source.Count; }
        }

        /// <summary>Gets a collection containing the keys in the
        /// <see cref="T:ReadOnlyDictionary{TKey,TValue}"></see>.</summary>
        /// <value>A <see cref="Dictionary{TKey,TValue}.KeyCollection"/> 
        /// containing the keys.</value>
        /// <returns>A
        /// <see cref="Dictionary{TKey,TValue}.KeyCollection"/>
        /// containing the keys in the
        /// <see cref="Dictionary{TKey,TValue}"></see>.
        /// </returns>
        virtual public ICollection<TKey> Keys {
            get { return this.source.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values of the
        /// <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <value>The collection of values.</value>
        virtual public ICollection<TValue> Values {
            get { return this.source.Values; }
        }

        /// <summary>Gets a value indicating whether the dictionary is read-only.
        /// This value will always be true.</summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the dictionary
        /// is synchronized (thread safe).
        /// </summary>
        bool ICollection.IsSynchronized {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to dictionary.
        /// </summary>
        object ICollection.SyncRoot {
            get {
                if (this.syncRoot == null) {
                    ICollection collection = this.source as ICollection;

                    if (collection != null) {
                        this.syncRoot = collection.SyncRoot;
                    }
                    else {
                        Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
                    }
                }

                return this.syncRoot;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// The value associated with the specified key. If the specified key
        /// is not found, a get operation throws a 
        /// <see cref="T:System.Collections.Generic.KeyNotFoundException" />,
        /// and a set operation creates a new element with the specified key.
        /// </returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when the key is null.
        /// </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and key does not exist in the collection.
        /// </exception>
        public TValue this[TKey key] {
            get { return this.source[key]; }
            set { ThrowNotSupportedException(); }
        }

        /// <summary>This method is not supported by the 
        /// <see cref="T:ReadOnlyDictionary`2"/>.</summary>
        /// <param name="key">
        /// The object to use as the key of the element to add.</param>
        /// <param name="value">
        /// The object to use as the value of the element to add.</param>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
            ThrowNotSupportedException();
        }

        /// <summary>Determines whether the <see cref="T:ReadOnlyDictionary`2" />
        /// contains the specified key.</summary>
        /// <returns>
        /// True if the <see cref="T:ReadOnlyDictionary`2" /> contains
        /// an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the
        /// <see cref="T:ReadOnlyDictionary`2"></see>.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when the key is null.
        /// </exception>
        virtual public bool ContainsKey(TKey key) {
            return this.source.ContainsKey(key);
        }

        /// <summary>
        /// This method is not supported by the <see cref="T:ReadOnlyDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// True if the element is successfully removed; otherwise, false.
        /// </returns>
        bool IDictionary<TKey, TValue>.Remove(TKey key) {
            ThrowNotSupportedException();
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value
        /// associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns>
        /// <b>true</b> if the <see cref="T:ReadOnlyDictionary`2" /> contains
        /// an element with the specified key; otherwise, <b>false</b>.
        /// </returns>
        virtual public bool TryGetValue(TKey key, out TValue value) {
            return this.source.TryGetValue(key, out value);
        }

        /// <summary>This method is not supported by the
        /// <see cref="T:ReadOnlyDictionary`2"/>.</summary>
        /// <param name="item">
        /// The object to add to the <see cref="T:ICollection`1"/>.
        /// </param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(
            KeyValuePair<TKey, TValue> item) {
            ThrowNotSupportedException();
        }

        /// <summary>This method is not supported by the 
        /// <see cref="T:ReadOnlyDictionary`2"/>.</summary>
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
            ThrowNotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:ICollection`1"/> contains a
        /// specific value.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="T:ICollection`1"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if item is found in the <b>ICollection</b>; 
        /// otherwise, <b>false</b>.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
            KeyValuePair<TKey, TValue> item) {
            ICollection<KeyValuePair<TKey, TValue>> collection = this.source;

            return collection.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a
        /// particular Array index. 
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the
        /// destination of the elements copied from ICollection.
        /// The Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
            KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            ICollection<KeyValuePair<TKey, TValue>> collection = this.source;
            collection.CopyTo(array, arrayIndex);
        }

        /// <summary>This method is not supported by the
        /// <see cref="T:ReadOnlyDictionary`2"/>.</summary>
        /// <param name="item">
        /// The object to remove from the ICollection.
        /// </param>
        /// <returns>Will never return a value.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            ThrowNotSupportedException();
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A IEnumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<KeyValuePair<TKey, TValue>>
            IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            IEnumerable<KeyValuePair<TKey, TValue>> enumerator = this.source;

            return enumerator.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An IEnumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.source.GetEnumerator();
        }

        /// <summary>
        /// For a description of this member, see <see cref="ICollection.CopyTo"/>. 
        /// </summary>
        /// <param name="array">
        /// The one-dimensional Array that is the destination of the elements copied from 
        /// ICollection. The Array must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in Array at which copying begins.
        /// </param>
        void ICollection.CopyTo(Array array, int index) {
            ICollection collection =
                new List<KeyValuePair<TKey, TValue>>(this.source);

            collection.CopyTo(array, index);
        }

        private static void ThrowNotSupportedException() {
            throw new NotSupportedException("This Dictionary is read-only");
        }
    }
}
