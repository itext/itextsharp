using System;
using System.IO;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace iTextSharp.text.pdfa {
    [TestFixture]
    internal class PdfA3CheckerTest {
        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        public const String TARGET = "PdfA3CheckerTest\\";
        public const String OUT = TARGET + "pdf\\out";


        [SetUp]
        virtual public void Initialize() {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
        }

        [Test]
        virtual public void FileSpecCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            MemoryStream txt = new MemoryStream();
            StreamWriter outp = new StreamWriter(txt);
            outp.Write("<foo><foo2>Hello world</foo2></foo>");
            outp.Close();
            writer.AddFileAttachment("foo file", txt.ToArray(), "foo.xml", "foo.xml", "application/xml",
                AFRelationshipValue.Source);

            document.Close();
        }

        [Test]
        virtual public void FileSpecCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            MemoryStream txt = new MemoryStream();
            StreamWriter outp = new StreamWriter(txt);
            outp.Write("<foo><foo2>Hello world</foo2></foo>");
            outp.Close();

            PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(writer, null, "foo.xml", txt.ToArray());
            fs.Put(PdfName.AFRELATIONSHIP, AFRelationshipValue.Unspecified);

            writer.AddFileAttachment(fs);

            document.Close();
        }

        [Test]
        virtual public void FileSpecCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            byte[] somePdf = new byte[25];
            writer.AddFileAttachment("some pdf file", somePdf, "foo.pdf", "foo.pdf", PdfAWriter.MimeTypePdf,
                AFRelationshipValue.Data);

            document.Close();
        }

        [Test]
        virtual public void FileSpecCheckTest4() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest4.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            byte[] somePdf = new byte[25];
            writer.AddPdfAttachment("some pdf file", somePdf, "foo.pdf", "foo.pdf");

            document.Close();
        }

        [Test]
        virtual public void FileSpecCheckTest5() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest5.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            MemoryStream txt = new MemoryStream();
            StreamWriter outp = new StreamWriter(txt);
            outp.Write("<foo><foo2>Hello world</foo2></foo>");
            outp.Close();

            bool exceptionThrown = false;
            try {
                PdfFileSpecification fs
                    = PdfFileSpecification.FileEmbedded(writer,
                        null, "foo.xml", txt.ToArray());
                writer.AddFileAttachment(fs);
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() != null && e.Message.Equals("The file specification dictionary for an embedded file shall contain correct AFRelationship key.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void FileSpecCheckTest6() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest2.pdf", FileMode.Create),
                PdfAConformanceLevel.PDF_A_3B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open,
                FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfDictionary _params = new PdfDictionary();
            _params.Put(PdfName.MODDATE, new PdfDate());
            PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(
                writer, RESOURCES + "foo.xml", "foo.xml", null, false, "text/xml", _params);
            fileSpec.Put(PdfName.AFRELATIONSHIP, AFRelationshipValue.Data);

            writer.AddFileAttachment(fileSpec);

            document.Close();
        }

        [Test]
        public virtual void FileSpecCheckTest7() {
            FileStream inPdf = new FileStream(RESOURCES + "fileSpec.pdf", FileMode.Open);
            MemoryStream xml = new MemoryStream();
            StreamWriter sr = new StreamWriter(xml);
            sr.Write("<foo><foo2>Hello world</foo2></foo>");
            sr.Close();

            MemoryStream output = new MemoryStream();
            PdfReader reader = new PdfReader(inPdf);
            PdfAStamper stamper = new PdfAStamper(reader, output, PdfAConformanceLevel.PDF_A_3B);

            stamper.CreateXmpMetadata();

            PdfDictionary embeddedFileParams = new PdfDictionary();
            embeddedFileParams.Put(PdfName.MODDATE, new PdfDate());
            PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(stamper.Writer, "foo", "foo",
                xml.ToArray(), "text/xml", embeddedFileParams, 0);
            fs.Put(PdfName.AFRELATIONSHIP, AFRelationshipValue.Source);
            stamper.AddFileAttachment("description", fs);

            stamper.Close();
            reader.Close();
        }
    }
}
