using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Stores parameters related to specific context which is either page or xobject
     * (in other words: which is the object having contents stream)
     */
    class PdfCleanUpContext {

        private PdfDictionary resources;
        private PdfContentByte canvas;

        public PdfCleanUpContext(PdfDictionary resources, PdfContentByte canvas) {
            this.resources = resources;
            this.canvas = canvas;
        }

        public PdfDictionary Resources {
            get { return resources; }
            set { resources = value; }
        }

        public PdfContentByte Canvas {
            get { return canvas; }
            set { canvas = value; }
        }
    }
}
