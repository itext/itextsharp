using iTextSharp.text.pdf.intern;

/*
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
    /** The graphic state dictionary.
    *
    * @author Paulo Soares
    */
    public class PdfGState : PdfDictionary {
        /** A possible blend mode */
        public static PdfName BM_NORMAL = new PdfName("Normal");
        /** A possible blend mode */
        public static PdfName BM_COMPATIBLE = new PdfName("Compatible");
        /** A possible blend mode */
        public static PdfName BM_MULTIPLY = new PdfName("Multiply");
        /** A possible blend mode */
        public static PdfName BM_SCREEN = new PdfName("Screen");
        /** A possible blend mode */
        public static PdfName BM_OVERLAY = new PdfName("Overlay");
        /** A possible blend mode */
        public static PdfName BM_DARKEN = new PdfName("Darken");
        /** A possible blend mode */
        public static PdfName BM_LIGHTEN = new PdfName("Lighten");
        /** A possible blend mode */
        public static PdfName BM_COLORDODGE = new PdfName("ColorDodge");
        /** A possible blend mode */
        public static PdfName BM_COLORBURN = new PdfName("ColorBurn");
        /** A possible blend mode */
        public static PdfName BM_HARDLIGHT = new PdfName("HardLight");
        /** A possible blend mode */
        public static PdfName BM_SOFTLIGHT = new PdfName("SoftLight");
        /** A possible blend mode */
        public static PdfName BM_DIFFERENCE = new PdfName("Difference");
        /** A possible blend mode */
        public static PdfName BM_EXCLUSION = new PdfName("Exclusion");
        
        /**
        * Sets the flag whether to apply overprint for stroking.
        * @param ov
        */
        virtual public bool OverPrintStroking {
            set {
                Put(PdfName.OP, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }

        /**
        * Sets the flag whether to apply overprint for non stroking painting operations.
        * @param ov
        */
        virtual public bool OverPrintNonStroking {
            set {
                Put(PdfName.op_, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }
        
        /**
        * Sets the flag whether to toggle knockout behavior for overprinted objects.
        * @param ov - accepts 0 or 1
        */
        virtual public int OverPrintMode {
            set {
                Put(PdfName.OPM, new PdfNumber(value == 0 ? 0 : 1));
            }
        }

        /**
        * Sets the current stroking alpha constant, specifying the constant shape or
        * constant opacity value to be used for stroking operations in the transparent
        * imaging model.
        * @param n
        */
        virtual public float StrokeOpacity {
            set {
                Put(PdfName.CA, new PdfNumber(value));
            }
        }
        
        /**
        * Sets the current stroking alpha constant, specifying the constant shape or
        * constant opacity value to be used for nonstroking operations in the transparent
        * imaging model.
        * @param n
        */
        virtual public float FillOpacity {
            set {
                Put(PdfName.ca, new PdfNumber(value));
            }
        }
        
        /**
        * The alpha source flag specifying whether the current soft mask
        * and alpha constant are to be interpreted as shape values (true)
        * or opacity values (false). 
        * @param v
        */
        virtual public bool AlphaIsShape {
            set {
                Put(PdfName.AIS, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }
        
        /**
        * Determines the behaviour of overlapping glyphs within a text object
        * in the transparent imaging model.
        * @param v
        */
        virtual public bool TextKnockout {
            set {
                Put(PdfName.TK, value ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
            }
        }
        
        /**
        * The current blend mode to be used in the transparent imaging model.
        * @param bm
        */
        virtual public PdfName BlendMode {
            set {
                Put(PdfName.BM, value);
            }
        }
        
        /**
         * Set the rendering intent, possible values are: PdfName.ABSOLUTECOLORIMETRIC,
         * PdfName.RELATIVECOLORIMETRIC, PdfName.SATURATION, PdfName.PERCEPTUAL.
         * @param ri
         */
        virtual public PdfName RenderingIntent {
            set {
                Put(PdfName.RI, value);
            }
        }

        public override void ToPdf(PdfWriter writer, System.IO.Stream os) {
            PdfWriter.CheckPdfIsoConformance(writer, PdfIsoKeys.PDFISOKEY_GSTATE, this);
            base.ToPdf(writer, os);
        }
    }
}
