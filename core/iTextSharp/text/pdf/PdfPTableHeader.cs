namespace iTextSharp.text.pdf {

    public class PdfPTableHeader : PdfPTableBody {

        public PdfPTableHeader()
            : base() {
                role = PdfName.THEAD;
        }

        public override PdfName Role {
            get { return role; }
            set { this.role = value; }
        }
    }
}