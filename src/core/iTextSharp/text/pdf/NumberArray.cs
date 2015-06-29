using System;
using System.Collections.Generic;
/*
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
     * A PdfArray object consisting of nothing but PdfNumber objects
     * @since 5.1.0
     */
    public class NumberArray : PdfArray {

        /**
         * Creates a PdfArray consisting of PdfNumber objects.
         * @param numbers float values
         */
        public NumberArray() : base() {
        }
        
        public NumberArray(float[] numbers) : base() {
            foreach (float f in numbers) {
                Add(new PdfNumber(f));
            }
        }
        
        public NumberArray(float n1) : base() {
                Add(new PdfNumber(n1));
        }
        
        public NumberArray(float n1, float n2) : base() {
                Add(new PdfNumber(n1));
                Add(new PdfNumber(n2));
        }
        
        public NumberArray(float n1, float n2, float n3) : base() {
                Add(new PdfNumber(n1));
                Add(new PdfNumber(n2));
                Add(new PdfNumber(n3));
        }
        
        public NumberArray(float n1, float n2, float n3, float n4) : base() {
                Add(new PdfNumber(n1));
                Add(new PdfNumber(n2));
                Add(new PdfNumber(n3));
                Add(new PdfNumber(n4));
        }
        
        public NumberArray(float n1, float n2, float n3, float n4, float n5) : base() {
                Add(new PdfNumber(n1));
                Add(new PdfNumber(n2));
                Add(new PdfNumber(n3));
                Add(new PdfNumber(n4));
                Add(new PdfNumber(n5));
        }
        
        public NumberArray(float n1, float n2, float n3, float n4, float n5, float n6) : base() {
                Add(new PdfNumber(n1));
                Add(new PdfNumber(n2));
                Add(new PdfNumber(n3));
                Add(new PdfNumber(n4));
                Add(new PdfNumber(n5));
                Add(new PdfNumber(n6));
        }
                
        /**
         * Creates a PdfArray consisting of PdfNumber objects.
         * @param numbers a List containing PdfNumber objects
         */
        public NumberArray(IList<PdfNumber> numbers) : base() {
            foreach (PdfNumber n in numbers) {
                Add(n);
            }
        }
    }
}
