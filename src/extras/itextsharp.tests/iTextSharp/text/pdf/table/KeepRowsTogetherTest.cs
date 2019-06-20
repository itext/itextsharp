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
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.table
{
    public class KeepRowsTogetherTest
    {
        private String cmpFolder = @"..\..\resources\text\pdf\table\keeprowstogether\";
        private String outFolder = @"table\keeprowstogether\";

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        /**
        * Creates two tables and both should be on their own page.
        *
        * @throws DocumentException
        * @throws IOException
        * @throws InterruptedException
        */
        [Test]
        public void TestKeepRowsTogetherInCombinationWithHeaders()
        {
            String file = "withheaders.pdf";
            CreateDocument(file, 0, 10, "Header for table 2", true, false);
            CompareDocuments(file);
        }

        /**
         * Creates two tables and the second table should have one row on pae 1 and every other row on page 2.
         *
         * @throws DocumentException
         * @throws IOException
         * @throws InterruptedException
         */
        [Test]
        public void TestKeepRowsTogetherWithoutHeader()
        {
            String file = "withoutheader.pdf";
            CreateDocument(file, 1, 10, "Header for table 2 (should be on page 1, not a header, just first row)", false, false);
            CompareDocuments(file);
        }

        /**
         * Creates two tables. The second table has 1 header row and it should skip the first header. 1 line of table 2 should be on page 1, the rest on page 2.
         *
         * @throws FileNotFoundException
         * @throws DocumentException
         */
        [Test]
        public void testKeepRowsTogetherInCombinationWithSkipFirstHeader()
        {
            String file = "withskipfirstheader.pdf";
            CreateDocument(file, 2, 10, "Header for Table 2", true, true);
        }

        /**
         * Utility method
         *
         * @param file fileName
         * @param start start index for the keepRowsTogether method
         * @param end end index for the keepRowsTogether method
         * @param header2Text text for the second table's header
         * @throws FileNotFoundException
         * @throws DocumentException
         */
        private void CreateDocument(String file, int start, int end, String header2Text, bool headerRows,
            bool skipFirstHeader)
        {
            Document document = new Document();

            PdfWriter.GetInstance(document, new FileStream(outFolder + file, FileMode.Create));

            document.Open();

            document.Add(CreateTable("Header for table 1", true, 1, skipFirstHeader));

            PdfPTable table = CreateTable(header2Text, headerRows, 2, skipFirstHeader);
            table.KeepRowsTogether(start, end);
            document.Add(table);

            document.Close();
        }

        /**
         * Utility ethod that creates a table with 41 rows. One of which may or may not be a header.
         *
         * @param headerText text for the first cell
         * @param headerRows is the first row a header
         * @param tableNumber number of the table
         * @return PdfPTable
         */
        private PdfPTable CreateTable(String headerText, bool headerRows, int tableNumber, bool skipFirstHeader)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell cell1 = new PdfPCell(new Paragraph(headerText));
            table.AddCell(cell1);

            if (headerRows)
            {
                table.HeaderRows = 1;

                if (skipFirstHeader)
                {
                    table.SkipFirstHeader = skipFirstHeader;
                }
            }

            for (int i = 0; i < 40; i++)
            {
                table.AddCell("Tab " + tableNumber + ", line " + i);
            }

            return table;
        }

        /**
         * Utility method that checks the created file against the cmp file
         * @param file name of the output document
         * @throws DocumentException
         * @throws InterruptedException
         * @throws IOException
         */
        private void CompareDocuments(String file)
        {
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFolder + file, cmpFolder + file, outFolder, "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }
    }
}
