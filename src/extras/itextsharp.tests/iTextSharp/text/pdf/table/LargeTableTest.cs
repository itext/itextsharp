/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
        public virtual void TestIncompleteTableAdd01() {
            const String file = "incomplete_add01.pdf";

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

            CompareTablePdf(file);
        }

        [Test]
        [Ignore("ignore")]
        public virtual void TestIncompleteTableAdd02()
        {
            const String file = "incomplete_add02.pdf";

            Document document = new Document(PageSize.LETTER);
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));

            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.HeaderRows = 2;
            table.SplitRows = false;
            table.Complete = false;

            for (int i = 0; i < 5; i++)
            {
                table.AddCell("Header " + i);
            }

            for (int i = 0; i < 500; i++)
            {
                if (i % 5 == 0)
                {
                    document.Add(table);
                }

                table.AddCell("Test " + i);
            }

            table.Complete = true;
            document.Add(table);
            document.Close();

            CompareTablePdf(file);
        }

        [Test]
        [Ignore("ignore")]
        public virtual void TestIncompleteTableAdd03()
        {
            const String file = "incomplete_add03.pdf";

            Document document = new Document(PageSize.LETTER);
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));

            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.HeaderRows = 2;
            table.FooterRows = 1;
            table.SplitRows = false;
            table.Complete = false;

            for (int i = 0; i < 5; i++)
            {
                table.AddCell("Header " + i);
            }

            for (int i = 0; i < 500; i++)
            {
                if (i % 5 == 0)
                {
                    document.Add(table);
                }

                table.AddCell("Test " + i);
            }

            table.Complete = true;
            document.Add(table);
            document.Close();

            CompareTablePdf(file);
        }

        [Test]
        public virtual void TestIncompleteTableAdd04()
        {
            const String file = "incomplete_add04.pdf";

            Document document = new Document(PageSize.LETTER);
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));

            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.HeaderRows = 1;
            table.FooterRows = 1;
            table.SplitRows = false;
            table.Complete = false;

            for (int i = 0; i < 5; i++)
            {
                table.AddCell("Header " + i);
            }

            for (int i = 0; i < 500; i++)
            {
                if (i % 5 == 0)
                {
                    document.Add(table);
                }

                table.AddCell("Test " + i);
            }

            table.Complete = true;
            document.Add(table);
            document.Close();

            CompareTablePdf(file);
        }

        [Test]
        public void testIncompleteTable2() {
            const String file = "incomplete_table_2.pdf";

            Document document = new Document(PageSize.A4.Rotate());
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));
            document.Open();
            Font font = new Font();
            float[] widths = new float[] {50f, 50f};
            PdfPTable table = new PdfPTable(widths.Length);
            table.Complete = false;
            table.SetWidths(widths);
            table.WidthPercentage = 100;
            PdfPCell cell = new PdfPCell(new Phrase("Column #1", font));
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Column #2", font));
            table.AddCell(cell);
            table.HeaderRows  = 1;

            for (int i = 0; i < 50; i++) {
                cell = new PdfPCell(new Phrase("Table cell #" + i, font));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Blah blah blah", font));
                table.AddCell(cell);
                if (i % 40 == 0) {
                    document.Add(table);
                }
            }
            table.Complete = true;
            document.Add(table);
            document.Close();

            CompareTablePdf(file);
        }

        [Test]
        public void RemoveRowFromIncompleteTable()
        {
            const string file = "incomplete_table_row_removed.pdf";

            Document document = new Document();

            PdfWriter pdfWriter = PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));

            document.Open();

            PdfPTable table = new PdfPTable(1);
            table.Complete = false;
            table.TotalWidth = 500;
            table.LockedWidth = true;

            for (int i = 0; i < 21; i++)
            {
                PdfPCell cell = new PdfPCell(new Phrase("Test" + i));
                table.AddCell(cell);
            }

            table.Rows.RemoveAt(20);
            document.Add(table);

            table.Complete = true;

            document.Add(table);

            document.Close();

            CompareTablePdf(file);
        }

        [Test]
        public void NestedHeaderFooter()  {
            const string file = "nested_header_footer.pdf";
            Document document = new Document(PageSize.A4.Rotate());
            PdfWriter.GetInstance(document, new FileStream(OUTPUT_FOLDER + file, FileMode.OpenOrCreate));
            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            PdfPCell cell = new PdfPCell(new Phrase("Table XYZ (Continued)"));
            cell.Colspan = 5;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Continue on next page"));
            cell.Colspan = 5 ;
            table.AddCell(cell);
            table.HeaderRows = 2;
            table.FooterRows = 1;
            table.SkipFirstHeader = true;
            table.SkipLastFooter = true;
            for (int i = 0; i < 350; i++) {
                table.AddCell(Convert.ToString(i + 1));
            }
            PdfPTable t = new PdfPTable(1);
            PdfPCell c = new PdfPCell(table);
            c.BorderColor = BaseColor.RED;
            c.Padding = 3;
            t.AddCell(c);
            document.Add(t);
            document.Close();

            CompareTablePdf(file);
        }

        
        private void CompareTablePdf(String file) {
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, CMP_FOLDER + file, OUTPUT_FOLDER, "diff");

            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }
    }
}
