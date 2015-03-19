using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpContentChunk {

        private bool visible;

        private PdfString text;
        private float startX;
        private float endX;
        private int numOfStrChunkBelongsTo;

        private bool image;
        private byte[] newImageData;

        public PdfCleanUpContentChunk(PdfString text, Vector startLocation, Vector endLocation, bool visible, int numOfStrChunkBelongsTo) {
            this.text = text;
            this.startX = startLocation[0];
            this.endX = endLocation[0];
            this.visible = visible;
            this.numOfStrChunkBelongsTo = numOfStrChunkBelongsTo;
        }

        public PdfCleanUpContentChunk(bool visible, byte[] newImageData) {
            this.image = true;
            this.visible = visible;
            this.newImageData = newImageData;
        }

        public virtual PdfString Text {
            get { return text; }
        }

        public virtual bool IsVisible() {
            return visible;
        }

        public virtual bool IsImage() {
            return image;
        }

        public virtual float StartX {
            get { return startX; }
        }

        public virtual float EndX {
            get { return endX; }
        }

        public virtual byte[] NewImageData {
            get { return newImageData;  }
        }

        public virtual int NumOfStrChunkBelongsTo {
            get { return numOfStrChunkBelongsTo; }
        }
    }
}
