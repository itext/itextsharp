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