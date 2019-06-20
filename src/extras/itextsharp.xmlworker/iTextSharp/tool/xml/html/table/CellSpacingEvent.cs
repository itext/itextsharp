/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
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
    public class CellSpacingEvent : IPdfPCellEvent {
        private TableStyleValues styleValues;

        /**
         * Construct a new CellSpacingEvent with the given TableStyleValues
         * @param styleValues the TableStyleValues
         */
        public CellSpacingEvent(TableStyleValues styleValues) {
            this.styleValues = styleValues;
        }

        /**
         * @see com.itextpdf.text.pdf.PdfPCellEvent#cellLayout(com.itextpdf.text.pdf.PdfPCell,
         *      com.itextpdf.text.Rectangle, com.itextpdf.text.pdf.PdfContentByte[])
         */
        virtual public void CellLayout(PdfPCell cell, Rectangle position,
                PdfContentByte[] canvases) {
            float effectivePadding = styleValues.BorderWidthLeft/2 + styleValues.HorBorderSpacing;
            float x1 = position.Left + effectivePadding;
            if (styleValues.IsLastInRow){
                effectivePadding = styleValues.BorderWidthRight/2 + styleValues.HorBorderSpacing;
            } else {
                effectivePadding = styleValues.BorderWidthRight/2;
            }
            float x2 = position.Right - effectivePadding;
            effectivePadding = styleValues.BorderWidthTop/2 + styleValues.VerBorderSpacing;
            float y1 = position.Top - effectivePadding;
            effectivePadding = styleValues.BorderWidthBottom/2;
            float y2 = position.Bottom + effectivePadding;
            PdfContentByte cb = canvases[PdfPTable.LINECANVAS];
            BaseColor color = styleValues.Background;
            if (color != null) {
                cb.SetColorStroke(color);
                cb.SetColorFill(color);
                cb.Rectangle(x1, y1, x2-x1, y2-y1);
                cb.Fill();
            }
            BaseColor borderColor = styleValues.BorderColorLeft;
            float width = styleValues.BorderWidthLeft;
            if (borderColor != null && width != 0) {
                cb.SetLineWidth(width);
                cb.SetColorStroke(borderColor);
                cb.MoveTo(x1, y1); // start leftUpperCorner
                cb.LineTo(x1, y2); // left
                cb.Stroke();
            }
            borderColor = styleValues.BorderColorBottom;
            width = styleValues.BorderWidthBottom;
            if (borderColor != null && width != 0) {
                cb.SetLineWidth(width);
                cb.SetColorStroke(borderColor);
                cb.MoveTo(x1, y2); // left
                cb.LineTo(x2, y2); // bottom
                cb.Stroke();
            }
            borderColor = styleValues.BorderColorRight;
            width = styleValues.BorderWidthRight;
            if (borderColor != null && width != 0) {
                cb.SetLineWidth(width);
                cb.SetColorStroke(borderColor);
                cb.MoveTo(x2, y2); // bottom
                cb.LineTo(x2, y1); // right
                cb.Stroke();
            }
            borderColor = styleValues.BorderColorTop;
            width = styleValues.BorderWidthTop;
            if (borderColor != null && width != 0) {
                cb.SetLineWidth(width);
                cb.SetColorStroke(borderColor);
                cb.MoveTo(x2, y1); // right
                cb.LineTo(x1, y1); // top
                cb.Stroke();
            }
            cb.ResetRGBColorStroke();
        }
    }
}
