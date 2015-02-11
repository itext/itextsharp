using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpCoveredArea {

        private Rectangle rect;
        private bool matchesObjRect; // true if object's (which is under cleaning) rect is the same as filter rect

        public PdfCleanUpCoveredArea(Rectangle rect, bool matchesObjRect) {
            this.rect = rect;
            this.matchesObjRect = matchesObjRect;
        }

        public virtual Rectangle Rect {
            get { return rect; }
        }

        public virtual bool MatchesObjRect {
            get { return matchesObjRect; }
        }
    }
}
