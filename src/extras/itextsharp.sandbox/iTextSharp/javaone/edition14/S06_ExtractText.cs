using System;
using iTextSharp.javaone.edition14.part2;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14
{
    public class S06_ExtractText
    {
        public static void Main(String[] args)
        {
            ContentStreams._main(args);
            S06_ExtractText app = new S06_ExtractText();
            app.ExtractSnippets(ContentStreams.RESULT_HIGH);
            app.ExtractSnippets(ContentStreams.RESULT_CHUNKS);
            app.ExtractSnippets(ContentStreams.RESULT_ABSOLUTE);
        }

        public void ExtractSnippets(String src)
        {
            PdfReader reader = new PdfReader(src);
            Console.WriteLine(PdfTextExtractor.GetTextFromPage(reader, 1));
        }
    }
}
