using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpContext {

        private PdfDictionary resources;
        private PdfContentByte canvas;

        private Stack<PdfCleanUpGraphicsState> graphicsStateStack = new Stack<PdfCleanUpGraphicsState>();

        public PdfCleanUpContext(PdfDictionary resources, PdfContentByte canvas) {
            this.resources = resources;
            this.canvas = canvas;

            graphicsStateStack.Push(new PdfCleanUpGraphicsState());
        }

        public PdfDictionary Resources {
            get { return resources; }
            set { resources = value; }
        }

        public PdfContentByte Canvas {
            get { return canvas; }
            set { canvas = value; }
        }

        public virtual float FontSize {
            get { return graphicsStateStack.Peek().FontSize; }
            set { graphicsStateStack.Peek().FontSize = value; }
        }

        public virtual float HorizontalScaling {
            get { return graphicsStateStack.Peek().HorizontalScaling; }
            set { graphicsStateStack.Peek().HorizontalScaling = value; }
        }

        public virtual float CharacterSpacing {
            get { return graphicsStateStack.Peek().CharacterSpacing; }
            set { graphicsStateStack.Peek().CharacterSpacing = value; }
        }

        public virtual float WordSpacing {
            get { return graphicsStateStack.Peek().WordSpacing; }
            set { graphicsStateStack.Peek().WordSpacing = value; }
        }

        public void SaveGraphicsState() {
            graphicsStateStack.Push(new PdfCleanUpGraphicsState(graphicsStateStack.Peek()));
        }

        public void RestoreGraphicsState() {
            graphicsStateStack.Pop();
        }
    }
}
