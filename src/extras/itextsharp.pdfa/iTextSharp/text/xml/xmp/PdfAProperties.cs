using System;
using iTextSharp.xmp;

namespace iTextSharp.text.xml.xmp {

    public class PdfAProperties {
        /**
         * Part, always 1.
         */
        public static readonly String PART = "part";
        /**
         * Conformance, A, B, or U.
         */
        public static readonly String CONFORMANCE = "conformance";

        /**
         * Adds part.
         *
         * @param xmpMeta
         * @param part
         */

        public static void SetPart(IXmpMeta xmpMeta, String part) {
            xmpMeta.SetProperty(XmpConst.NS_PDFA_ID, PdfAProperties.PART, part);
        }

        /**
         * Adds the conformance.
         *
         * @param xmpMeta
         * @param conformance
         */

        public static void SetConformance(IXmpMeta xmpMeta, String conformance) {
            xmpMeta.SetProperty(XmpConst.NS_PDFA_ID, PdfAProperties.CONFORMANCE, conformance);
        }
    }
}
