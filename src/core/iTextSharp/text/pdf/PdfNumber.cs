using System;
using iTextSharp.text.error_messages;

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
     * <CODE>PdfNumber</CODE> provides two types of numbers, int and real.
     * <P>
     * ints may be specified by signed or unsigned constants. Reals may only be
     * in decimal format.<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 4.3 (page 37).
     *
     * @see        PdfObject
     * @see        BadPdfFormatException
     */

    public class PdfNumber : PdfObject {
    
        /** actual value of this <CODE>PdfNumber</CODE>, represented as a <CODE>double</CODE> */
        private double value;
    
        // constructors
    
        /**
         * Constructs a <CODE>PdfNumber</CODE>-object.
         *
         * @param        content            value of the new <CODE>PdfNumber</CODE>-object
         */
    
        public PdfNumber(string content) : base(NUMBER) {
            try {
                value = Double.Parse(content.Trim(), System.Globalization.NumberFormatInfo.InvariantInfo);
                this.Content = content;
            }
            catch (Exception nfe){
                throw new Exception(MessageLocalization.GetComposedMessage("1.is.not.a.valid.number.2", content, nfe.ToString()));
            }
        }
    
        /**
         * Constructs a new int <CODE>PdfNumber</CODE>-object.
         *
         * @param        value                value of the new <CODE>PdfNumber</CODE>-object
         */
    
        public PdfNumber(int value) : base(NUMBER) {
            this.value = value;
            this.Content = value.ToString();
        }

        /**
         * Constructs a new long <CODE>PdfNumber</CODE>-object.
         *
         * @param        value                value of the new <CODE>PdfNumber</CODE>-object
         */
    
        public PdfNumber(long value) : base(NUMBER) {
            this.value = value;
            this.Content = value.ToString();
        }
    
        /**
         * Constructs a new REAL <CODE>PdfNumber</CODE>-object.
         *
         * @param        value                value of the new <CODE>PdfNumber</CODE>-object
         */
    
        public PdfNumber(double value) : base(NUMBER) {
            this.value = value;
            Content = ByteBuffer.FormatDouble(value);
        }
    
        /**
         * Constructs a new REAL <CODE>PdfNumber</CODE>-object.
         *
         * @param        value                value of the new <CODE>PdfNumber</CODE>-object
         */
    
        public PdfNumber(float value) : this((double)value) {}
    
        // methods returning the value of this object
    
        /**
         * Returns the primitive <CODE>int</CODE> value of this object.
         *
         * @return        a value
         */
    
        virtual public int IntValue {
            get {
                return (int) value;
            }
        }
    
        /**
         * Returns the primitive <CODE>long</CODE> value of this object.
         *
         * @return        a value
         */
    
        virtual public long LongValue {
            get {
                return (long) value;
            }
        }
    
        /**
         * Returns the primitive <CODE>double</CODE> value of this object.
         *
         * @return        a value
         */
    
        virtual public double DoubleValue {
            get {
                return value;
            }
        }
    
        virtual public float FloatValue {
            get {
                return (float)value;
            }
        }
    
        // other methods
    
        /**
         * Increments the value of the <CODE>PdfNumber</CODE>-object with 1.
         */
    
        virtual public void Increment() {
            value += 1.0;
            Content = ByteBuffer.FormatDouble(value);
        }
    }
}
