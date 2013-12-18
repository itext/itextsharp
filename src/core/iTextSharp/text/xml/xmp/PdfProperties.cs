using System;
using iTextSharp.xmp;

namespace iTextSharp.text.xml.xmp {

    public class PdfProperties {
        /** Keywords. */
        public static readonly String KEYWORDS = "Keywords";
        /** The PDF file version (for example: 1.0, 1.3, and so on). */
        public static readonly String VERSION = "PDFVersion";
        /** The Producer. */
        public static readonly String PRODUCER = "Producer";
        public static readonly String PART = "part";

        /**
         * Adds keywords.
         *
         * @param xmpMeta
         * @param keywords
         */

        public static void SetKeywords(IXmpMeta xmpMeta, String keywords) {
            xmpMeta.SetProperty(XmpConst.NS_PDF, KEYWORDS, keywords);
        }

        /**
         * Adds the producer.
         *
         * @param xmpMeta
         * @param producer
         */

        public static void SetProducer(IXmpMeta xmpMeta, String producer) {
            xmpMeta.SetProperty(XmpConst.NS_PDF, PRODUCER, producer);
        }

        /**
         * Adds the version.
         *
         * @param xmpMeta
         * @param version
         */

        public static void SetVersion(IXmpMeta xmpMeta, String version) {
            xmpMeta.SetProperty(XmpConst.NS_PDF, VERSION, version);
        }
    }
}
