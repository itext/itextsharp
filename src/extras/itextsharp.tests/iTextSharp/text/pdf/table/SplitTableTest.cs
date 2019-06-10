/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
