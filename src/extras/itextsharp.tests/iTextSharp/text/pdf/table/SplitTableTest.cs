using System;
using System.Globalization;
using System.IO;
using System.util.collections;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {
    [TestFixture]
    class SplitTableTest {
        private static readonly String cmpFolder = @"..\..\resources\text\pdf\table\SplitTableTest\";
        private static readonly String outFolder = @"table\SplitTableTest\";

        [TestFixtureSetUp]
        public static void Init() {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void AddOnPageBreakSimpleTest() {
            RunLargeTableTest("addOnPageBreakSimple", 0, 0, 40, 34);
        }

        [Test]
        public void AddOnPageBreakHeaderTest() {
            RunLargeTableTest("addOnPageBreakHeader", 2, 0, 40, 32);
        }

        [Test]
        public void BigCellSplitDefaultTest() {
            RunBigRowTest("bigCellSplitDefault", true, false, 700, false);
        }

        [Test]
        public void BigCellSplitLateTest() {
            RunBigRowTest("bigCellSplitLate", true, true, 700, false);
        }

        [Test]
        public void BigCellNoSplitTest() {
            RunBigRowTest("bigCellNoSplit", false, false, 700, false);
        }

        [Test]
        public void VeryBigCellSplitDefaultTest() {
            RunBigRowTest("veryBigCellSplitDefault", true, false, 800, true);
        }

        [Test]
        public void VeryBigCellSplitLateTest() {
            RunBigRowTest("veryBigCellSplitLate", true, true, 800, true);
        }

        [Test]
        public void VeryBigCellNoSplitTest() {
            RunBigRowTest("veryBigCellNoSplit", false, false, 800, true);
        }

        private void RunBigRowTest(String name, bool splitRows, bool splitLate, float bigRowHeight, bool expectException) {
            String outPdf = outFolder + name + ".pdf";
            String cmpPdf = cmpFolder + "cmp_" + name + ".pdf";
            String diff = "diff_" + name + "_";

            Document document = new Document();
            Stream outStream = File.Create(outPdf);
            PdfWriter writer = PdfWriter.GetInstance(document, outStream);

            try {
                document.SetPageSize(PageSize.A4);
                document.Open();

                PdfPTable table = new PdfPTable(1);
                for (int i = 0; i < 10; ++i) {
                    PdfPCell cell = new PdfPCell(new Phrase("cell before big one #" + i));
                    table.AddCell(cell);
                }
                PdfPCell bigCell = new PdfPCell(new Phrase("Big cell"));
                bigCell.FixedHeight = bigRowHeight;
                table.AddCell(bigCell);
                for (int i = 0; i < 10; ++i) {
                    PdfPCell cell = new PdfPCell(new Phrase("cell after big one #" + i));
                    table.AddCell(cell);
                }

                table.SplitRows = splitRows;
                table.SplitLate = splitLate;
                table.Complete = true;
                document.Add(table);
            }
            catch (DocumentException e) {
                Assert.True(expectException);
            }
            finally {
                document.Close();
                writer.Close();
                outStream.Close();
            }

            if (!expectException) {
                Assert.Null(new CompareTool().CompareByContent(outPdf, cmpPdf, outFolder, diff));
            }
        }

        private void RunLargeTableTest(String name, int headerRows, int footerRows, int rows, params int[] flushIndexes) {
            String outPdf = outFolder + name + ".pdf";
            String cmpPdf = cmpFolder + "cmp_" + name + ".pdf";
            String diff = "diff_" + name + "_";

            Document document = new Document();
            Stream outStream = File.Create(outPdf);
            PdfWriter writer = PdfWriter.GetInstance(document, outStream);

            document.SetPageSize(PageSize.A4);
            document.Open();

            PdfPTable table = new PdfPTable(1);
            table.Complete = false;
            table.SplitRows = false;
            table.SplitLate = false;

            int fullHeader = 0;
            if (headerRows > 0) {
                for (int i = 0; i < headerRows; ++i) {
                    PdfPCell header = new PdfPCell();
                    header.AddElement(new Phrase("Header " + i));
                    table.AddCell(header);
                }
                fullHeader += headerRows;
            }
            if (footerRows > 0) {
                for (int i = 0; i < footerRows; ++i) {
                    PdfPCell footer = new PdfPCell();
                    footer.AddElement(new Phrase("Footer " + i));
                    table.AddCell(footer);
                }
                fullHeader += footerRows;
                table.FooterRows = footerRows;
            }
            table.HeaderRows = fullHeader;

            HashSet2<int> indexes = new HashSet2<int>(flushIndexes);
            for (int i = 0; i < rows; ++i) {
                PdfPCell cell = new PdfPCell();
                cell.AddElement(new Phrase(i.ToString(CultureInfo.InvariantCulture)));
                table.AddCell(cell);

                if (indexes.Contains(i)) {
                    document.Add(table);
                }
            }

            table.Complete = true;
            document.Add(table);

            document.Close();
            writer.Close();
            outStream.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, cmpPdf, outFolder, diff));
        }
    }
}
