using System;

namespace iTextSharp.text.pdf {
    public interface IPdfSpecialColorSpace {
        PdfObject GetPdfObject(PdfWriter writer);
        bool Equals(Object obj);
    }
}
