using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14.part2
{
    public class ContentStreams
    {
        public static readonly string RESULT_HIGH = "results/javaone/edition2014/part2/hello-highlevel.pdf";
        public static readonly string RESULT_LOW = "results/javaone/edition2014/part2/hello-lowlevel.pdf";
        public static readonly string RESULT_UNCOMPRESSED = "results/javaone/edition2014/part2/hello-uncompressed.pdf";
        public static readonly string RESULT_CHUNKS = "results/javaone/edition2014/part2/hello-chunks.pdf";
        public static readonly string RESULT_ABSOLUTE = "results/javaone/edition2014/part2/hello-absolute.pdf";
        public static readonly string RESULT_REFLOW = "results/javaone/edition2014/part2/hello-reflow.pdf";
        public static readonly string RESULT_REFLOW_LOW = "results/javaone/edition2014/part2/hello-reflow-low.pdf";

        public static void _main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(RESULT_HIGH).Directory;
            if (dir != null)
                dir.Create();

            ContentStreams cs = new ContentStreams();
            cs.CreatePdfHigh();
            cs.CreatePdfLow();
            cs.CreatePdfUncompressed();
            cs.CreatePdfChunks();
            cs.CreatePdfAbsolute();
            cs.CreatePdfReflow();
            cs.CreatePdfReflowLow();
        }

        public void CreatePdfHigh()
        {
            // step 1
            Document document = new Document(PageSize.LETTER);
            // step 2
            PdfWriter.GetInstance(document, new FileStream(RESULT_HIGH, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World!"));
            // step 5
            document.Close();
        }

        public void CreatePdfLow()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_LOW, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText("Hello World!"); // (Hello World!)Tj
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
        }

        public void CreatePdfUncompressed()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_UNCOMPRESSED, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World!"));
            // step 5
            document.Close();
        }

        public void CreatePdfChunks()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_CHUNKS, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            Paragraph p = new Paragraph();
            p.Leading = 18;
            document.Add(p);
            document.Add(new Chunk("H"));
            document.Add(new Chunk("e"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("o"));
            document.Add(new Chunk(" "));
            document.Add(new Chunk("W"));
            document.Add(new Chunk("o"));
            document.Add(new Chunk("r"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("d"));
            document.Add(new Chunk("!"));
            // step 5
            document.Close();
        }

        public void CreatePdfAbsolute()
        {
            // step 1
            Document document = new Document();
            Document.Compress = false;
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_ABSOLUTE, FileMode.Create));
            //writer.setCompressionLevel(PdfStream.NO_COMPRESSION);
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            writer.CompressionLevel = 0;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText("Hel"); // (Hel)Tj
            canvas.MoveText(30.65f, 0);
            canvas.ShowText("World!"); // (World!)Tj
            canvas.MoveText(-12.7f, 0);
            canvas.ShowText("lo"); // (lo)Tj
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
            Document.Compress = true;
        }

        public void CreatePdfReflow()
        {
            // step 1
            Document document = new Document();
            Document.Compress = false;
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_REFLOW, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            document.Add(
                new Paragraph(
                    "0 Hello World 1 Hello World 2 Hello World 3 Hello World 4 Hello World 5 Hello World 6 Hello World 7 Hello World 8 Hello World 9 Hello World A Hello World B Hello World"));
            // step 5
            document.Close();
            Document.Compress = true;
        }

        public void CreatePdfReflowLow()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_REFLOW_LOW, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText(
                "0 Hello World 1 Hello World 2 Hello World 3 Hello World 4 Hello World 5 Hello World 6 Hello World 7 Hello World 8 Hello World 9 Hello World A Hello World B Hello World");
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
        }
    }
}
