using System;
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
    /** Base class containing properties and methods commom to all
     * barcode types.
     *
     * @author Paulo Soares
     */
    public abstract class Barcode {
        /** A type of barcode */
        public const int EAN13 = 1;
        /** A type of barcode */
        public const int EAN8 = 2;
        /** A type of barcode */
        public const int UPCA = 3;
        /** A type of barcode */
        public const int UPCE = 4;
        /** A type of barcode */
        public const int SUPP2 = 5;
        /** A type of barcode */
        public const int SUPP5 = 6;
        /** A type of barcode */
        public const int POSTNET = 7;
        /** A type of barcode */
        public const int PLANET = 8;
        /** A type of barcode */
        public const int CODE128 = 9;
        /** A type of barcode */
        public const int CODE128_UCC = 10;
        /** A type of barcode */
        public const int CODE128_RAW = 11;
        /** A type of barcode */
        public const int CODABAR = 12;

        /** The minimum bar width.
         */
        protected float x;    

        /** The bar multiplier for wide bars or the distance between
         * bars for Postnet and Planet.
         */
        protected float n;
    
        /** The text font. <CODE>null</CODE> if no text.
         */
        protected BaseFont font;

        /** The size of the text or the height of the shorter bar
         * in Postnet.
         */    
        protected float size;
    
        /** If positive, the text distance under the bars. If zero or negative,
         * the text distance above the bars.
         */
        protected float baseline;
    
        /** The height of the bars.
         */
        protected float barHeight;
    
        /** The text Element. Can be <CODE>Element.ALIGN_LEFT</CODE>,
         * <CODE>Element.ALIGN_CENTER</CODE> or <CODE>Element.ALIGN_RIGHT</CODE>.
         */
        protected int textAlignment;
    
        /** The optional checksum generation.
         */
        protected bool generateChecksum;
    
        /** Shows the generated checksum in the the text.
         */
        protected bool checksumText;
    
        /** Show the start and stop character '*' in the text for
         * the barcode 39 or 'ABCD' for codabar.
         */
        protected bool startStopText;
    
        /** Generates extended barcode 39.
         */
        protected bool extended;
    
        /** The code to generate.
         */
        protected string code = "";
    
        /** Show the guard bars for barcode EAN.
         */
        protected bool guardBars;
    
        /** The code type.
         */
        protected int codeType;
    
        /** The ink spreading. */
        protected float inkSpreading = 0;

        /** Gets the minimum bar width.
         * @return the minimum bar width
         */
        virtual public float X {
            get {
                return x;
            }

            set {
                this.x = value;
            }
        }
    
        /** Gets the bar multiplier for wide bars.
         * @return the bar multiplier for wide bars
         */
        virtual public float N {
            get {
                return n;
            }

            set {
                this.n = value;
            }
        }
    
        /** Gets the text font. <CODE>null</CODE> if no text.
         * @return the text font. <CODE>null</CODE> if no text
         */
        virtual public BaseFont Font {
            get {
                return font;
            }

            set {
                this.font = value;
            }
        }
    
        /** Gets the size of the text.
         * @return the size of the text
         */
        virtual public float Size {
            get {
                return size;
            }

            set {
                this.size = value;
            }
        }
    
        /** Gets the text baseline.
         * If positive, the text distance under the bars. If zero or negative,
         * the text distance above the bars.
         * @return the baseline.
         */
        virtual public float Baseline {
            get {
                return baseline;
            }

            set {
                this.baseline = value;
            }
        }
    
        /** Gets the height of the bars.
         * @return the height of the bars
         */
        virtual public float BarHeight {
            get {
                return barHeight;
            }

            set {
                this.barHeight = value;
            }
        }
    
        /** Gets the text Element. Can be <CODE>Element.ALIGN_LEFT</CODE>,
         * <CODE>Element.ALIGN_CENTER</CODE> or <CODE>Element.ALIGN_RIGHT</CODE>.
         * @return the text alignment
         */
        virtual public int TextAlignment{
            get {
                return textAlignment;
            }

            set {
                this.textAlignment = value;
            }
        }
    
        /** The property for the optional checksum generation.
         */
        virtual public bool GenerateChecksum {
            set {
                this.generateChecksum = value;
            }
            get {
                return generateChecksum;
            }
        }
    
        /** Sets the property to show the generated checksum in the the text.
         * @param checksumText new value of property checksumText
         */
        virtual public bool ChecksumText {
            set {
                this.checksumText = value;
            }
            get {
                return checksumText;
            }
        }
    
        /** Gets the property to show the start and stop character '*' in the text for
         * the barcode 39.
         * @param startStopText new value of property startStopText
         */
        virtual public bool StartStopText {
            set {
                this.startStopText = value;
            }
            get {
                return startStopText;
            }
        }
    
        /** Sets the property to generate extended barcode 39.
         * @param extended new value of property extended
         */
        virtual public bool Extended {
            set {
                this.extended = value;
            }
            get {
                return extended;
            }
        }
    
        /** Gets the code to generate.
         * @return the code to generate
         */
        public virtual string Code {
            get {
                return code;
            }

            set {
                this.code = value;
            }
        }
    
        /** Sets the property to show the guard bars for barcode EAN.
         * @param guardBars new value of property guardBars
         */
        virtual public bool GuardBars {
            set {
                this.guardBars = value;
            }
            get {
                return guardBars;
            }
        }
    
        /** Gets the code type.
         * @return the code type
         */
        virtual public int CodeType {
            get {
                return codeType;
            }

            set {
                this.codeType = value;
            }
        }
    
        /** Gets the maximum area that the barcode and the text, if
         * any, will occupy. The lower left corner is always (0, 0).
         * @return the size the barcode occupies.
         */    
        public abstract Rectangle BarcodeSize {
            get;
        }

        virtual public float InkSpreading {
            set {
                inkSpreading = value;
            }
            get {
                return inkSpreading;
            }
        }
    
        /** Places the barcode in a <CODE>PdfContentByte</CODE>. The
         * barcode is always placed at coodinates (0, 0). Use the
         * translation matrix to move it elsewhere.<p>
         * The bars and text are written in the following colors:<p>
         * <P><TABLE BORDER=1>
         * <TR>
         *    <TH><P><CODE>barColor</CODE></TH>
         *    <TH><P><CODE>textColor</CODE></TH>
         *    <TH><P>Result</TH>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P>bars and text painted with current fill color</TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>barColor</CODE></TD>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P>bars and text painted with <CODE>barColor</CODE></TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>null</CODE></TD>
         *    <TD><P><CODE>textColor</CODE></TD>
         *    <TD><P>bars painted with current color<br>text painted with <CODE>textColor</CODE></TD>
         *    </TR>
         * <TR>
         *    <TD><P><CODE>barColor</CODE></TD>
         *    <TD><P><CODE>textColor</CODE></TD>
         *    <TD><P>bars painted with <CODE>barColor</CODE><br>text painted with <CODE>textColor</CODE></TD>
         *    </TR>
         * </TABLE>
         * @param cb the <CODE>PdfContentByte</CODE> where the barcode will be placed
         * @param barColor the color of the bars. It can be <CODE>null</CODE>
         * @param textColor the color of the text. It can be <CODE>null</CODE>
         * @return the dimensions the barcode occupies
         */    
        public abstract Rectangle PlaceBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor);
    
        /** Creates a template with the barcode.
         * @param cb the <CODE>PdfContentByte</CODE> to create the template. It
         * serves no other use
         * @param barColor the color of the bars. It can be <CODE>null</CODE>
         * @param textColor the color of the text. It can be <CODE>null</CODE>
         * @return the template
         * @see #placeBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor)
         */    
        virtual public PdfTemplate CreateTemplateWithBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor) {
            PdfTemplate tp = cb.CreateTemplate(0, 0);
            Rectangle rect = PlaceBarcode(tp, barColor, textColor);
            tp.BoundingBox = rect;
            return tp;
        }
    
        /** Creates an <CODE>Image</CODE> with the barcode.
         * @param cb the <CODE>PdfContentByte</CODE> to create the <CODE>Image</CODE>. It
         * serves no other use
         * @param barColor the color of the bars. It can be <CODE>null</CODE>
         * @param textColor the color of the text. It can be <CODE>null</CODE>
         * @return the <CODE>Image</CODE>
         * @see #placeBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor)
         */    
        virtual public Image CreateImageWithBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor) {
            return Image.GetInstance(CreateTemplateWithBarcode(cb, barColor, textColor));
        }

        /**
        * The alternate text to be used, if present.
        */
        protected String altText;

        /**
        * Sets the alternate text. If present, this text will be used instead of the
        * text derived from the supplied code.
        * @param altText the alternate text
        */
        virtual public String AltText {
            set {
                altText = value;
            }
            get {
                return altText;
            }
        }

#if DRAWING
        public abstract System.Drawing.Image CreateDrawingImage(System.Drawing.Color foreground, System.Drawing.Color background);
#endif// DRAWING
    }

}
