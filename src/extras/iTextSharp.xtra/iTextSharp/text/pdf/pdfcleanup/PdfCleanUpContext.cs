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

        public float LineWidth {
            get { return graphicsStateStack.Peek().LineWidth; }
            set { graphicsStateStack.Peek().LineWidth = value; }
        }

        public int LineCapStyle {
            get { return graphicsStateStack.Peek().LineCapStyle; }
            set { graphicsStateStack.Peek().LineCapStyle = value; }
        }

        public int LineJoinStyle {
            get { return graphicsStateStack.Peek().LineJoinStyle; }
            set { graphicsStateStack.Peek().LineJoinStyle = value; }
        }

        public float MiterLimit {
            get { return graphicsStateStack.Peek().MiterLimit; }
            set { graphicsStateStack.Peek().MiterLimit = value; }
        }

        /**
         * @return {@link LineDashPattern} object, describing the dash pattern which should be applied.
         *         If no pattern should be applied (i.e. solid line), then returns <CODE>null</CODE>.
         */
        public virtual LineDashPattern GetLineDashPattern() {
            return graphicsStateStack.Peek().GetLineDashPattern();
        }

        public virtual void SetLineDashPattern(LineDashPattern lineDashPattern) {
            graphicsStateStack.Peek().SetLineDashPattern(lineDashPattern);
        }

        public void SaveGraphicsState() {
            graphicsStateStack.Push(new PdfCleanUpGraphicsState(graphicsStateStack.Peek()));
        }

        public void RestoreGraphicsState() {
            graphicsStateStack.Pop();
        }
    }
}
