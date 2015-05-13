using System;
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {

    public class ColorsInTaggedDocumentsTest {
        private String cmpFolder = @"..\..\resources\text\pdf\table\tableColorsTest\";
        private String outFolder = @"table\tableColorsTest\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void ColorsInTaggedDocumentsTest1() {
            String output = "coloredTables.pdf";
            String cmp = "cmp_coloredTables.pdf";

            CreateColoredTablesFile(outFolder + output, true);
            CompareDocuments(output, cmp, false);
        }

        [Test]
        public void ColorsInTaggedDocumentsTest2() {
            String output = "coloredTables.pdf";
            String cmp = "cmp_coloredTables.pdf";

            //visually comparing results of tagged and non-tagged colored tables
            CreateColoredTablesFile(outFolder + output, false);
            CompareDocuments(output, cmp, true);
        }

        private void CreateColoredTablesFile(string outPath, bool tagged) {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, File.Create(outPath));
            if (tagged)
                writer.SetTagged();
            document.Open();

            BaseColor color = new BaseColor(255, 255, 240);
            Font coloredFont = new Font(Font.FontFamily.HELVETICA, 12f, Font.NORMAL, color);

            //First table
            PdfPTable table = new PdfPTable(4);
            int rowsNum = 10;
            int columnsNum = 4;
            for (int i = 0; i < rowsNum; ++i) {
                for (int j = 0; j < columnsNum; ++j) {
                    PdfPCell cell = new PdfPCell(new Paragraph("text", coloredFont));
                    cell.BorderWidth = 2;
                    cell.BorderColor = BaseColor.DARK_GRAY;
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    table.AddCell(cell);
                }
            }
            document.Add(table);
            document.NewPage();


            Font fontRed = new Font(Font.FontFamily.HELVETICA, 12f, Font.NORMAL, new BaseColor(255, 0, 0));
            Font fontGreen = new Font(Font.FontFamily.HELVETICA, 12f, Font.NORMAL, new BaseColor(0, 255, 0));
            Font fontBlue = new Font(Font.FontFamily.HELVETICA, 12f, Font.NORMAL, new BaseColor(0, 0, 255));

            //Second table
            table = new PdfPTable(4);

            PdfPCell cell11 = new PdfPCell(new Paragraph("text", fontRed));
            PdfPCell cell12 = new PdfPCell(new Paragraph("text", fontBlue));
            PdfPCell cell13 = new PdfPCell(new Paragraph("text", fontGreen));


            PdfPCell cell21 = new PdfPCell(new Paragraph("text", fontRed));
            PdfPCell cell22 = new PdfPCell(new Paragraph("text", fontGreen));
            PdfPCell cell23 = new PdfPCell(new Paragraph("text", fontBlue));

            PdfPCell cell32 = new PdfPCell(new Paragraph("text", fontBlue));
            PdfPCell cell33 = new PdfPCell(new Paragraph("text", fontRed));
            PdfPCell cell34 = new PdfPCell(new Paragraph("text", fontGreen));

            table.AddCell(cell11);
            table.AddCell(cell12);
            table.AddCell(cell13);

            table.AddCell(cell21);
            table.AddCell(cell22);
            table.AddCell(cell23);

            table.AddCell(cell32);
            table.AddCell(cell33);
            table.AddCell(cell34);

            document.Add(table);

            document.Add(new Phrase("  "));

            //Third table
            table = new PdfPTable(4);

            cell11 = new PdfPCell(new Paragraph("text", fontRed));
            cell11.BackgroundColor = BaseColor.YELLOW;
            cell11.BorderWidth = 3;
            cell11.BorderColor = new BaseColor(0, 0, 255);
            cell12 = new PdfPCell(new Paragraph("text", fontBlue));
            cell13 = new PdfPCell(new Paragraph("text", fontGreen));


            cell21 = new PdfPCell(new Paragraph("text", fontRed));
            cell21.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell21.BorderColor = BaseColor.PINK;
            cell21.BorderWidth = 3;
            cell22 = new PdfPCell(new Paragraph("text", fontGreen));
            cell22.BackgroundColor = BaseColor.YELLOW;
            cell22.BorderColor = BaseColor.BLUE;
            cell22.BorderWidth = 3;
            cell23 = new PdfPCell(new Paragraph("text", fontBlue));
            cell23.BackgroundColor = BaseColor.GREEN;
            cell23.BorderWidth = 3;
            cell23.BorderColor = BaseColor.WHITE;

            cell32 = new PdfPCell(new Paragraph("text", fontBlue));
            cell32.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell32.BorderColor = BaseColor.MAGENTA;
            cell32.BorderWidth = 3;
            cell33 = new PdfPCell(new Paragraph("text", fontRed));
            cell33.BackgroundColor = BaseColor.PINK;
            cell33.BorderColor = BaseColor.CYAN;
            cell33.BorderWidth = 3;
            cell34 = new PdfPCell(new Paragraph("text", fontGreen));
            cell34.BackgroundColor = BaseColor.ORANGE;
            cell34.BorderColor = BaseColor.WHITE;
            cell34.BorderWidth = 3;

            table.AddCell(cell11);
            table.AddCell(cell12);
            table.AddCell(cell13);

            table.AddCell(cell21);
            table.AddCell(cell22);
            table.AddCell(cell23);

            table.AddCell(cell32);
            table.AddCell(cell33);
            table.AddCell(cell34);

            document.Add(table);

            document.Close();
        }

        private void CompareDocuments(String @out, String cmp, bool visuallyOnly) {
            CompareTool compareTool = new CompareTool();
            string errorMessage;
            if (visuallyOnly) {
                errorMessage = compareTool.Compare(outFolder + @out, cmpFolder + cmp, outFolder, "diff");
            } else {
                errorMessage = compareTool.CompareByContent(outFolder + @out, cmpFolder + cmp, outFolder, "diff");
            }
            if (errorMessage != null)
                Assert.Fail(errorMessage);
        }
    }
}