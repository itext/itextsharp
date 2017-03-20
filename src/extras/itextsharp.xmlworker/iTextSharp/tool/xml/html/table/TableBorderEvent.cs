/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace iTextSharp.tool.xml.html.table {

    /**
     * @author Emiel Ackermann
     *
     */
    public class TableBorderEvent : IPdfPTableEvent{
        /**
         *
         */
        private TableStyleValues styleValues;

        /**
         * Construct a new TableBorderEvent with the given TableStyleValues.
         *
         * @param styleValues the TableStyleValues
         * @see com.itextpdf.text.pdf.PdfPTableEvent#tableLayout(com.itextpdf.text.pdf.PdfPTable,
         *      float[][], float[], int, int,
         *      com.itextpdf.text.pdf.PdfContentByte[])
         */
        public TableBorderEvent(TableStyleValues styleValues) {
            this.styleValues = styleValues;
        }

        virtual public void TableLayout(PdfPTable table, float[][] width, float[] height,
                int headerRows, int rowStart, PdfContentByte[] canvas) {
            float left = styleValues.BorderWidthLeft;
            float right = styleValues.BorderWidthRight;
            float top = styleValues.BorderWidthTop;
            float bottom = styleValues.BorderWidthBottom;
            float[] widths = width[0];
            float effectivePadding = left/2;
            float x1 = widths[0]-effectivePadding;
            effectivePadding = right/2;
            float x2 = widths[widths.Length - 1]+effectivePadding;
            effectivePadding = top/2;
            float y1 = height[0]+effectivePadding;
            effectivePadding = bottom/2+styleValues.VerBorderSpacing;
            float y2 = height[height.Length - 1]-effectivePadding;
            PdfContentByte cb = canvas[PdfPTable.BACKGROUNDCANVAS];
            BaseColor color = styleValues.Background;
            if (color != null) {
                cb.SetColorFill(color);
                cb.Rectangle(x1, y1, x2-x1, y2-y1);
                cb.Fill();
            }
            cb = canvas[PdfPTable.LINECANVAS];
            if (left != 0) {
                color = styleValues.BorderColorLeft;
                if (color == null) {
                    color = BaseColor.BLACK;
                }
                cb.SetLineWidth(left);
                cb.SetColorStroke(color);
                cb.MoveTo(x1, y1); // start leftUpperCorner
                cb.LineTo(x1, y2); // left
                cb.Stroke();
            }
            if (bottom != 0) {
                color = styleValues.BorderColorBottom;
                if (color == null) {
                    color = BaseColor.BLACK;
                }
                cb.SetLineWidth(bottom);
                cb.SetColorStroke(color);
                cb.MoveTo(x1, y2); // left
                cb.LineTo(x2, y2); // bottom
                cb.Stroke();
            }
            if (right != 0) {
                color = styleValues.BorderColorRight;
                if (color == null) {
                    color = BaseColor.BLACK;
                }
                cb.SetLineWidth(right);
                cb.SetColorStroke(color);
                cb.MoveTo(x2, y2); // bottom
                cb.LineTo(x2, y1); // right
                cb.Stroke();
            }
            if (top != 0) {
                color = styleValues.BorderColorTop;
                if (color == null) {
                    color = BaseColor.BLACK;
                }
                cb.SetLineWidth(top);
                cb.SetColorStroke(color);
                cb.MoveTo(x2, y1); // right
                cb.LineTo(x1, y1); // top
                cb.Stroke();
            }
            cb.ResetRGBColorStroke();
        }

        /**
         * Returns the used TableStyleValues
         * @return TableStyleValues
         */
        virtual public TableStyleValues TableStyleValues {
            get {
                return this.styleValues;
            }
        }
    }
}
