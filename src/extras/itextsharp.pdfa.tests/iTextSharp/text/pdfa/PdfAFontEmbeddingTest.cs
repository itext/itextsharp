using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts
{
    public class FontEmbeddingTest
    {
        public const String RESOURCES = @"..\..\resources\text\pdfa\";

        [Test]
        public void TestNotoFont()
        {
            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, new MemoryStream(), PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();
            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            String fontPath = RESOURCES + "NotoSansCJKjp-Bold.otf";
            BaseFont bf = BaseFont.CreateFont(fontPath, "Identity-H", true);
            Font font = new Font(bf, 14);
            String[] lines = new String[] {"Noto test", "in japanese:", "\u713C"};

            foreach (String line in lines)
            {
                document.Add(new Paragraph(line, font));
            }

            document.Close();
        }
    }
}