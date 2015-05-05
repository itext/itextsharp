using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {

    /**
     * @author Raf Hens
     */
    public class KeepTogetherTest {

        private String cmpFolder = @"..\..\resources\text\pdf\table\keeptogether\";
        private String outFolder = @"table\keeptogether\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void TestKeepTogether1() {
            TestKeepTogether(true, true);
            CompareDocuments(true, true);
        }

        [Test]
        public void TestKeepTogether2() {
            TestKeepTogether(true, false);
            CompareDocuments(true, false);
        }

        [Test]
        public void TestKeepTogether3() {
            TestKeepTogether(false, true);
            CompareDocuments(false, true);
        }

        [Test]
        public void TestKeepTogether4() {
            TestKeepTogether(false, false);
            CompareDocuments(false, false);
        }

        public void TestKeepTogether(bool tagged, bool keepTogether) {
            Document document = new Document();
            String file = "tagged_" + tagged + "-keeptogether_" + keepTogether + ".pdf";
            PdfWriter writer = PdfWriter.GetInstance(document, File.Create(outFolder + file));
            if (tagged)
                writer.SetTagged();
            document.Open();
            int columns = 3;
            int tables = 3;
            for (int tableCount = 0; tableCount < tables; tableCount++) {
                PdfPTable table = new PdfPTable(columns);
                for (int rowCount = 0; rowCount < 50; rowCount++) {
                    PdfPCell cell1 = new PdfPCell(new Paragraph("t" + tableCount + " r:" + rowCount));
                    PdfPCell cell2 = new PdfPCell(new Paragraph("t" + tableCount + " r:" + rowCount));
                    PdfPCell cell3 = new PdfPCell(new Paragraph("t" + tableCount + " r:" + rowCount));
                    table.AddCell(cell1);
                    table.AddCell(cell2);
                    table.AddCell(cell3);
                }
                table.SpacingAfter = 10f;
                table.KeepTogether = keepTogether;
                document.Add(table);
            }
            document.Close();
        }

        /**
         * Utility method that checks the created file against the cmp file
         * @param tagged Tagged document
         * @param keepTogether PdfPTable.setKeepTogether(true/false)
         * @throws DocumentException
         * @throws InterruptedException
         * @throws IOException
         */
        private void CompareDocuments(bool tagged, bool keepTogether) {
            String file = "tagged_" + tagged + "-keeptogether_" + keepTogether + ".pdf";
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFolder + file, cmpFolder + file, outFolder, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }
}
