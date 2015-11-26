using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf.intern;

/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
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

namespace iTextSharp.text.pdf {
    /**
     * <CODE>PdfDictionary</CODE> is the Pdf dictionary object.
     * <P>
     * A dictionary is an associative table containing pairs of objects. The first element
     * of each pair is called the <I>key</I> and the second element is called the <I>value</I>.
     * Unlike dictionaries in the PostScript language, a key must be a <CODE>PdfName</CODE>.
     * A value can be any kind of <CODE>PdfObject</CODE>, including a dictionary. A dictionary is
     * generally used to collect and tie together the attributes of a complex object, with each
     * key-value pair specifying the name and value of an attribute.<BR>
     * A dictionary is represented by two left angle brackets (<<), followed by a sequence of
     * key-value pairs, followed by two right angle brackets (>>).<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 4.7 (page 40-41).
     * <P>
     *
     * @see        PdfObject
     * @see        PdfName
     * @see        BadPdfFormatException
     */

    public class PdfDictionary : PdfObject {
    
        // static membervariables (types of dictionary's)
    
        /** This is a possible type of dictionary */
        public static PdfName FONT = PdfName.FONT;
    
        /** This is a possible type of dictionary */
        public static PdfName OUTLINES = PdfName.OUTLINES;
    
        /** This is a possible type of dictionary */
        public static PdfName PAGE = PdfName.PAGE;
    
        /** This is a possible type of dictionary */
        public static PdfName PAGES = PdfName.PAGES;
    
        /** This is a possible type of dictionary */
        public static PdfName CATALOG = PdfName.CATALOG;
    
        // membervariables
    
        /** This is the type of this dictionary */
        private PdfName dictionaryType = null;
    
        /** This is the hashmap that contains all the values and keys of the dictionary */
        protected internal Dictionary<PdfName, PdfObject> hashMap;
    
        // constructors
    
        /**
         * Constructs an empty <CODE>PdfDictionary</CODE>-object.
         */
        public PdfDictionary() : base(DICTIONARY) {
            hashMap = new Dictionary<PdfName,PdfObject>();
        }

        /**
         * Constructs a <CODE>PdfDictionary</CODE>-object of a certain type.
         *
         * @param        type    a <CODE>PdfName</CODE>
         */
        public PdfDictionary(PdfName type)
            : this() {
            dictionaryType = type;
            Put(PdfName.TYPE, dictionaryType);
        }

        public PdfDictionary(int capacity): base(DICTIONARY) {
            hashMap = new Dictionary<PdfName, PdfObject>(capacity);
        }
    
        // methods overriding some methods in PdfObject
    
        /**
         * Returns the PDF representation of this <CODE>PdfDictionary</CODE>.
         *
         * @return        an array of <CODE>byte</CODE>
         */
        public override void ToPdf(PdfWriter writer, Stream os) {
            PdfWriter.CheckPdfIsoConformance(writer, PdfIsoKeys.PDFISOKEY_OBJECT, this);
            os.WriteByte((byte)'<');
            os.WriteByte((byte)'<');

            // loop over all the object-pairs in the Hashtable
            PdfObject value;
            foreach (KeyValuePair<PdfName, PdfObject> e in hashMap) {
                value = e.Value;
                e.Key.ToPdf(writer, os);
                int type = value.Type;
                if (type != PdfObject.ARRAY && type != PdfObject.DICTIONARY && type != PdfObject.NAME && type != PdfObject.STRING)
                    os.WriteByte((byte)' ');
                value.ToPdf(writer, os);
            }
            os.WriteByte((byte)'>');
            os.WriteByte((byte)'>');
        }
    
    
        // methods concerning the Hashtable member value
    
        /**
         * Adds a <CODE>PdfObject</CODE> and its key to the <CODE>PdfDictionary</CODE>.
         * If the value is <CODE>null</CODE> or <CODE>PdfNull</CODE> the key is deleted.
         *
         * @param        key        key of the entry (a <CODE>PdfName</CODE>)
         * @param        value    value of the entry (a <CODE>PdfObject</CODE>)
         */
        virtual public void Put(PdfName key, PdfObject value) {
            if (value == null || value.IsNull())
                hashMap.Remove(key);
            else
                hashMap[key] = value;
        }
    
