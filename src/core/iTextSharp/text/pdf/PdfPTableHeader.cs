using iTextSharp.text.pdf;

namespace iTextSharp.text.pdf {

    public class PdfPTableHeader : PdfPTableBody {

        protected new PdfName role = PdfName.THEAD;

        public PdfPTableHeader()
            : base() {
        }

        public new PdfName Role {
            get { return role; }
            set { this.role = value; }
        }
    }
}