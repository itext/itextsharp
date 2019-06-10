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

        [Test]
        public void TestSplitLateAndSplitRow1()
        {
            String filename = "testSplitLateAndSplitRow1.pdf";
            Document doc = new Document(PageSize.LETTER, 72f, 72f, 72f, 72f);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outFolder + filename, FileMode.Create));
            doc.Open();
            PdfContentByte canvas = writer.DirectContent;

            ColumnText ct = new ColumnText(canvas);

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < 21; ++i)
            {
                text.Append(i).Append("\n");
            }

            // Add a table with a single row and column that doesn't fit on one page
            PdfPTable t = new PdfPTable(1);
            t.SplitLate = true;
            t.SplitRows = true;
            t.WidthPercentage = 100f;

            ct.AddElement(
                new Paragraph(
                    "Pushing table down\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\n"));

            PdfPCell c = new PdfPCell();
            c.HorizontalAlignment = Element.ALIGN_LEFT;
            c.VerticalAlignment = Element.ALIGN_TOP;
            c.Border = Rectangle.NO_BORDER;
            c.BorderWidth = 0;
            c.Padding = 0;
            c.AddElement(new Paragraph(text.ToString()));
            t.AddCell(c);

            ct.AddElement(t);

            int status = 0;
            while (ColumnText.HasMoreText(status))
            {
                ct.SetSimpleColumn(doc.Left, doc.Bottom, doc.Right, doc.Top);
                status = ct.Go();

                if (ColumnText.HasMoreText(status))
                    doc.NewPage();
            }

            doc.Close();

            String errorMessage = new CompareTool().CompareByContent(outFolder + filename, cmpFolder + filename,
                outFolder, "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestSplitLateAndSplitRow2()
        {
            String filename = "testSplitLateAndSplitRow2.pdf";
            Document doc = new Document(PageSize.LETTER, 72f, 72f, 72f, 72f);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outFolder + filename, FileMode.Create));
            doc.Open();
            PdfContentByte canvas = writer.DirectContent;

            ColumnText ct = new ColumnText(canvas);

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < 42; ++i)
            {
                text.Append(i).Append("\n");
            }

            // Add a table with a single row and column that doesn't fit on one page
            PdfPTable t = new PdfPTable(1);
            t.SplitLate = true;
            t.SplitRows = true;
            t.WidthPercentage = 100f;

            ct.AddElement(
                new Paragraph(
                    "Pushing table down\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\ndown\n"));

            PdfPCell c = new PdfPCell();
            c.HorizontalAlignment = Element.ALIGN_LEFT;
            c.VerticalAlignment = Element.ALIGN_TOP;
            c.Border = Rectangle.NO_BORDER;
            c.BorderWidth = 0;
            c.Padding = 0;
            c.AddElement(new Paragraph(text.ToString()));
            t.AddCell(c);

            ct.AddElement(t);

            int status = 0;
            while (ColumnText.HasMoreText(status))
            {
                ct.SetSimpleColumn(doc.Left, doc.Bottom, doc.Right, doc.Top);
                status = ct.Go();

                if (ColumnText.HasMoreText(status))
                    doc.NewPage();
            }

            doc.Close();

            String errorMessage = new CompareTool().CompareByContent(outFolder + filename, cmpFolder + filename,
                outFolder, "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
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
