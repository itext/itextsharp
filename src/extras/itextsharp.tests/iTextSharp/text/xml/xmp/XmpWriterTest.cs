using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using iTextSharp.text.pdf;
using iTextSharp.xmp;
using iTextSharp.xmp.options;

namespace iTextSharp.text.xml.xmp {
    [TestFixture]
    public class XmpWriterTest {

        public static String OUT_FOLDER = "XmpWriterTest/";
        public static String CMP_FOLDER = @"../../resources/text/xml/xmp/";

        [TestFixtureSetUp]
        virtual public void Init() {
            if (Directory.Exists(OUT_FOLDER)) {
                foreach (String path in Directory.GetFiles(OUT_FOLDER))
                    if (File.Exists(path))
                        File.Delete(path);
            } else
                Directory.CreateDirectory(OUT_FOLDER);
        }

        [Test]
        virtual public void CreatePdfTest() {
            String fileName = "xmp_metadata.pdf";
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create));
            MemoryStream os = new MemoryStream();
            XmpWriter xmp = new XmpWriter(os, XmpWriter.UTF16, 2000);

            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Hello World");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "XMP & Metadata");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Metadata");

            PdfProperties.SetKeywords(xmp.XmpMeta, "Hello World, XMP & Metadata, Metadata");
            PdfProperties.SetVersion(xmp.XmpMeta, "1.4");

            xmp.Close();

            writer.XmpMetadata = os.ToArray();
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World"));
            // step 5
            document.Close();

            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void CreatePdfAutomaticTest() {
            String fileName = "xmp_metadata_automatic.pdf";
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create));
            document.AddTitle("Hello World example");
            document.AddSubject("This example shows how to add metadata & XMP");
            document.AddKeywords("Metadata, iText, step 3");
            document.AddCreator("My program using 'iText'");
            document.AddAuthor("Bruno Lowagie & Paulo Soares");
            writer.CreateXmpMetadata();
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World"));
            // step 5
            document.Close();
            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void ManipulatePdfTest() {
            String fileName = "xmp_metadata_added.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUT_FOLDER + fileName, FileMode.Create));
            Dictionary<String, String> info = reader.Info;
            MemoryStream baos = new MemoryStream();
            XmpWriter xmp = new XmpWriter(baos, info);
            xmp.Close();
            stamper.XmpMetadata = baos.ToArray();
            stamper.Close();
            reader.Close();

            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void ManipulatePdf2Test() {
            String fileName = "xmp_metadata_added2.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUT_FOLDER + fileName, FileMode.Create));
            stamper.CreateXmpMetadata();
            XmpWriter xmp = stamper.XmpWriter;
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Hello World");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "XMP & Metadata");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Metadata");

            PdfProperties.SetVersion(xmp.XmpMeta, "1.4");
            stamper.Close();
            reader.Close();

            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void DeprecatedLogicTest() {
            String fileName = "xmp_metadata_deprecated.pdf";
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create));
            MemoryStream os = new MemoryStream();
            XmpWriter xmp = new XmpWriter(os);
            XmpSchema dc = new DublinCoreSchema();
            XmpArray subject = new XmpArray(XmpArray.UNORDERED);
            subject.Add("Hello World");
            subject.Add("XMP & Metadata");
            subject.Add("Metadata");
            dc.SetProperty(DublinCoreSchema.SUBJECT, subject);
            xmp.AddRdfDescription(dc.Xmlns, dc.ToString());
            PdfSchema pdf = new PdfSchema();
            pdf.AddKeywords("Hello World, XMP & Metadata, Metadata");
            pdf.AddVersion("1.4");
            xmp.AddRdfDescription(pdf);
            xmp.Close();
            writer.XmpMetadata = os.ToArray();
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World"));
            // step 5
            document.Close();
            CompareResults("xmp_metadata.pdf", fileName);
        }

        private void CompareResults(String orig, String curr) {
            PdfReader cmpReader = new PdfReader(CMP_FOLDER + orig);
            PdfReader outReader = new PdfReader(OUT_FOLDER + curr);
            byte[] cmpBytes = cmpReader.Metadata, outBytes = outReader.Metadata;
            IXmpMeta xmpMeta = XmpMetaFactory.ParseFromBuffer(cmpBytes);

            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.CREATEDATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.MODIFYDATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.METADATADATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_PDF, PdfProperties.PRODUCER, true, true);

            cmpBytes = XmpMetaFactory.SerializeToBuffer(xmpMeta, new SerializeOptions(SerializeOptions.SORT));

            xmpMeta = XmpMetaFactory.ParseFromBuffer(outBytes);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.CREATEDATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.MODIFYDATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_XMP, XmpBasicProperties.METADATADATE, true, true);
            XmpUtils.RemoveProperties(xmpMeta, XmpConst.NS_PDF, PdfProperties.PRODUCER, true, true);

            outBytes = XmpMetaFactory.SerializeToBuffer(xmpMeta, new SerializeOptions(SerializeOptions.SORT));

            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            if (!xmldiff.Compare(new XmlTextReader(new MemoryStream(cmpBytes)),
                                 new XmlTextReader(new MemoryStream(outBytes)))) {
                String currXmlName = curr.Replace(".pdf", ".xml");
                FileStream outStream = new FileStream(OUT_FOLDER + "cmp_" + currXmlName, FileMode.Create);
                outStream.Write(cmpBytes, 0, cmpBytes.Length);

                outStream = new FileStream(OUT_FOLDER + currXmlName, FileMode.Create);
                outStream.Write(outBytes, 0, outBytes.Length);
                Assert.Fail("The XMP packages are different!");
            }
        }
    }
}
