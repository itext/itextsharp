using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Represents a chunk of a pdf content stream which is under cleanup processing. E.g. image, text.
     */
    abstract class PdfCleanUpContentChunk {

        private bool visible;

        public PdfCleanUpContentChunk(bool visible) {
            this.visible = visible;
        }

        public virtual bool Visible {
            get { return visible; }
        }

        /**
         * Represents a text fragment from a pdf content stream.
         */
        public class Text : PdfCleanUpContentChunk {

            private PdfString text;
            private float startX;
            private float endX;
            private int numOfStrTextBelongsTo;

            public Text(PdfString text, Vector startLocation, Vector endLocation, bool visible, int numOfStrTextBelongsTo) : base(visible) {
                this.text = text;
                this.startX = startLocation[0];
                this.endX = endLocation[0];
                this.numOfStrTextBelongsTo = numOfStrTextBelongsTo;
            }

            public virtual PdfString GetText() {
                return text;
            }

            public virtual float StartX {
                get { return startX; }
            }

            public virtual float EndX {
                get { return endX; }
            }

            public virtual int NumOfStrTextBelongsTo {
                get { return numOfStrTextBelongsTo; }
            }
        }

        /**
         * Represents an image used in a pdf content stream.
         */
        public class Image : PdfCleanUpContentChunk {

            private byte[] newImageData;

            public Image(bool visible, byte[] newImageData) : base(visible) {
                this.newImageData = newImageData;
            }

            public virtual byte[] NewImageData {
                get { return newImageData; }
            }
        }
    }
}
