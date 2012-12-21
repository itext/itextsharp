using iTextSharp.text.pdf;

using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf {

    public class PdfPTableFooter : PdfPTableBody {

        protected new PdfName role = PdfName.TFOOT;

        public PdfPTableFooter() : base() {
        }

        public new PdfName Role {
            get { return role; }
            set { this.role = value; }
        }
    }
}
