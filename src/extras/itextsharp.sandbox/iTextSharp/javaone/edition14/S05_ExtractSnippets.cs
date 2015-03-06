using System;
using System.IO;
using iTextSharp.javaone.edition14.part2;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// Extracts snippets of text from different Hello World examples
    /// </summary>
    public class S05_ExtractSnippets
    {
        public static readonly string RESULT_HIGH = "results/javaone/edition2014/05_hello-highlevel.txt";
        public static readonly string RESULT_LOW = "results/javaone/edition2014/05_hello-lowlevel.txt";
        public static readonly string RESULT_CHUNKS = "results/javaone/edition2014/05_hello-chunks.txt";
        public static readonly string RESULT_ABSOLUTE = "results/javaone/edition2014/05_hello-absolute.txt";

        public static void Main(String[] args)
        {
            ContentStreams._main(args);
            S05_ExtractSnippets app = new S05_ExtractSnippets();
            app.extractSnippets(ContentStreams.RESULT_HIGH, RESULT_HIGH);
            app.extractSnippets(ContentStreams.RESULT_CHUNKS, RESULT_CHUNKS);
            app.extractSnippets(ContentStreams.RESULT_ABSOLUTE, RESULT_ABSOLUTE);
        }

        public void extractSnippets(String src, String dest)
        {
            TextWriter output = new StreamWriter(new FileStream(dest, FileMode.Create));
            PdfReader reader = new PdfReader(src);
            IRenderListener listener = new MyTextRenderListener(output);
            PdfContentStreamProcessor processor =
                new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            output.Flush();
            output.Close();
            reader.Close();
        }
    }
}
