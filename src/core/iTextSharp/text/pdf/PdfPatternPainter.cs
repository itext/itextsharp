using System;
using iTextSharp.text;
using iTextSharp.text.error_messages;

/*
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

namespace iTextSharp.text.pdf {

    /**
     * Implements the pattern.
     */

    public sealed class PdfPatternPainter : PdfTemplate {
    
        internal float xstep, ystep;
        internal bool stencil = false;
        internal BaseColor defaultColor;
    
        /**
         *Creates a <CODE>PdfPattern</CODE>.
         */
    
        private PdfPatternPainter() : base() {
            type = TYPE_PATTERN;
        }
    
        /**
         * Creates new PdfPattern
         *
         * @param wr the <CODE>PdfWriter</CODE>
         */
    
        internal PdfPatternPainter(PdfWriter wr) : base(wr) {
            type = TYPE_PATTERN;
        }
    
        internal PdfPatternPainter(PdfWriter wr, BaseColor defaultColor) : this(wr) {
            stencil = true;
            if (defaultColor == null)
                this.defaultColor = BaseColor.GRAY;
            else
                this.defaultColor = defaultColor;
        }
    
        public float XStep {
            get {
                return this.xstep;
            }

            set {
                this.xstep = value;
            }
        }
    
        public float YStep {
            get {
                return this.ystep;
            }

            set {
                this.ystep = value;
            }
        }
    
        public bool IsStencil() {
            return stencil;
        }
    
        public void SetPatternMatrix(float a, float b, float c, float d, float e, float f) {
            SetMatrix(a, b, c, d, e, f);
        }

        /**
        * Gets the stream representing this pattern
        * @return the stream representing this pattern
        */
        public PdfPattern GetPattern() {
            return new PdfPattern(this);
        }
        
        /**
        * Gets the stream representing this pattern
        * @param   compressionLevel    the compression level of the stream
        * @return the stream representing this pattern
        * @since   2.1.3
        */
        public PdfPattern GetPattern(int compressionLevel) {
            return new PdfPattern(this, compressionLevel);
        }
    
        /**
         * Gets a duplicate of this <CODE>PdfPatternPainter</CODE>. All
         * the members are copied by reference but the buffer stays different.
         * @return a copy of this <CODE>PdfPatternPainter</CODE>
         */
    
        public override PdfContentByte Duplicate {
            get {
                PdfPatternPainter tpl = new PdfPatternPainter();
                tpl.writer = writer;
                tpl.pdf = pdf;
                tpl.thisReference = thisReference;
                tpl.pageResources = pageResources;
                tpl.bBox = new Rectangle(bBox);
                tpl.xstep = xstep;
                tpl.ystep = ystep;
                tpl.matrix = matrix;
                tpl.stencil = stencil;
                tpl.defaultColor = defaultColor;
                return tpl;
            }
        }
    
        public BaseColor DefaultColor {
            get {
                return defaultColor;
            }
        }
    
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setGrayFill(float)
        */
        public override void SetGrayFill(float gray) {
            CheckNoColor();
            base.SetGrayFill(gray);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetGrayFill()
        */
        public override void ResetGrayFill() {
            CheckNoColor();
            base.ResetGrayFill();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setGrayStroke(float)
        */
        public override void SetGrayStroke(float gray) {
            CheckNoColor();
            base.SetGrayStroke(gray);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetGrayStroke()
        */
        public override void ResetGrayStroke() {
            CheckNoColor();
            base.ResetGrayStroke();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setRGBColorFillF(float, float, float)
        */
        public override void SetRGBColorFillF(float red, float green, float blue) {
            CheckNoColor();
            base.SetRGBColorFillF(red, green, blue);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetRGBColorFill()
        */
        public override void ResetRGBColorFill() {
            CheckNoColor();
            base.ResetRGBColorFill();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setRGBColorStrokeF(float, float, float)
        */
        public override void SetRGBColorStrokeF(float red, float green, float blue) {
            CheckNoColor();
            base.SetRGBColorStrokeF(red, green, blue);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetRGBColorStroke()
        */
        public override void ResetRGBColorStroke() {
            CheckNoColor();
            base.ResetRGBColorStroke();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setCMYKColorFillF(float, float, float, float)
        */
        public override void SetCMYKColorFillF(float cyan, float magenta, float yellow, float black) {
            CheckNoColor();
            base.SetCMYKColorFillF(cyan, magenta, yellow, black);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetCMYKColorFill()
        */
        public override void ResetCMYKColorFill() {
            CheckNoColor();
            base.ResetCMYKColorFill();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setCMYKColorStrokeF(float, float, float, float)
        */
        public override void SetCMYKColorStrokeF(float cyan, float magenta, float yellow, float black) {
            CheckNoColor();
            base.SetCMYKColorStrokeF(cyan, magenta, yellow, black);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#resetCMYKColorStroke()
        */
        public override void ResetCMYKColorStroke() {
            CheckNoColor();
            base.ResetCMYKColorStroke();
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#addImage(com.lowagie.text.Image, float, float, float, float, float, float)
        */
        public override void AddImage(Image image, float a, float b, float c, float d, float e, float f) {
            if (stencil && !image.IsMask())
                CheckNoColor();
            base.AddImage(image, a, b, c, d, e, f);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setCMYKColorFill(int, int, int, int)
        */
        public override void SetCMYKColorFill(int cyan, int magenta, int yellow, int black) {
            CheckNoColor();
            base.SetCMYKColorFill(cyan, magenta, yellow, black);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setCMYKColorStroke(int, int, int, int)
        */
        public override void SetCMYKColorStroke(int cyan, int magenta, int yellow, int black) {
            CheckNoColor();
            base.SetCMYKColorStroke(cyan, magenta, yellow, black);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setRGBColorFill(int, int, int)
        */
        public override void SetRGBColorFill(int red, int green, int blue) {
            CheckNoColor();
            base.SetRGBColorFill(red, green, blue);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setRGBColorStroke(int, int, int)
        */
        public override void SetRGBColorStroke(int red, int green, int blue) {
            CheckNoColor();
            base.SetRGBColorStroke(red, green, blue);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setColorStroke(java.awt.Color)
        */
        public override void SetColorStroke(BaseColor color) {
            CheckNoColor();
            base.SetColorStroke(color);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setColorFill(java.awt.Color)
        */
        public override void SetColorFill(BaseColor color) {
            CheckNoColor();
            base.SetColorFill(color);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setColorFill(com.lowagie.text.pdf.PdfSpotColor, float)
        */
        public override void SetColorFill(PdfSpotColor sp, float tint) {
            CheckNoColor();
            base.SetColorFill(sp, tint);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setColorStroke(com.lowagie.text.pdf.PdfSpotColor, float)
        */
        public override void SetColorStroke(PdfSpotColor sp, float tint) {
            CheckNoColor();
            base.SetColorStroke(sp, tint);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setPatternFill(com.lowagie.text.pdf.PdfPatternPainter)
        */
        public override void SetPatternFill(PdfPatternPainter p) {
            CheckNoColor();
            base.SetPatternFill(p);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setPatternFill(com.lowagie.text.pdf.PdfPatternPainter, java.awt.Color, float)
        */
        public override void SetPatternFill(PdfPatternPainter p, BaseColor color, float tint) {
            CheckNoColor();
            base.SetPatternFill(p, color, tint);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setPatternStroke(com.lowagie.text.pdf.PdfPatternPainter, java.awt.Color, float)
        */
        public override void SetPatternStroke(PdfPatternPainter p, BaseColor color, float tint) {
            CheckNoColor();
            base.SetPatternStroke(p, color, tint);
        }
        
        /**
        * @see com.lowagie.text.pdf.PdfContentByte#setPatternStroke(com.lowagie.text.pdf.PdfPatternPainter)
        */
        public override void SetPatternStroke(PdfPatternPainter p) {
            CheckNoColor();
            base.SetPatternStroke(p);
        }
    
        internal void CheckNoColor() {
            if (stencil)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("colors.are.not.allowed.in.uncolored.tile.patterns"));
        }
    }
}
