using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class PageEventTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PageEventTest\";
        private const string OUTPUT_FOLDER = @"PageEventTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void PageEventTest01() {
            String fileName = "pageEventTest01.pdf";

            MemoryStream baos = new MemoryStream();
            Document doc = new Document(PageSize.LETTER, 144, 144, 144, 144);
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.PageEvent = new MyEventHandler();

            writer.SetTagged();
            doc.Open();

            Chunk c = new Chunk("This is page 1");
            doc.Add(c);

            doc.Close();

            File.WriteAllBytes(OUTPUT_FOLDER + fileName, baos.ToArray());
            baos.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + fileName, TEST_RESOURCES_PATH + fileName, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        private class MyEventHandler : PdfPageEventHelper {
            private PdfPTable _header;
            private PdfPTable _footer;

            public MyEventHandler() {
                _header = new PdfPTable(1);
                PdfPCell hCell = new PdfPCell(new Phrase("HEADER"));
                hCell.Border = Rectangle.NO_BORDER;
                _header.AddCell(hCell);
                _header.SetTotalWidth(new float[]{300f});

                _footer = new PdfPTable(1);
                PdfPCell fCell = new PdfPCell(new Phrase("FOOTER"));
                fCell.Border = Rectangle.NO_BORDER;
                _footer.AddCell(fCell);
                _footer.SetTotalWidth(new float[]{300f});
            }

            public override void OnStartPage(PdfWriter writer, Document document) {
                base.OnStartPage(writer, document);
                WriteHeader(writer);
            }

            public override void OnEndPage(PdfWriter writer, Document document) {
                base.OnEndPage(writer, document);
                WriteFooter(writer);
            }

            private void WriteHeader(PdfWriter writer) {
                writer.DirectContent.SaveState();
                _header.WriteSelectedRows(0, _header.Rows.Count, 72, writer.PageSize.Height - 72, writer.DirectContent);
                writer.DirectContent.RestoreState();
            }

            private void WriteFooter(PdfWriter writer) {
                writer.DirectContent.SaveState();
                _footer.WriteSelectedRows(0, _footer.Rows.Count, 72, 72, writer.DirectContent);
                writer.DirectContent.RestoreState();
            }
        }
    }
}
