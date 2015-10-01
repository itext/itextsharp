using System;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class VerticalPositionTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\VerticalPositionTest\";
        private const string OUTPUT_FOLDER = @"VerticalPositionTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void VerticalPositionTest0() {
            String file = "vertical_position.pdf";

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, File.Create(OUTPUT_FOLDER + file));
            document.Open();

            writer.PageEvent = new CustomPageEvent();

            PdfPTable table = new PdfPTable(2);
            for (int i = 0; i < 100; i++) {
                table.AddCell("Hello " + i);
                table.AddCell("World " + i);
            }

            document.Add(table);

            document.NewPage();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 1000; i++) {
                sb.Append("some more text ");
            }
            document.Add(new Paragraph(sb.ToString()));

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, TEST_RESOURCES_PATH + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }

    internal class CustomPageEvent : PdfPageEventHelper {
        public override void OnEndPage(PdfWriter writer, Document document) {
            Rectangle pageSize = writer.PageSize;
            float verticalPosition = writer.GetVerticalPosition(false);
            PdfContentByte canvas = writer.DirectContent;
            Rectangle rect = new Rectangle(0, verticalPosition, pageSize.Right, pageSize.Top);
            rect.Border = Rectangle.BOX;
            rect.BorderWidth = 1;
            rect.BorderColor = BaseColor.BLUE;
            canvas.Rectangle(rect);
        }
    }
}
