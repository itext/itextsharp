using System;
using System.Collections.Generic;
using iTextSharp.text.xml.simpleparser;
/**
 *
 */
namespace iTextSharp.text.xml.simpleparser.handler {

    /**
     * This {@link NewLineHandler} returns true on the tags <code>p</code>,
     * <code>blockqoute</code>and <code>br</code>
     *
     * @author Balder
     *
     */
    public class HTMLNewLineHandler : INewLineHandler {

        private readonly Dictionary<String,object> newLineTags = new Dictionary<string,object>();

        /**
         * Default constructor
         *
         * @since 5.0.6
         */
        public HTMLNewLineHandler() {
            newLineTags["p"] = null;
            newLineTags["blockquote"] = null;
            newLineTags["br"] = null;
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.text.xml.simpleparser.NewLineHandler#isNewLineTag(java.lang
         * .String)
         */
        public bool IsNewLineTag(String tag) {
            return newLineTags.ContainsKey(tag);
        }

    }
}