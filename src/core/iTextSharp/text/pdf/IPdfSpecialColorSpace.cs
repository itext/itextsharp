namespace iTextSharp.text.pdf {
    public interface IPdfSpecialColorSpace {
        ColorDetails[] GetColorantDetails(PdfWriter writer);
    }
}
