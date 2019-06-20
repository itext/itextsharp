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
