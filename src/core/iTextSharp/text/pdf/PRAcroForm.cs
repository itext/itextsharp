using System;
using System.Collections.Generic;
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
    * This class captures an AcroForm on input. Basically, it extends Dictionary
    * by indexing the fields of an AcroForm
    * @author Mark Thompson
    */

    public class PRAcroForm : PdfDictionary {
        
        /**
        * This class holds the information for a single field
        */
        public class FieldInformation {
            internal String fieldName;
            internal PdfDictionary info;
            internal PRIndirectReference refi;
            
            internal FieldInformation(String fieldName, PdfDictionary info, PRIndirectReference refi) {
                this.fieldName = fieldName;
        	    this.info = info;
                this.refi = refi;
            }
        
            /**
             * Returns the name of the widget annotation (the /NM entry).
             * @return	a String or null (if there's no /NM key)
             */
            virtual public String WidgetName {
                get {
                    PdfObject name = info.Get(PdfName.NM);
                    if (name != null)
                        return name.ToString();
                    return null;
                }
            }
            virtual public String Name {
                get {
                    return fieldName; 
                }
            }
            virtual public PdfDictionary Info {
                get {
                    return info; 
                }
            }
            virtual public PRIndirectReference Ref {
                get {
                    return refi; 
                }
            }
        }

        internal List<FieldInformation> fields;
        internal List<PdfDictionary> stack;
        internal Dictionary<String, FieldInformation> fieldByName;
        internal PdfReader reader;
        
        /**
        * Constructor
        * @param reader reader of the input file
        */
        public PRAcroForm(PdfReader reader) {
            this.reader = reader;
            fields = new List<FieldInformation>();
            fieldByName = new Dictionary<string,FieldInformation>();
            stack = new List<PdfDictionary>();
        }
        /**
        * Number of fields found
        * @return size
        */
        public new int Size {
            get {
                return fields.Count;
            }
        }
        
        virtual public List<FieldInformation> Fields {
            get {
                return fields;
            }
        }
        
        virtual public FieldInformation GetField(String name) {
            FieldInformation f;
            fieldByName.TryGetValue(name, out f);
            return f;
        }
        
        /**
        * Given the title (/T) of a reference, return the associated reference
        * @param name a string containing the path
        * @return a reference to the field, or null
        */
        virtual public PRIndirectReference GetRefByName(String name) {
            FieldInformation fi = GetField(name);
            if (fi == null) return null;
            return fi.Ref;
        }
        /**
        * Read, and comprehend the acroform
        * @param root the docment root
        */
        virtual public void ReadAcroForm(PdfDictionary root) {
            if (root == null)
                return;
            hashMap = root.hashMap;
            PushAttrib(root);
            PdfArray fieldlist = (PdfArray)PdfReader.GetPdfObjectRelease(root.Get(PdfName.FIELDS));
            if (fieldlist != null) {
                IterateFields(fieldlist, null, null);
            }
        }

        /**
        * After reading, we index all of the fields. Recursive.
        * @param fieldlist An array of fields
        * @param fieldDict the last field dictionary we encountered (recursively)
        * @param parentPath the pathname of the field, up to this point or null
        */
        virtual protected void IterateFields(PdfArray fieldlist, PRIndirectReference fieldDict, String parentPath) {
            foreach (PRIndirectReference refi in fieldlist.ArrayList) {
                PdfDictionary dict = (PdfDictionary) PdfReader.GetPdfObjectRelease(refi);
                
                // if we are not a field dictionary, pass our parent's values
                PRIndirectReference myFieldDict = fieldDict;
                String fullPath = parentPath;
                PdfString tField = (PdfString)dict.Get(PdfName.T);
                bool isFieldDict = tField != null;
                
                if (isFieldDict) {
                    myFieldDict = refi;
                    if (parentPath == null) {
                        fullPath = tField.ToString();
                    } else {
                        fullPath = parentPath + '.' + tField.ToString();
                    }
                }
                
                PdfArray kids = (PdfArray)dict.Get(PdfName.KIDS);
                if (kids != null) {
                    PushAttrib(dict);
                    IterateFields(kids, myFieldDict, fullPath);
                    stack.RemoveAt(stack.Count - 1);   // pop
                }
                else {          // leaf node
                    if (myFieldDict != null) {
                        PdfDictionary mergedDict = (PdfDictionary)stack[stack.Count - 1];
                        if (isFieldDict)
                            mergedDict = MergeAttrib(mergedDict, dict);
                        
                        mergedDict.Put(PdfName.T, new PdfString(fullPath));
                        FieldInformation fi = new FieldInformation(fullPath, mergedDict, myFieldDict);
                        fields.Add(fi);
                        fieldByName[fullPath] = fi;
                    }
                }
            }
        }
        /**
        * merge field attributes from two dictionaries
        * @param parent one dictionary
        * @param child the other dictionary
        * @return a merged dictionary
        */
        virtual protected PdfDictionary MergeAttrib(PdfDictionary parent, PdfDictionary child) {
            PdfDictionary targ = new PdfDictionary();
            if (parent != null) targ.Merge(parent);
            
            foreach (PdfName key in child.Keys) {
                if (key.Equals(PdfName.DR) || key.Equals(PdfName.DA) ||
                key.Equals(PdfName.Q)  || key.Equals(PdfName.FF) ||
                key.Equals(PdfName.DV) || key.Equals(PdfName.V)
                || key.Equals(PdfName.FT) || key.Equals(PdfName.NM)
                || key.Equals(PdfName.F)) {
                    targ.Put(key,child.Get(key));
                }
            }
            return targ;
        }
        /**
        * stack a level of dictionary. Merge in a dictionary from this level
        */
        virtual protected void PushAttrib(PdfDictionary dict) {
            PdfDictionary dic = null;
            if (stack.Count != 0) {
                dic = stack[stack.Count - 1];
            }
            dic = MergeAttrib(dic, dict);
            stack.Add(dic);
        }
    }
}
