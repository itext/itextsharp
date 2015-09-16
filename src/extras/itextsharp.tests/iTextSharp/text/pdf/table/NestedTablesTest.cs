using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {
    class NestedTablesTest {

        private String cmpFolder = @"..\..\resources\text\pdf\table\nestedTablesTest\";
        private String outFolder = @"table\nestedTablesTest\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        [Timeout(30000)]
        public void NestedTablesTest01() {
            String output = "nestedTablesTest.pdf";
            String cmp = "cmp_nestedTablesTest.pdf";

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Document doc = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc, File.Create(outFolder + output));
            doc.Open();

            ColumnText column = new ColumnText(writer.DirectContent);
            column.SetSimpleColumn(72, 72, 523, 770);
            column.AddElement(CreateNestedTables(15));
            column.Go();

            doc.Close();

            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            CompareDocuments(output, cmp, false);
        }

        private static PdfPTable CreateNestedTables(int n) {
            PdfPCell cell = new PdfPCell();
            cell.AddElement(new Chunk("Hello"));

            if (n > 0)
                cell.AddElement(CreateNestedTables(n - 1));

            PdfPTable table = new PdfPTable(1);
            table.AddCell(cell);
            return table;
        }


        private void CompareDocuments(String @out, String cmp, bool visuallyOnly) {
            CompareTool compareTool = new CompareTool();
            String errorMessage;
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
