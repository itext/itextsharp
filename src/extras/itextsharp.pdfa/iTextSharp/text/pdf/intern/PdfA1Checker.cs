using System;
using iTextSharp.text.error_messages;

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