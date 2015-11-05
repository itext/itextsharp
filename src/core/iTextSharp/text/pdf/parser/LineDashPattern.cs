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

using System.util;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * Represents the line dash pattern. The line dash pattern shall control the pattern
     * of dashes and gaps used to stroke paths. It shall be specified by a dash array and
     * a dash phase.
     *
     * @since 5.5.6
     */
    public class LineDashPattern {

        private PdfArray dashArray;
        private float dashPhase;

        private int currentIndex;
        private int elemOrdinalNumber = 1;
        private DashArrayElem currentElem;

        /**
         * Creates new {@link LineDashPattern} object.
         * @param dashArray The dash array. See {@link #getDashArray()}
         * @param dashPhase The dash phase. See {@link #getDashPhase()}
         */
        public LineDashPattern(PdfArray dashArray, float dashPhase) {
            this.dashArray = new PdfArray(dashArray);
            this.dashPhase = dashPhase;
            InitFirst(dashPhase);
        }

        /**
         * Getter and setter for the dash array.
         *
         * The dash array’s elements is number that specify the lengths of
         * alternating dashes and gaps; the numbers are nonnegative. The
         * elements are expressed in user space units.
         *
         * @return The dash array.
         */
        public PdfArray DashArray {
            get { return dashArray; }
            set { dashArray = value; }
        }

        /**
         * Getter and setter for the dash phase.
         *
         * The dash phase shall specify the distance into the dash pattern at which
         * to start the dash. The elements are expressed in user space units.
         *
         * @return The dash phase.
         */
        public float DashPhase {
            get { return dashPhase; }
            set { dashPhase = value; }
        }

        /**
         * Calculates and returns the next element which is either gap or dash.
         * @return The next dash array's element.
         */
        public DashArrayElem Next() {
            DashArrayElem ret = currentElem;

            if (dashArray.Size > 0) {
                currentIndex = (currentIndex + 1) % DashArray.Size;
                currentElem = new DashArrayElem(dashArray.GetAsNumber(currentIndex).FloatValue,
                    IsEven(++elemOrdinalNumber));
            }

            return ret;
        }

        /**
         * Checks whether the dashed pattern is solid or not. It's solid when the
         * size of a dash array is even and sum of all the units off in the array
         * is 0.<br/>
         * For example: [3 0 4 0 5 0 6 0] (sum is 0), [3 0 4 0 5 1] (sum is 1).
         */
        public bool IsSolid() {
            if (dashArray.Size % 2 != 0) {
                return false;
            }

            float unitsOffSum = 0;

            for (int i = 1; i < dashArray.Size; i += 2) {
                unitsOffSum += dashArray.GetAsNumber(i).FloatValue;
            }

            return Util.Compare(unitsOffSum, 0) == 0;
        }

        /**
         * Resets the dash array so that the {@link #next()} method will start
         * from the beginning of the dash array.
         */
        public void Reset() {
            currentIndex = 0;
            elemOrdinalNumber = 1;
            InitFirst(dashPhase);
        }

        private void InitFirst(float phase) {
            if (dashArray.Size > 0) {
                while (phase > 0) {
                    phase -= dashArray.GetAsNumber(currentIndex).FloatValue;
                    currentIndex = (currentIndex + 1) % DashArray.Size;
                    elemOrdinalNumber++;
                }

                if (phase < 0) {
                    --elemOrdinalNumber;
                    --currentIndex;
                    currentElem = new DashArrayElem(-phase, IsEven(elemOrdinalNumber));
                } else {
                    currentElem = new DashArrayElem(dashArray.GetAsNumber(currentIndex).FloatValue, 
                        IsEven(elemOrdinalNumber));
                }
            }
        }

        private bool IsEven(int num) {
            return (num % 2) == 0;
        }

        public class DashArrayElem {

            private float val;
            private bool isGap;

            public DashArrayElem(float val, bool isGap) {
                this.val = val;
                this.isGap = isGap;
            }

            public float Value
            {
                get { return val; }
                set { val = value; }
            }

            public bool IsGap
            {
                get { return isGap; }
                set { isGap = value; }
            }
        }
    }
}
