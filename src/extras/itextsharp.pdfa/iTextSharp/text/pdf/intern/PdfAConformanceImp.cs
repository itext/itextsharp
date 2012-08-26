using System;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.intern;
using iTextSharp.text.pdf.interfaces;

/*
 * $Id: PdfAConformanceImp.java 322 2012-07-23 09:58:41Z bruno $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Alexander Chingarev, Bruno Lowagie, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
namespace iTextSharp.text.pdf.intern{
    /**
     * Implementation of the PdfAConformance interface,
     * including the level of conformance.
     * @see PdfAConformance
     */
    public class PdfAConformanceImp : IPdfAConformance {

        /** The PDF conformance level, e.g. PDF/A1 Level A, PDF/A3 Level U,... */
        private PdfAConformanceLevel conformanceLevel;

        /**
         *
         * @param writer
         * @param key
         * @param obj1
         */
        static public void CheckPdfAConformance(PdfWriter writer, int key, Object obj1) {
            if (writer == null || !writer.IsPdfIso())
                return;
            switch (key) {
                case PdfIsoKeys.PDFISOKEY_FONT:
                    if (!((BaseFont)obj1).IsEmbedded())
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont) obj1).PostscriptFontName));
                    break;
                case PdfIsoKeys.PDFISOKEY_IMAGE:
                    PdfImage image = (PdfImage)obj1;
                    if (image.Get(PdfName.SMASK) != null)
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.smask.key.is.not.allowed.in.images"));
                    break;
                case PdfIsoKeys.PDFISOKEY_GSTATE:
                    PdfDictionary gs = (PdfDictionary)obj1;
                    PdfObject obj = gs.Get(PdfName.BM);
                    if (obj != null && !PdfGState.BM_NORMAL.Equals(obj) && !PdfGState.BM_COMPATIBLE.Equals(obj))
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", obj.ToString()));
                    obj = gs.Get(PdfName.CA);
                    double v = 0.0;
                    if (obj != null && (v = ((PdfNumber)obj).DoubleValue) != 1.0)
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v));
                    obj = gs.Get(PdfName.ca_);
                    v = 0.0;
                    if (obj != null && (v = ((PdfNumber)obj).DoubleValue) != 1.0)
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v));
                    break;
                case PdfIsoKeys.PDFISOKEY_LAYER:
                    throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("layers.are.not.allowed"));
                default:
                    break;
            }
        }

        /**
         * @see com.itextpdf.text.pdf.interfaces.PdfAConformance#getConformanceLevel()
         */
        public PdfAConformanceLevel GetConformanceLevel() {
            return conformanceLevel;
        }

        /**
         * @see PdfAConformance#setConformanceLevel(com.itextpdf.text.pdf.PdfAConformanceLevel)
         */
        public void SetConformanceLevel(PdfAConformanceLevel conformanceLevel) {
            this.conformanceLevel = conformanceLevel;
        }

        /**
         * @see com.itextpdf.text.pdf.interfaces.PdfAConformance#isPdfIso()
         */
        public bool IsPdfIso() {
            return true;
        }

    }
}