        /**
         * Adds a <CODE>PdfObject</CODE> and its key to the <CODE>PdfDictionary</CODE>.
         * If the value is null it does nothing.
         *
         * @param        key        key of the entry (a <CODE>PdfName</CODE>)
         * @param        value    value of the entry (a <CODE>PdfObject</CODE>)
         */
        virtual public void PutEx(PdfName key, PdfObject value) {
            if (value == null)
                return;
            Put(key, value);
        }
    
        /**
         * Copies all of the mappings from the specified <CODE>PdfDictionary</CODE>
         * to this <CODE>PdfDictionary</CODE>.
         *
         * These mappings will replace any mappings previously contained in this
         * <CODE>PdfDictionary</CODE>.
         *
         * @param dic The <CODE>PdfDictionary</CODE> with the mappings to be
         *   copied over
         */
        virtual public void PutAll(PdfDictionary dic) {
            if (hashMap.Count == 0) {
                hashMap = new Dictionary<PdfName, PdfObject>(dic.hashMap);
            } else {
                foreach (KeyValuePair<PdfName, PdfObject> item in dic.hashMap) {
                    if (hashMap.ContainsKey(item.Key))
                        hashMap[item.Key] = item.Value;
                    else
                        hashMap.Add(item.Key, item.Value);
                }
            }
        }

        /**
         * Removes a <CODE>PdfObject</CODE> and its key from the <CODE>PdfDictionary</CODE>.
         *
         * @param        key        key of the entry (a <CODE>PdfName</CODE>)
         */
        virtual public void Remove(PdfName key) {
            hashMap.Remove(key);
        }
    
        /**
         * Removes all the <CODE>PdfObject</CODE>s and its <VAR>key</VAR>s from the
         * <CODE>PdfDictionary</CODE>.
         * @since 5.0.2
         */
        virtual public void Clear() {
            hashMap.Clear();
        }

        /**
         * Gets a <CODE>PdfObject</CODE> with a certain key from the <CODE>PdfDictionary</CODE>.
         *
         * @param        key        key of the entry (a <CODE>PdfName</CODE>)
         * @return        the previous </CODE>PdfObject</CODE> corresponding with the <VAR>key</VAR>
         */
        virtual public PdfObject Get(PdfName key) {
            PdfObject obj;
            if (hashMap.TryGetValue(key, out obj))
                return obj;
            else
                return null;
        }
    
        // methods concerning the type of Dictionary
    
        /**
         *  Checks if a <CODE>Dictionary</CODE> is of the type FONT.
         *
         * @return        <CODE>true</CODE> if it is, <CODE>false</CODE> if it isn't.
         */
        virtual public bool IsFont() {
            return CheckType(FONT);
        }
    
        /**
         *  Checks if a <CODE>Dictionary</CODE> is of the type PAGE.
         *
         * @return        <CODE>true</CODE> if it is, <CODE>false</CODE> if it isn't.
         */
        virtual public bool IsPage() {
            return CheckType(PAGE);
        }
    
        /**
         *  Checks if a <CODE>Dictionary</CODE> is of the type PAGES.
         *
         * @return        <CODE>true</CODE> if it is, <CODE>false</CODE> if it isn't.
         */
        virtual public bool IsPages() {
            return CheckType(PAGES);
        }
    
        /**
         *  Checks if a <CODE>Dictionary</CODE> is of the type CATALOG.
         *
         * @return        <CODE>true</CODE> if it is, <CODE>false</CODE> if it isn't.
         */
        virtual public bool IsCatalog() {
            return CheckType(CATALOG);
        }
    
