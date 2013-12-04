using System;
using iTextSharp.text.xml.simpleparser;
/**
 *
 */

namespace iTextSharp.text.xml.simpleparser.handler {

    /**
     * Always returns false.
     * @author Balder
     * @since 5.0.6
     *
     */
    public class NeverNewLineHandler : INewLineHandler {

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.text.xml.simpleparser.NewLineHandler#isNewLineTag(java.lang
         * .String)
         */
        public bool IsNewLineTag(String tag) {
            return false;
        }
    }
}