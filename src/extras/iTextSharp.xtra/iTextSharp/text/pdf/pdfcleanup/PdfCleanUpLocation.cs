using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Defines the region in PDF document to be erased.
     */
    public class PdfCleanUpLocation {

        private int page;
        private Rectangle region;
        private BaseColor cleanUpColor;

        public PdfCleanUpLocation(int page, Rectangle region) {
            this.page = page;
            this.region = region;
        }

        /**
         *
         * @param page
         * @param region
         * @param cleanUpColor color used to fill cleaned up area.
         */
        public PdfCleanUpLocation(int page, Rectangle region, BaseColor cleanUpColor) : this(page, region) {
            this.cleanUpColor = cleanUpColor;
        }

        public virtual int Page {
            get { return page; }
        }

        public virtual Rectangle Region {
            get { return region; }
        }

        public virtual BaseColor CleanUpColor {
            get { return cleanUpColor; }
        }
    }
}