        /**
         *  Checks if a <CODE>Dictionary</CODE> is of the type OUTLINES.
         *
         * @return        <CODE>true</CODE> if it is, <CODE>false</CODE> if it isn't.
         */
        virtual public bool IsOutlineTree() {
            return CheckType(OUTLINES);
        }

        /**
         * Checks the type of the dictionary.
         * @param type the type you're looking for
         * @return true if the type of the dictionary corresponds with the type you're looking for
         */
        virtual public bool CheckType(PdfName type) {
            if(type == null)
                return false;
            if(dictionaryType == null)
                dictionaryType = GetAsName(PdfName.TYPE);
            return type.Equals(dictionaryType);
        }


        virtual public void Merge(PdfDictionary other) {
            if (hashMap.Count == 0) { 
                hashMap = new Dictionary<PdfName, PdfObject>(other.hashMap);
            } else {
                foreach (PdfName key in other.hashMap.Keys) {
                    hashMap[key] = other.hashMap[key];
                }
            }
        }
    
        virtual public void MergeDifferent(PdfDictionary other) {
            foreach (PdfName key in other.hashMap.Keys) {
                if (!hashMap.ContainsKey(key)) {
                    hashMap[key] = other.hashMap[key];
                }
            }
        }

        virtual public Dictionary<PdfName,PdfObject>.KeyCollection Keys {
            get {
                return hashMap.Keys;
            }
        }

        virtual public int Size {
            get {
                return hashMap.Count;
            }
        }
    
        virtual public bool Contains(PdfName key) {
            return hashMap.ContainsKey(key);
        }

        public virtual Dictionary<PdfName,PdfObject>.Enumerator GetEnumerator() {
            return hashMap.GetEnumerator();
        }

        public override String ToString() {
            if (Get(PdfName.TYPE) == null) return "Dictionary";
    	    return "Dictionary of type: " + Get(PdfName.TYPE);
        }

        /**
        * This function behaves the same as 'get', but will never return an indirect reference,
        * it will always look such references up and return the actual object.
        * @param key 
        * @return null, or a non-indirect object
        */
        public virtual PdfObject GetDirectObject(PdfName key) {
            return PdfReader.GetPdfObject(Get(key));
        }
        
        /**
        * All the getAs functions will return either null, or the specified object type
        * This function will automatically look up indirect references. There's one obvious
        * exception, the one that will only return an indirect reference.  All direct objects
        * come back as a null.
        * Mark A Storer (2/17/06)
        * @param key
        * @return the appropriate object in its final type, or null
        */
        virtual public PdfDictionary GetAsDict(PdfName key) {
            PdfDictionary dict = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsDictionary())
                dict = (PdfDictionary) orig;
            return dict;
        }
        
        virtual public PdfArray GetAsArray(PdfName key) {
            PdfArray array = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsArray())
                array = (PdfArray) orig;
            return array;
        }
        
        virtual public PdfStream GetAsStream(PdfName key) {
            PdfStream stream = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsStream())
                stream = (PdfStream) orig;
            return stream;
        }
        
        virtual public PdfString GetAsString(PdfName key) {
            PdfString str = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsString())
                str = (PdfString) orig;
            return str;
        }
        
        virtual public PdfNumber GetAsNumber(PdfName key) {
            PdfNumber number = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsNumber())
                number = (PdfNumber) orig;
            return number;
        }
        
        virtual public PdfName GetAsName(PdfName key) {
            PdfName name = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsName())
                name = (PdfName) orig;
            return name;
        }
        
        virtual public PdfBoolean GetAsBoolean(PdfName key) {
            PdfBoolean b = null;
            PdfObject orig = GetDirectObject(key);
            if (orig != null && orig.IsBoolean())
                b = (PdfBoolean)orig;
            return b;
        }
        
        virtual public PdfIndirectReference GetAsIndirectObject( PdfName key ) {
            PdfIndirectReference refi = null;
            PdfObject orig = Get(key); // not getDirect this time.
            if (orig != null && orig.IsIndirect())
                refi = (PdfIndirectReference) orig;
            return refi;
        }
    }
}
