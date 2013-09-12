using System;
using System.IO;
using NUnit.Framework;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdfa
{
    [TestFixture]
    public class PdfAWriterTest
    {
        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        public const String TARGET = "PdfAWriterTest\\";
        public const String OUT = TARGET + "pdf\\out";


        [SetUp]
        public void Initialize()
        {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
            Document.Compress = false;
        }

        [Test]
        public void TestCreatePdfA_1()
        {
            Document document = null;
            PdfAWriter writer = null;
            try
            {
                string filename = OUT + "TestCreatePdfA_1.pdf";
                FileStream fos = new FileStream(filename, FileMode.Create);

                document = new Document();

                writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
                writer.CreateXmpMetadata();

                document.Open();

                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
                document.Add(new Paragraph("Hello World", font));

                FileStream iccProfileFileStream = new FileStream(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open);
                ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                Assert.Fail("PdfAConformance exception should not be thrown: " + e.Message);
            }
        }

        [Test]
        public void TestCreatePdfA_2()
        {
            bool exceptionThrown = false;
            Document document = null;
            PdfAWriter writer = null;
            try
            {
                string filename = OUT + "TestCreatePdfA_1.pdf";
                FileStream fos = new FileStream(filename, FileMode.Create);

                document = new Document();

                writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
                writer.CreateXmpMetadata();

                document.Open();

                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.NOT_EMBEDDED, 12, Font.BOLD);
                document.Add(new Paragraph("Hello World", font));

                FileStream iccProfileFileStream = new FileStream(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read);
                ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
                document.Close();
            }
            catch (PdfAConformanceException)
            {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        public void TestPdfAStamper1()
        {
            string filename = OUT + "TestPdfAStamper1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = new FileStream(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();

            PdfReader reader = new PdfReader(filename);
            FileStream stamperFileStream = new FileStream(OUT + "TestPdfAStamper1_.pdf", FileMode.Create);
            PdfAStamper stamper = new PdfAStamper(reader, stamperFileStream, PdfAConformanceLevel.PDF_A_1B);
            stamper.Close();
            stamperFileStream.Close();
            reader.Close();
        }

        [Test]
        public void TestPdfAStamper2()
        {
            string filename = OUT + "TestPdfAStamper2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = new FileStream(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();

            PdfReader reader = new PdfReader(filename);
            bool exceptionThrown = false;
            try
            {
                FileStream stamperFileStream = new FileStream(OUT + "TestPdfAStamper2_.pdf", FileMode.Create);
                PdfAStamper stamper = new PdfAStamper(reader, stamperFileStream, PdfAConformanceLevel.PDF_A_1B);
                stamper.Close();
                stamperFileStream.Close();
            }
            catch (PdfAConformanceException)
            {
                exceptionThrown = true;
            }
            reader.Close();
            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        public void TestPdfAStamper3()
        {
            string filename = OUT + "TestPdfAStamper3.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfWriter writer = PdfWriter.GetInstance(document, fos);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            document.Close();

            PdfReader reader = new PdfReader(filename);
            bool exceptionThrown = false;
            try
            {
                FileStream stamperFileStream = new FileStream(OUT + "TestPdfAStamper3_.pdf", FileMode.Create);
                PdfAStamper stamper = new PdfAStamper(reader, stamperFileStream, PdfAConformanceLevel.PDF_A_1A);
                stamper.Close();
                stamperFileStream.Close();
            }
            catch (PdfAConformanceException)
            {
                exceptionThrown = true;
            }
            reader.Close();
            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }
    }
}
