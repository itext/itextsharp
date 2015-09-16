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
using iTextSharp.text.error_messages;

/*
 * This class is based on org.apache.IntHashMap.commons.lang
 * http://jakarta.apache.org/commons/lang/xref/org/apache/commons/lang/IntHashMap.html
 * It was adapted by Bruno Lowagie for use in iText,
 * reusing methods that were written by Paulo Soares.
 * Instead of being a hashtable that stores objects with an int as key,
 * it stores int values with an int as key.
 * 
 * This is the original license of the original class IntHashMap:
 * 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Note: originally released under the GNU LGPL v2.1, 
 * but rereleased by the original author under the ASF license (above).
 */

namespace iTextSharp.text.pdf {

    /// A Hashtable that uses ints as the keys.
    // <P>
    // Use just like java.util.Hashtable, except that the keys must be ints.
    // This is much faster than creating a new int for each access.
    // <P>
    // <A HREF="/resources/classes/Acme/IntHashtable.java">Fetch the software.</A><BR>
    // <A HREF="/resources/classes/Acme.tar.gz">Fetch the entire Acme package.</A>
    // <P>
    // @see java.util.Hashtable

    public class IntHashtable {
        /// The hash table data.
        private IntHashtableEntry[] table;
    
        /// The total number of entries in the hash table.
        private int count;
    
        /// Rehashes the table when count exceeds this threshold.
        private int threshold;
    
        /// The load factor for the hashtable.
        private float loadFactor;
    
