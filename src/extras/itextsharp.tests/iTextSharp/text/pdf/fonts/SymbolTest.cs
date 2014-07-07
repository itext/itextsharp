using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts {
    public class SymbolTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\fonts\SymbolFontTest\";
        [SetUp]
        public virtual void SetUp() {
            Directory.CreateDirectory("fonts/SymbolFontTest");
        }

        [Test]
        public virtual void TextWithSymbolEncoding() {
            BaseFont f = BaseFont.CreateFont(BaseFont.SYMBOL, BaseFont.SYMBOL, false);
            FileStream fs = new FileStream("fonts/SymbolFontTest/textWithSymbolEncoding.pdf", FileMode.Create);
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            Paragraph p;
            writer.CompressionLevel = 0;
            doc.Open();

            String origText = "ΑΒΓΗ€\u2022\u2663\u22c5";
            p = new Paragraph(new Chunk(origText, new Font(f, 16)));
            doc.Add(p);
            doc.Close();
            CompareTool compareTool = new CompareTool("fonts/SymbolFontTest/textWithSymbolEncoding.pdf",
                TEST_RESOURCES_PATH + "cmp_textWithSymbolEncoding.pdf");
            String errorMessage = compareTool.CompareByContent("fonts/SymbolFontTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }

            PdfReader reader = new PdfReader("fonts/SymbolFontTest/textWithSymbolEncoding.pdf");
            String text = PdfTextExtractor.GetTextFromPage(reader, 1, new SimpleTextExtractionStrategy());
            reader.Close();
            Assert.AreEqual(origText, text);
        }
    }
}
