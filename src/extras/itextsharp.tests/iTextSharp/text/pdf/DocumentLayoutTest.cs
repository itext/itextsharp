using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class DocumentLayoutTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\DocumentLayoutTest\";
        private const string OUTPUT_FOLDER = @"AcroFieldsTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void TextLeadingTest() {
            String file = "phrases.pdf";

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, File.Create(OUTPUT_FOLDER + file));
            document.Open();

            Phrase p1 = new Phrase("first, leading of 150");
            p1.Leading = 150;
            document.Add(p1);
            document.Add(Chunk.NEWLINE);

            Phrase p2 = new Phrase("second, leading of 500");
            p2.Leading = 500;
            document.Add(p2);
            document.Add(Chunk.NEWLINE);

            Phrase p3 = new Phrase();
            p3.Add(new Chunk("third, leading of 20"));
            p3.Leading = 20;
            document.Add(p3);

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, TEST_RESOURCES_PATH + file, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }

}
