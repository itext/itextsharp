using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfReaderSelectPagesTest
    {
        byte[] data;
        string dataFile;
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PdfReaderSelectPagesTest\";



        [SetUp]
        public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
            dataFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "RomeoJuliet.pdf");
            data = TestResourceUtils.GetResourceAsByteArray(TEST_RESOURCES_PATH, "RomeoJuliet.pdf");
        }

        [TearDown]
        public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void Test()
        {
            PdfReader reader = new PdfReader(dataFile);
            try
            {
                reader.SelectPages("4-8");
                ManipulateWithStamper(reader);
                ManipulateWithCopy(reader);
            }
            finally
            {
                reader.Close();
            }
        }


        /**
         * Creates a new PDF based on the one in the reader
         * @param reader a reader with a PDF file
         * @throws IOException
         * @throws DocumentException
         */
        private void ManipulateWithStamper(PdfReader reader)
        {
            PdfStamper stamper = new PdfStamper(reader, new MemoryStream());
            stamper.Close();
        }

        /**
         * Creates a new PDF based on the one in the reader
         * @param reader a reader with a PDF file
         * @throws IOException
         * @throws DocumentException
         */
        private void ManipulateWithCopy(PdfReader reader)
        {
            int n = reader.NumberOfPages;
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, new MemoryStream());
            document.Open();
            for (int i = 0; i < n; )
            {
                copy.AddPage(copy.GetImportedPage(reader, ++i));
            }
            document.Close();
        }

    }
}
