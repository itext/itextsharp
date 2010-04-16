using System;
using iTextSharp.text.pdf;
/*
 * Created on Mar 29, 2010
 * (c) 2010 Trumpet, Inc.
 *
 */
namespace iTextSharp.text.pdf.parser {

    /**
     * A utility class that makes it cleaner to process content from pages of a PdfReader
     * through a specified RenderListener.
     * @since 5.0.2
     */
    public class PdfReaderContentParser {
        /** the reader this parser will process */
        private PdfReader reader;
        
        public PdfReaderContentParser(PdfReader reader) {
            this.reader = reader;
        }

        /**
         * Processes content from the specified page number using the specified listener
         * @param <E> the type of the renderListener - this makes it easy to chain calls
         * @param pageNumber the page number to process
         * @param renderListener the listener that will receive render callbacks
         * @return the provided renderListener
         * @throws IOException if operations on the reader fail
         */
        
        public E ProcessContent<E>(int pageNumber, E renderListener) where E : IRenderListener {
            PdfDictionary pageDic = reader.GetPageN(pageNumber);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(renderListener);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, pageNumber), resourcesDic);        
            return renderListener;
        }
    }
}