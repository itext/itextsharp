using System;
using iTextSharp.text.error_messages;
/*
 * $Id: PdfA1Checker.java 5827 2013-05-31 08:56:23Z blowagie $
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
namespace iTextSharp.text.pdf.intern
{
public class PdfA1Checker : PdfAChecker {

    protected override void CheckFont(PdfWriter writer, int key, Object obj1) {
        BaseFont bf = (BaseFont) obj1;
        if (bf.FontType == BaseFont.FONT_TYPE_DOCUMENT) {
            PdfStream prs = null;
            PdfDictionary fontDictionary = ((DocumentFont) bf).FontDictionary;
            PdfDictionary fontDescriptor = fontDictionary.GetAsDict(PdfName.FONTDESCRIPTOR);
            if (fontDescriptor != null) {
                prs = fontDescriptor.GetAsStream(PdfName.FONTFILE);
                if (prs == null) {
                    prs = fontDescriptor.GetAsStream(PdfName.FONTFILE2);
                }
                if (prs == null) {
                    prs = fontDescriptor.GetAsStream(PdfName.FONTFILE3);
                }
            }
            if (prs == null) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont) obj1).PostscriptFontName));
            }
        } else {
            if (!bf.IsEmbedded())
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont) obj1).PostscriptFontName));
        }
    }

    protected override void CheckImage(PdfWriter writer, int key, Object obj1) {
        PdfImage image = (PdfImage) obj1;
        if (image.Get(PdfName.SMASK) != null)
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("the.smask.key.is.not.allowed.in.images"));
    }

    protected override void CheckGState(PdfWriter writer, int key, Object obj1) {
        PdfDictionary gs = (PdfDictionary) obj1;
        PdfObject obj = gs.Get(PdfName.BM);
        if (obj != null && !PdfGState.BM_NORMAL.Equals(obj) && !PdfGState.BM_COMPATIBLE.Equals(obj))
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", obj.ToString()));
        obj = gs.Get(PdfName.CA);
        double v = 0.0;
        if (obj != null && (v = ((PdfNumber) obj).DoubleValue) != 1.0)
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v));
        obj = gs.Get(PdfName.ca_);
        v = 0.0;
        if (obj != null && (v = ((PdfNumber) obj).DoubleValue) != 1.0)
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", v));
    }

    protected override void CheckLayer(PdfWriter writer, int key, Object obj1) {
        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("layers.are.not.allowed"));
    }

}

}