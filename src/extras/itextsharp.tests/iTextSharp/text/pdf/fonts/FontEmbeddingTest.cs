using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts
{
    public class FontEmbeddingTest
    {
        private static string srcFolder = @"..\..\resources\text\pdf\fonts\NotoFont\";

        [Test]
        public void TestNotoFont()
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new MemoryStream());
            document.Open();
            String fontPath = srcFolder + "NotoSansCJKjp-Bold.otf";
            BaseFont bf = BaseFont.CreateFont(fontPath, "Identity-H", true);
            Font font = new Font(bf, 14);
            String[] lines = new String[] {"Noto test", "in japanese:", "\u713C"};

            foreach (String line in lines)
            {
                document.Add(new Paragraph(line, font));
            }
            document.Add(Chunk.NEWLINE);
            document.Close();
        }
    }
}