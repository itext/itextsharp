using System;
using System.Collections.Generic;
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfReaderTest
    {
        [SetUp]
        public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PdfReaderTest\";

        [Test, Ignore("validity of test needs to be resolved")]
        public void TestGetLink()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest1.pdf");
            PdfReader currentReader = new PdfReader(testFile);
            Document document = new Document(PageSize.A4, 0, 0, 0, 0);
            PdfWriter writer = PdfWriter.GetInstance(document, new MemoryStream());
            document.Open();
            document.NewPage();
            List<PdfAnnotation.PdfImportedLink> links = currentReader.GetLinks(1);
            PdfAnnotation.PdfImportedLink link = links[0];
            writer.AddAnnotation(link.CreateAnnotation(writer));
            document.Close();

            currentReader.Close();
        }

        [Test]
        public void testGetLink2()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest2.pdf");
            string filename = testFile;
            PdfReader rdr = new PdfReader(new RandomAccessFileOrArray(filename), new byte[0]);
            // this one works: PdfReader rdr = new PdfReader(filename);
            rdr.ConsolidateNamedDestinations(); // does not help
            rdr.GetLinks(1);

            rdr.Close();
        }

        [Test]
        public void testPageResources()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest2.pdf");
            String filename = testFile;
            PdfReader rdr = new PdfReader(new RandomAccessFileOrArray(filename), new byte[0]);

            PdfDictionary pageResFromNum = rdr.GetPageResources(1);
            PdfDictionary pageResFromDict = rdr.GetPageResources(rdr.GetPageN(1));
            // same size & keys
            Assert.IsTrue(pageResFromNum.Keys.Equals(pageResFromDict.Keys));

            rdr.Close();
        }
    }
}
