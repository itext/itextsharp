using System;
using System.IO;
using System.Collections;
using System.util;

/*
 * $Id: PdfArray.cs,v 1.7 2008/05/13 11:25:18 psoares33 Exp $
 * 
 *
 * Copyright 1999, 2000, 2001, 2002 Bruno Lowagie
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */

namespace iTextSharp.text.pdf {

/**
 * <CODE>PdfArray</CODE> is the PDF Array object.
 * <P>
 * An array is a sequence of PDF objects. An array may contain a mixture of object types.
 * An array is written as a left square bracket ([), followed by a sequence of objects,
 * followed by a right square bracket (]).<BR>
 * This object is described in the 'Portable Document Format Reference Manual version 1.3'
 * section 4.6 (page 40).
 *
 * @see        PdfObject
 */

public class PdfArray : PdfObject {
    
    // membervariables
    
/** this is the actual array of PdfObjects */
    protected ArrayList arrayList;
    
    // constructors
    
/**
 * Constructs an empty <CODE>PdfArray</CODE>-object.
 */
    
    public PdfArray() : base(ARRAY) {
        arrayList = new ArrayList();
    }
    
/**
 * Constructs an <CODE>PdfArray</CODE>-object, containing 1 <CODE>PdfObject</CODE>.
 *
 * @param    object        a <CODE>PdfObject</CODE> that has to be added to the array
 */
    
    public PdfArray(PdfObject obj) : base(ARRAY) {
        arrayList = new ArrayList();
        arrayList.Add(obj);
    }
    
    public PdfArray(float[] values) : base(ARRAY) {
        arrayList = new ArrayList();
        Add(values);
    }
    
    public PdfArray(int[] values) : base(ARRAY) {
        arrayList = new ArrayList();
        Add(values);
    }
    
    /**
     * Constructs a PdfArray with the elements of an ArrayList.
     * Throws a ClassCastException if the ArrayList contains something
     * that isn't a PdfObject.
     * @param   l   an ArrayList with PdfObjects
     * @since 2.1.3
     */
    public PdfArray(ArrayList l) : this() {
        foreach (PdfObject o in l)
            Add(o);
    }

/**
 * Constructs an <CODE>PdfArray</CODE>-object, containing all the <CODE>PdfObject</CODE>s in a given <CODE>PdfArray</CODE>.
 *
 * @param    array        a <CODE>PdfArray</CODE> that has to be added to the array
 */
    
    public PdfArray(PdfArray array) : base(ARRAY) {
        arrayList = new ArrayList(array.arrayList);
    }
    
    // methods overriding some methods in PdfObject
    
/**
 * Returns the PDF representation of this <CODE>PdfArray</CODE>.
 *
 * @return        an array of <CODE>byte</CODE>s
 */
    
    public override void ToPdf(PdfWriter writer, Stream os) {
        os.WriteByte((byte)'[');
        bool first = true;
        PdfObject obj = null;
        foreach (PdfObject obja in arrayList) {
            obj = (obja == null) ? PdfNull.PDFNULL : obja;
            type = obj.Type;
            if (!first && type != PdfObject.ARRAY && type != PdfObject.DICTIONARY && type != PdfObject.NAME && type != PdfObject.STRING)
                os.WriteByte((byte)' ');
            first = false;
            obj.ToPdf(writer, os);
        }
        os.WriteByte((byte)']');
    }
    
    // methods concerning the ArrayList-membervalue
    
    // ARRAY CONTENT METHODS
    
    /**
     * Overwrites a specified location of the array.
     * 
     * @param idx The index of the element to be overwritten
     * @param obj new value for the specified index
     * @throws IndexOutOfBoundsException if the specified position doesn't exist
     * @return the previous value
     * @since 2.1.5
     */
    public PdfObject this[int idx] {
        get {
            return (PdfObject)arrayList[idx];
        }
        set {
            arrayList[idx] = value;
        }
    }

    /**
     * Remove the element at the specified position from the array.
     * 
     * Shifts any subsequent elements to the left (subtracts one from their
     * indices).
     * 
     * @param idx The index of the element to be removed.
     * @throws IndexOutOfBoundsException the specified position doesn't exist
     * @since 2.1.5
     */
    public PdfObject Remove(int idx) {
        PdfObject tmp = (PdfObject)arrayList[idx];
        arrayList.RemoveAt(idx);
        return tmp;
    }

/**
 * Returns an ArrayList containing <CODE>PdfObject</CODE>s.
 *
 * @return        an ArrayList
 */
    
