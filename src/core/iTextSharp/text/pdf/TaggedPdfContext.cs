using System.Collections.Generic;
using iTextSharp.text;

namespace com.itextpdf.text.pdf{

    public class TaggedPdfContext{

        protected internal Dictionary<ListItem, PdfListBody> lBodies = new Dictionary<ListItem, PdfListBody>();

    }
}