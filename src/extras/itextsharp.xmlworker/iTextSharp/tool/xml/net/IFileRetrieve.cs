using System;
using System.IO;
/**
 *
 */
namespace iTextSharp.tool.xml.net {

    /**
     * @author itextpdf.com
     *
     */
    public interface IFileRetrieve {

        /**
         * Process content from a given URL. using {@link URL#openStream()}
         * @param href the URL to process
         * @param processor the ReadingProcessor
         * @throws IOException if something went wrong.
         */
        void ProcessFromHref(String href, IReadingProcessor processor);

        /**
         * Process content from a given stream.
         * @param in the stream to process
         * @param processor the ReadingProcessor
         * @throws IOException if something went wrong.
         */
        void ProcessFromStream(Stream inp, IReadingProcessor processor);



    }
}