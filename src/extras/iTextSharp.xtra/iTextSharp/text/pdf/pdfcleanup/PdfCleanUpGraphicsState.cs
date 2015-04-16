using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Represents subset of graphics state parameters.
     */
    class PdfCleanUpGraphicsState {

        private float fontSize = 1;
        private float horizontalScaling = 100; // in percents
        private float characterSpacing;
        private float wordSpacing;
        private float lineWidth = 1.0f;
        private int lineCapStyle = PdfContentByte.LINE_CAP_BUTT;
        private int lineJoinStyle = PdfContentByte.LINE_JOIN_MITER;
        private float miterLimit = 10.0f;
        private LineDashPattern lineDashPattern;

        public PdfCleanUpGraphicsState() {
        }

        public PdfCleanUpGraphicsState(float fontSize, float horizontalScaling, float characterSpacing, float wordSpacing) {
            this.fontSize = fontSize;
            this.horizontalScaling = horizontalScaling;
            this.characterSpacing = characterSpacing;
            this.wordSpacing = wordSpacing;
        }

        public PdfCleanUpGraphicsState(PdfCleanUpGraphicsState graphicsState) {
            this.fontSize = graphicsState.fontSize;
            this.horizontalScaling = graphicsState.horizontalScaling;
            this.characterSpacing = graphicsState.characterSpacing;
            this.wordSpacing = graphicsState.wordSpacing;
        }

        public virtual float FontSize {
            get { return fontSize; }
            set { fontSize = value; }
        }

        public virtual float HorizontalScaling {
            get { return horizontalScaling; }
            set { horizontalScaling = value; }
        }

        public virtual float CharacterSpacing {
            get { return characterSpacing; }
            set { characterSpacing = value; }
        }

        public virtual float WordSpacing {
            get { return wordSpacing; }
            set { wordSpacing = value; }
        }

        public float LineWidth {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        public int LineCapStyle {
            get { return lineCapStyle; }
            set { lineCapStyle = value; }
        }

        public int LineJoinStyle {
            get { return lineJoinStyle; }
            set { lineJoinStyle = value; }
        }

        public float MiterLimit {
            get { return miterLimit; }
            set { miterLimit = value; }
        }

        /**
         * @return {@link LineDashPattern} object, describing the dash pattern which should be applied.
         *         If no pattern should be applied (i.e. solid line), then returns <CODE>null</CODE>.
         */
        public virtual LineDashPattern GetLineDashPattern() {
            return lineDashPattern;
        }

        public virtual void SetLineDashPattern(LineDashPattern lineDashPattern) {
            this.lineDashPattern = lineDashPattern;
        }

        public class LineDashPattern {

            private PdfArray dashArray;
            private float dashPhase;

            public LineDashPattern(PdfArray dashArray, float dashPhase) {
                this.dashArray = new PdfArray(dashArray);
                this.dashPhase = dashPhase;
            }

            public PdfArray DashArray {
                get { return dashArray; }
                set { dashArray = value; }
            }

            public float DashPhase {
                get { return dashPhase; }
                set { dashPhase = value; }
            }
        }
    }
}