        /// Constructs a new, empty hashtable with the specified initial
        // capacity and the specified load factor.
        // @param initialCapacity the initial number of buckets
        // @param loadFactor a number between 0.0 and 1.0, it defines
        //      the threshold for rehashing the hashtable into
        //      a bigger one.
        // @exception IllegalArgumentException If the initial capacity
        // is less than or equal to zero.
        // @exception IllegalArgumentException If the load factor is
        // less than or equal to zero.
        public IntHashtable( int initialCapacity, float loadFactor ) {
            if (initialCapacity < 0) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.capacity.1", initialCapacity));
            }
            if (loadFactor <= 0) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.load.1", loadFactor));
            }
            this.loadFactor = loadFactor;
            table = new IntHashtableEntry[initialCapacity];
            threshold = (int) ( initialCapacity * loadFactor );
        }
    
        /// Constructs a new, empty hashtable with the specified initial
        // capacity.
        // @param initialCapacity the initial number of buckets
        public IntHashtable( int initialCapacity ) : this( initialCapacity, 0.75f ) {}
    
        /// Constructs a new, empty hashtable. A default capacity and load factor
        // is used. Note that the hashtable will automatically grow when it gets
        // full.
        public IntHashtable() : this( 101, 0.75f ) {}
    
        /// Returns the number of elements contained in the hashtable.
        virtual public int Size {
            get {
                return count;
            }
        }
    
        /// Returns true if the hashtable contains no elements.
        virtual public bool IsEmpty() {
            return count == 0;
        }
    
        /// Returns true if the specified object is an element of the hashtable.
        // This operation is more expensive than the ContainsKey() method.
        // @param value the value that we are looking for
        // @exception NullPointerException If the value being searched
        // for is equal to null.
        // @see IntHashtable#containsKey
        virtual public bool Contains( int value ) {
            IntHashtableEntry[] tab = table;
            for ( int i = tab.Length ; i-- > 0 ; ) {
                for ( IntHashtableEntry e = tab[i] ; e != null ; e = e.next ) {
                    if ( e.value == value )
                        return true;
                }
            }
            return false;
        }
    
        /// Returns true if the collection contains an element for the key.
        // @param key the key that we are looking for
        // @see IntHashtable#contains
        virtual public bool ContainsKey( int key ) {
            IntHashtableEntry[] tab = table;
            int hash = key;
            int index = ( hash & 0x7FFFFFFF ) % tab.Length;
            for ( IntHashtableEntry e = tab[index] ; e != null ; e = e.next ) {
                if ( e.hash == hash && e.key == key )
                    return true;
            }
            return false;
        }
    
        /// Gets the object associated with the specified key in the
        // hashtable.
        // @param key the specified key
        // @returns the element for the key or null if the key
        //      is not defined in the hash table.
        // @see IntHashtable#put
        public int this[int key] {
            get {
                IntHashtableEntry[] tab = table;
                int hash = key;
                int index = ( hash & 0x7FFFFFFF ) % tab.Length;
                for ( IntHashtableEntry e = tab[index] ; e != null ; e = e.next ) {
                    if ( e.hash == hash && e.key == key )
                        return e.value;
                }
                return 0;
            }

            set {
                // Makes sure the key is not already in the hashtable.
                IntHashtableEntry[] tab = table;
                int hash = key;
                int index = ( hash & 0x7FFFFFFF ) % tab.Length;
                for ( IntHashtableEntry e = tab[index] ; e != null ; e = e.next ) {
                    if ( e.hash == hash && e.key == key ) {
                        e.value = value;
                        return;
                    }
                }
        
                if ( count >= threshold ) {
                    // Rehash the table if the threshold is exceeded.
                    Rehash();
                    this[key] = value;
                    return;
                }
        
                // Creates the new entry.
                IntHashtableEntry en = new IntHashtableEntry();
                en.hash = hash;
                en.key = key;
                en.value = value;
                en.next = tab[index];
                tab[index] = en;
                ++count;
            }
        }
    
        /// Rehashes the content of the table into a bigger table.
        // This method is called automatically when the hashtable's
        // size exceeds the threshold.
        virtual protected void Rehash() {
            int oldCapacity = table.Length;
            IntHashtableEntry[] oldTable = table;
        
            int newCapacity = oldCapacity * 2 + 1;
            IntHashtableEntry[] newTable = new IntHashtableEntry[newCapacity];
        
            threshold = (int) ( newCapacity * loadFactor );
            table = newTable;
        
            for ( int i = oldCapacity ; i-- > 0 ; ) {
                for ( IntHashtableEntry old = oldTable[i] ; old != null ; ) {
                    IntHashtableEntry e = old;
                    old = old.next;
                
                    int index = ( e.hash & 0x7FFFFFFF ) % newCapacity;
                    e.next = newTable[index];
                    newTable[index] = e;
                }
            }
        }
    
        /// Removes the element corresponding to the key. Does nothing if the
        // key is not present.
        // @param key the key that needs to be removed
        // @return the value of key, or null if the key was not found.
        virtual public int Remove( int key ) {
            IntHashtableEntry[] tab = table;
                int hash = key;
            int index = ( hash & 0x7FFFFFFF ) % tab.Length;
            for ( IntHashtableEntry e = tab[index], prev = null ; e != null ; prev = e, e = e.next ) {
                if ( e.hash == hash && e.key == key ) {
                    if ( prev != null )
                        prev.next = e.next;
                    else
                        tab[index] = e.next;
                    --count;
                    return e.value;
                }
            }
            return 0;
        }
    
        /// Clears the hash table so that it has no more elements in it.
        virtual public void Clear() {
            IntHashtableEntry[] tab = table;
            for ( int index = tab.Length; --index >= 0; )
                tab[index] = null;
            count = 0;
        }
    
        virtual public IntHashtable Clone() {
            IntHashtable t = new IntHashtable();
            t.count = count;
            t.loadFactor = loadFactor;
            t.threshold = threshold;
            t.table = new IntHashtableEntry[table.Length];
            for (int i = table.Length ; i-- > 0 ; ) {
                t.table[i] = (table[i] != null)
                ? (IntHashtableEntry)table[i].Clone() : null;
            }
            return t;
        }

        virtual public int[] ToOrderedKeys() {
            int[] res = GetKeys();
            Array.Sort(res);
            return res;
        }
        
        virtual public int[] GetKeys() {
            int[] res = new int[count];
            int ptr = 0;
            int index = table.Length;
            IntHashtableEntry entry = null;
            while (true) {
                if (entry == null)
                    while ((index-- > 0) && ((entry = table[index]) == null));
                if (entry == null)
                    break;
                IntHashtableEntry e = entry;
                entry = e.next;
                res[ptr++] = e.key;
            }
            return res;
        }
    
        public class IntHashtableEntry {
            internal int hash;
            internal int key;
            internal int value;
            internal IntHashtableEntry next;
            
            virtual public int Key {
                get {
                    return key;
                }
            }
            
            virtual public int Value {
                get {
                    return value;
                }
            }
            
            virtual protected internal IntHashtableEntry Clone() {
                IntHashtableEntry entry = new IntHashtableEntry();
                entry.hash = hash;
                entry.key = key;
                entry.value = value;
                entry.next = (next != null) ? next.Clone() : null;
                return entry;
            }
        }    

        virtual public IntHashtableIterator GetEntryIterator() {
            return new IntHashtableIterator(table);
        }
        
        public class IntHashtableIterator {
            //    boolean keys;
            int index;
            IntHashtableEntry[] table;
            IntHashtableEntry entry;
            
            internal IntHashtableIterator(IntHashtableEntry[] table) {
                this.table = table;
                this.index = table.Length;
            }
            
            virtual public bool HasNext() {
                if (entry != null) {
                    return true;
                }
                while (index-- > 0) {
                    if ((entry = table[index]) != null) {
                        return true;
                    }
                }
                return false;
            }
            
            virtual public IntHashtableEntry Next() {
                if (entry == null) {
                    while ((index-- > 0) && ((entry = table[index]) == null));
                }
                if (entry != null) {
                    IntHashtableEntry e = entry;
                    entry = e.next;
                    return e;
                }
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("inthashtableiterator"));
            }
        }        
    }
}
