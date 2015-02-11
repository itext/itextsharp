using System;
using System.Collections.Generic;
using System.Text;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Represents subset of graphics state parameters
     */
    class PdfCleanUpGraphicsState {

        private float fontSize = 1;
        private float horizontalScaling = 100; // in percents
        private float characterSpacing;
        private float wordSpacing;

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
    }
}
