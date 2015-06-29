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

        /**
         * PdfContentStreamProcessor is able to process only Device* color spaces,
         * so I had to add this workaround.
         */
        private Stack<IList<PdfObject>> strokeColorOperands;

        public PdfCleanUpContext() {
            IList<PdfObject> initialStrokeColor = new List<PdfObject>(new PdfObject[] { PdfName.DEVICEGRAY, new PdfLiteral("CS") });
            strokeColorOperands = new Stack<IList<PdfObject>>();
            strokeColorOperands.Push(initialStrokeColor);
        }

        public PdfCleanUpContext(PdfDictionary resources, PdfContentByte canvas) : this() {
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

        public void PushStrokeColor(IList<PdfObject> strokeColorOperands) {
            this.strokeColorOperands.Push(new List<PdfObject>(strokeColorOperands));
        }

        public IList<PdfObject> PeekStrokeColor() {
            if (strokeColorOperands.Count == 0) {
                return null;
            } else {
                return strokeColorOperands.Peek();
            }
        }

        public IList<PdfObject> PopStrokeColor() {
            return strokeColorOperands.Pop();
        }
    }
}
