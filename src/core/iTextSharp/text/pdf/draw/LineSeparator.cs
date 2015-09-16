/*
 * 
 * $Id$
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

namespace iTextSharp.text.pdf.draw {

    /**
    * Element that draws a solid line from left to right.
    * Can be added directly to a document or column.
    * Can also be used to create a separator chunk.
    * @author   Paulo Soares
    * @since    2.1.2
    */
    public class LineSeparator : VerticalPositionMark {
        
        /** The thickness of the line. */
        protected float lineWidth = 1;
        /** The width of the line as a percentage of the available page width. */
        protected float percentage = 100;
        /** The color of the line. */
        protected BaseColor lineColor;
        /** The alignment of the line. */
        protected int alignment = Element.ALIGN_BOTTOM;
        
        /**
        * Creates a new instance of the LineSeparator class.
        * @param lineWidth      the thickness of the line
        * @param percentage the width of the line as a percentage of the available page width
        * @param color          the color of the line
        * @param align          the alignment
        * @param offset     the offset of the line relative to the current baseline (negative = under the baseline)
        */
        public LineSeparator(float lineWidth, float percentage, BaseColor lineColor, int align, float offset) {
            this.lineWidth = lineWidth;
            this.percentage = percentage;
            this.lineColor = lineColor;
            this.alignment = align;
            this.offset = offset;
        }

        /**
         * Creates a new instance of the LineSeparator class.
         * @param font			the font
         */
        public LineSeparator(Font font) {
            this.lineWidth = PdfChunk.UNDERLINE_THICKNESS*font.Size;
            this.offset = PdfChunk.UNDERLINE_OFFSET*font.Size;
            this.percentage = 100;
            this.lineColor = font.Color;
        }

        /**
        * Creates a new instance of the LineSeparator class with
        * default values: lineWidth 1 user unit, width 100%, centered with offset 0.
        */
        public LineSeparator() {
        }

        /**
        * @see com.lowagie.text.pdf.draw.DrawInterface#draw(com.lowagie.text.pdf.PdfContentByte, float, float, float, float, float)
        */
        public override void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y) {
            canvas.SaveState();
            DrawLine(canvas, llx, urx, y);
            canvas.RestoreState();
        }

        /**
        * Draws a horizontal line.
        * @param canvas the canvas to draw on
        * @param leftX      the left x coordinate
        * @param rightX the right x coordindate
        * @param y          the y coordinate
        */
        virtual public void DrawLine(PdfContentByte canvas, float leftX, float rightX, float y) {
            float w;
            if (Percentage < 0)
                w = -Percentage;
            else
                w = (rightX - leftX) * Percentage / 100.0f;
            float s;
            switch (Alignment) {
                case Element.ALIGN_LEFT:
                    s = 0;
                    break;
                case Element.ALIGN_RIGHT:
                    s = rightX - leftX - w;
                    break;
                default:
                    s = (rightX - leftX - w) / 2;
                    break;
            }
            canvas.SetLineWidth(LineWidth);
            if (LineColor != null)
                canvas.SetColorStroke(LineColor);
            canvas.MoveTo(s + leftX, y + offset);
            canvas.LineTo(s + w + leftX, y + offset);
            canvas.Stroke();
        }
        
        /**
        * Setter for the line width.
        * @param lineWidth  the thickness of the line that will be drawn.
        */
        virtual public float LineWidth {
            get {
                return lineWidth;
            }
            set {
                lineWidth = value;
            }
        }

        /**
        * Setter for the width as a percentage of the available width.
        * @return   a width percentage
        */
        virtual public float Percentage {
            get {
                return percentage;
            }
            set {
                percentage = value;
            }
        }

        /**
        * Setter for the color of the line that will be drawn.
        * @param color  a color
        */

        virtual public BaseColor LineColor {
            get {
                return lineColor;
            }
            set {
                lineColor = value;
            }
        }

        /**
        * Setter for the alignment of the line.
        * @param align  an alignment value
        */
        virtual public int Alignment {
            get {
                return alignment;
            }
            set {
                alignment = value;
            }
        }
    }
}
