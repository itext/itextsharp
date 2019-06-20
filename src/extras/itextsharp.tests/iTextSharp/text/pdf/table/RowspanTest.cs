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
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table {
    /**
     * @author Michael Demey
     */
    [TestFixture]
    public class RowspanTest {
        private static readonly String CMP_FOLDER = @"..\..\resources\text\pdf\table\RowspanTest\";
        private static readonly String OUTPUT_FOLDER = @"table\RowspanTest\";

        [TestFixtureSetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public virtual void Rowspan_Test() {
            String file = "rowspantest.pdf";

            string fileE = CMP_FOLDER + file;
            Console.Write(File.Exists(fileE));
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));
            document.Open();
            PdfContentByte contentByte = writer.DirectContent;

            Rectangle rect = document.PageSize;

            PdfPTable table = new PdfPTable(4);

            table.TotalWidth = rect.Right - rect.Left + 1;
            table.LockedWidth = true;

            float[] widths = new float[] {
                0.1f, 0.54f, 0.12f, 0.25f
            };

            table.SetWidths(widths);

            PdfPCell cell_1_1 = new PdfPCell(new Phrase("1-1"));
            cell_1_1.Colspan = 4;
            table.AddCell(cell_1_1);

            PdfPCell cell_2_1 = new PdfPCell(new Phrase("2-1"));
            cell_2_1.Rowspan = 2;
            table.AddCell(cell_2_1);

            PdfPCell cell_2_2 = new PdfPCell(new Phrase("2-2"));
            cell_2_2.Colspan = 2;
            table.AddCell(cell_2_2);

            PdfPCell cell_2_4 = new PdfPCell(new Phrase("2-4"));
            cell_2_4.Rowspan = 3;
            table.AddCell(cell_2_4);

            PdfPCell cell_3_2 = new PdfPCell(new Phrase("3-2"));
            table.AddCell(cell_3_2);

            PdfPCell cell_3_3 = new PdfPCell(new Phrase("3-3"));
            table.AddCell(cell_3_3);

            PdfPCell cell_4_1 = new PdfPCell(new Phrase("4-1"));
            cell_4_1.Colspan = 3;
            table.AddCell(cell_4_1);

            table.WriteSelectedRows(0, -1, rect.Left, rect.Top, contentByte);

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, CMP_FOLDER + file, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void NestedTableTest() {
            Document doc = new Document(PageSize.A4);
            String file = "nestedtabletest.pdf";
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));
            doc.Open();

            ColumnText col = new ColumnText(writer.DirectContent);
            col.SetSimpleColumn(
                Utilities.MillimetersToPoints(25),
                Utilities.MillimetersToPoints(25),
                PageSize.A4.Right - Utilities.MillimetersToPoints(25),
                PageSize.A4.Top - Utilities.MillimetersToPoints(25));

            PdfPTable table = new PdfPTable(3);
            table.HeaderRows = 1;
            table.AddCell("H1");
            table.AddCell("H2");
            table.AddCell("H3");

            for (int i = 0; i < 15; i++) {
                PdfPCell cell = new PdfPCell(createNestedTable());
                cell.Rowspan = 3;
                cell.Colspan = 3;
                table.AddCell(cell);
            }
            col.AddElement(table);

            while (ColumnText.HasMoreText(col.Go())) {
                doc.NewPage();
                col.YLine = PageSize.A4.Top - Utilities.MillimetersToPoints(25);
            }

            doc.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, CMP_FOLDER + file, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        private PdfPTable createNestedTable() {
            PdfPTable table = new PdfPTable(3);
            table.AddCell("S1");
            table.AddCell("S2");
            table.AddCell("S3");

            for (int i = 0; i < 2; i++) {
                table.AddCell("    " + (i + 1));
                table.AddCell("    " + (i + 1));
                table.AddCell("    " + (i + 1));
            }
            return table;
        }
    }
}
