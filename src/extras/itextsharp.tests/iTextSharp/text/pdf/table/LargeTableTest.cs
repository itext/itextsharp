using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {

    [TestFixture]
    public class LargeTableTest {
        private static readonly String CMP_FOLDER = @"..\..\resources\text\pdf\table\LargeTableTest\";
        private static readonly String OUTPUT_FOLDER = @"table\LargeTableTest\";

        [TestFixtureSetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public virtual void TestNoChangeInSetSkipFirstHeader() {
            Document document = new Document();
            Stream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            PdfPTable table = new PdfPTable(5);
            table.HeaderRows = 1;
            table.SplitRows = false;
            table.Complete = false;

            for (int i = 0; i < 5; ++i) {
                table.AddCell("Header " + i);
            }

            table.AddCell("Cell 1");

            document.Add(table);

            Assert.False(table.SkipFirstHeader);

            table.Complete = true;

            for (int i = 1; i < 5; i++) {
                table.AddCell("Cell " + i);
            }

            document.Add(table);

            document.Close();
            stream.Close();
        }

        [Test]
        public virtual void TestIncompleteTableAdd() {
            const String file = "incomplete_add.pdf";

            Document document = new Document(PageSize.LETTER);
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));

            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.HeaderRows = 1;
            table.SplitRows = false;
            table.Complete = false;

            for (int i = 0; i < 5; i++) {
                table.AddCell("Header " + i);
            }

            for (int i = 0; i < 500; i++) {
                if (i % 5 == 0) {
                    document.Add(table);
                }

                table.AddCell("Test " + i);
            }

            table.Complete = true;
            document.Add(table);
            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, CMP_FOLDER + file, OUTPUT_FOLDER, "diff");

            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }
}
