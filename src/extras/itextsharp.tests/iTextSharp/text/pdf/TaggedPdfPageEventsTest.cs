using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class TaggedPdfPageEventsTest : PdfPageEventHelper {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\TaggedPdfPageEventsTest\";
        private const string OUTPUT_FOLDER = @"TaggedPdfPageEventsTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void Test() {
            String file = "tagged_pdf_page_events.pdf";

            Document document = new Document();

            PdfWriter pdfWriter = PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));

            pdfWriter.SetTagged();
            pdfWriter.PageEvent = new TaggedPdfPageEventsTest();

            document.Open();

            document.Add(new Paragraph("Hello"));
            document.NewPage();
            document.Add(new Paragraph("World"));

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, TEST_RESOURCES_PATH + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document) {
            PdfPTable headerTable = new PdfPTable(1);
            headerTable.AddCell(new Phrase("Header"));

            WriteTable(writer.DirectContent, headerTable,
                new Rectangle(0, document.PageSize.Height - 50f, document.PageSize.Width, document.PageSize.Height));
        }

        public override void OnEndPage(PdfWriter writer, Document document) {
            PdfPTable footerTable = new PdfPTable(1);
            footerTable.AddCell(new Phrase("Footer"));
            WriteTable(writer.DirectContent, footerTable, new Rectangle(0, 0, document.PageSize.Width, 50f));
        }


        private void WriteTable(PdfContentByte directContent, PdfPTable table, Rectangle coordinates) {
            ColumnText columnText = new ColumnText(directContent);
            columnText.SetSimpleColumn(coordinates);
            columnText.AddElement(table);
            columnText.Go();
        }
    }
}
