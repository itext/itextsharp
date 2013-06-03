using System;

namespace iTextSharp.text.pdf.intern
{
    public abstract class PdfAChecker {
        protected abstract void CheckFont(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckImage(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckGState(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckLayer(PdfWriter writer, int key, Object obj1);

        internal void CheckPdfAConformance(PdfWriter writer, int key, Object obj1) {
            if(writer == null || !writer.IsPdfIso())
                return;
            switch(key) {
                case PdfIsoKeys.PDFISOKEY_FONT:
                    CheckFont(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_IMAGE:
                    CheckImage(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_GSTATE:
                    CheckGState(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_LAYER:
                    CheckLayer(writer, key, obj1);
                    break;
                default:
                    break;
            }
        }
    }
}