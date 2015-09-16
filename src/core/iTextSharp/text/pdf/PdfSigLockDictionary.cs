using System;
/*
 * $Id: PdfSigLockDictionary.java 5345 2012-08-22 08:14:09Z blowagie $
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
namespace iTextSharp.text.pdf {

    /**
     * A signature field lock dictionary.
     */
    public class PdfSigLockDictionary : PdfDictionary  {
        
        /**
         * Enumerates the different actions of a signature lock.
         * Indicates the set of fields that should be locked:
         * all the fields in the document,
         * all the fields specified in the /Fields array
         * all the fields except those specified in the /Fields array
         */
        public class LockAction {
            public static readonly LockAction ALL = new LockAction(PdfName.ALL);
            public static readonly LockAction INCLUDE = new LockAction(PdfName.INCLUDE);
            public static readonly LockAction EXCLUDE = new LockAction(PdfName.EXCLUDE);

            private PdfName name;
            
            private LockAction(PdfName name) {
                this.name = name;
            }
            
            virtual public PdfName Value {
                get {
                    return name;
                }
            }
        }

        /**
         * Enumerates the different levels of permissions.
         */
        public class LockPermissions {
            public static readonly LockPermissions NO_CHANGES_ALLOWED = new LockPermissions(1);
            public static readonly LockPermissions FORM_FILLING = new LockPermissions(2);
            public static readonly LockPermissions FORM_FILLING_AND_ANNOTATION = new LockPermissions(3);
            
            private PdfNumber number;
            
            private LockPermissions(int p) {
                number = new PdfNumber(p);
            }
            
            virtual public PdfNumber Value {
                get {
                    return number;
                }
            }
        }
        
        /**
         * Creates a signature lock valid for all fields in the document.
         */
        public PdfSigLockDictionary() : base(PdfName.SIGFIELDLOCK) {
            this.Put(PdfName.ACTION, LockAction.ALL.Value);
        }
        
        /**
         * Creates a signature lock for all fields in the document,
         * setting specific permissions.
         */
        public PdfSigLockDictionary(LockPermissions p) : this() {
           this.Put(PdfName.P, p.Value);
        }
        
        /**
         * Creates a signature lock for specific fields in the document.
         */
        public PdfSigLockDictionary(LockAction action, params String[] fields) : this(action, null, fields) {
        }
        
        /**
         * Creates a signature lock for specific fields in the document.
         */
        public PdfSigLockDictionary(LockAction action, LockPermissions p, params String[] fields) : base(PdfName.SIGFIELDLOCK) {
            this.Put(PdfName.ACTION, action.Value);
            if (p != null)
                this.Put(PdfName.P, p.Value);
            PdfArray fieldsArray = new PdfArray();
            foreach (String field in fields) {
                fieldsArray.Add(new PdfString(field));
            }
            this.Put(PdfName.FIELDS, fieldsArray);
        }
    }
}