    public ArrayList ArrayList {
        get {
            return arrayList;
        }
    }
    
/**
 * Returns the number of entries in the array.
 *
 * @return        the size of the ArrayList
 */
    
    public int Size {
        get {
            return arrayList.Count;
        }
    }
    
    /**
     * Returns <CODE>true</CODE> if the array is empty.
     * 
     * @return <CODE>true</CODE> if the array is empty
     * @since 2.1.5
     */
    public bool IsEmpty() {
        return arrayList.Count == 0;
    }

/**
 * Adds a <CODE>PdfObject</CODE> to the <CODE>PdfArray</CODE>.
 *
 * @param        object            <CODE>PdfObject</CODE> to add
 * @return        <CODE>true</CODE>
 */
    
    public virtual bool Add(PdfObject obj) {
        arrayList.Add(obj);
        return true;
    }
    
    public virtual bool Add(float[] values) {
        for (int k = 0; k < values.Length; ++k)
            arrayList.Add(new PdfNumber(values[k]));
        return true;
    }
    
    public virtual bool Add(int[] values) {
        for (int k = 0; k < values.Length; ++k)
            arrayList.Add(new PdfNumber(values[k]));
        return true;
    }
    
    /**
     * Inserts the specified element at the specified position.
     * 
     * Shifts the element currently at that position (if any) and
     * any subsequent elements to the right (adds one to their indices).
     *
     * @param index The index at which the specified element is to be inserted
     * @param element The element to be inserted
     * @throws IndexOutOfBoundsException if the specified index is larger than the
     *   last position currently set, plus 1. 
     * @since 2.1.5
     */
    public virtual void Add(int index, PdfObject element) {
        arrayList.Insert(index, element);
    }

    /**
     * Inserts a <CODE>PdfObject</CODE> at the beginning of the
     * <CODE>PdfArray</CODE>.
     * 
     * The <CODE>PdfObject</CODE> will be the first element, any other elements
     * will be shifted to the right (adds one to their indices).
     *
     * @param object The <CODE>PdfObject</CODE> to add
     */
    public virtual void AddFirst(PdfObject obj) {
        arrayList.Insert(0, obj);
    }
    
/**
 * Checks if the <CODE>PdfArray</CODE> allready contains a certain <CODE>PdfObject</CODE>.
 *
 * @param        object            <CODE>PdfObject</CODE> to check
 * @return        <CODE>true</CODE>
 */
    
    public bool Contains(PdfObject obj) {
        return arrayList.Contains(obj);
    }

    public ListIterator GetListIterator() {
        return new ListIterator(arrayList);
    }

    public override string ToString() {
        return arrayList.ToString();
    }

//    public PdfObject GetPdfObject( int idx ) {
//        return (PdfObject)arrayList[idx];
//    }
    
    public PdfObject GetDirectObject( int idx ) {
        return PdfReader.GetPdfObject(this[idx]);
    }
    
    // more of the same like PdfDictionary. (MAS 2/17/06)
    public PdfDictionary GetAsDict(int idx) {
        PdfDictionary dict = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsDictionary())
            dict = (PdfDictionary) orig;
        return dict;
    }
    
    public PdfArray GetAsArray(int idx) {
        PdfArray array = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsArray())
            array = (PdfArray) orig;
        return array;
    }
    
    public PdfStream GetAsStream(int idx) {
        PdfStream stream = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsStream())
            stream = (PdfStream) orig;
        return stream;
    }
    
    public PdfString GetAsString(int idx) {
        PdfString str = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsString())
            str = (PdfString) orig;
        return str;
    }
    
    public PdfNumber GetAsNumber(int idx) {
        PdfNumber number = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsNumber())
            number = (PdfNumber) orig;
        return number;
    }
    
    public PdfName GetAsName(int idx) {
        PdfName name = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsName())
            name = (PdfName) orig;
        return name;
    }
    
    public PdfBoolean GetAsBoolean(int idx) {
        PdfBoolean b = null;
        PdfObject orig = GetDirectObject(idx);
        if (orig != null && orig.IsBoolean())
            b = (PdfBoolean) orig;
        return b;
    }
    
    public PdfIndirectReference GetAsIndirectObject(int idx) {
        PdfIndirectReference refi = null;
        PdfObject orig = this[idx]; // not getDirect this time.
        if (orig != null && orig.IsIndirect())
            refi = (PdfIndirectReference) orig;
        return refi;
    }
}
}