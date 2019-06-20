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
using System.Xml;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using iTextSharp.text.pdf;
using iTextSharp.xmp;
using iTextSharp.xmp.options;

namespace iTextSharp.text.xml.xmp {
    [TestFixture]
    public class PdfAXmpWriterTest {

        public static String OUT_FOLDER = "PdfAXmpWriterTest/";
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
            PdfWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                      PdfAConformanceLevel.PDF_A_2B);
            writer.SetTagged();
            writer.CreateXmpMetadata();
            XmpWriter xmp = writer.XmpWriter;

            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Hello World");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "XMP & Metadata");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Metadata");

            PdfProperties.SetKeywords(xmp.XmpMeta, "Hello World, XMP & Metadata, Metadata");
            PdfProperties.SetVersion(xmp.XmpMeta, "1.4");

            // step 3
            document.Open();
            document.AddLanguage("en_US");

            // step 4
            Font font = FontFactory.GetFont("../../resources/text/pdfa/FreeMonoBold.ttf", BaseFont.WINANSI,
                                            BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            FileStream iccStream = new FileStream("../../resources/text/pdfa/sRGB Color Space Profile.icm",
                                                 FileMode.Open);
            ICC_Profile icc = ICC_Profile.GetInstance(iccStream);
            iccStream.Close();
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
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
            PdfWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                      PdfAConformanceLevel.PDF_A_1B);
            document.AddTitle("Hello World example");
            document.AddSubject("This example shows how to add metadata & XMP");
            document.AddKeywords("Metadata, iText, step 3");
            document.AddCreator("My program using 'iText'");
            document.AddAuthor("Bruno Lowagie & Paulo Soares");
            writer.CreateXmpMetadata();
            // step 3
            document.Open();
            Font font = FontFactory.GetFont("../../resources/text/pdfa/FreeMonoBold.ttf", BaseFont.WINANSI,
                                            BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccStream = new FileStream("../../resources/text/pdfa/sRGB Color Space Profile.icm",
                                                 FileMode.Open);
            ICC_Profile icc = ICC_Profile.GetInstance(iccStream);
            iccStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            // step 5
            document.Close();
            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void ManipulatePdfTest() {
            String fileName = "xmp_metadata_added.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            PdfStamper stamper = new PdfAStamper(reader, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                 PdfAConformanceLevel.PDF_A_1A);
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
        virtual public void ManipulatePdf2Test() {
            String fileName = "xmp_metadata_added2.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            PdfStamper stamper = new PdfAStamper(reader, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                 PdfAConformanceLevel.PDF_A_1A);
            MemoryStream os = new MemoryStream();
            XmpWriter xmp = new PdfAXmpWriter(os, reader.Info, PdfAConformanceLevel.PDF_A_1A, stamper.Writer);
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Hello World");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "XMP & Metadata");
            DublinCoreProperties.AddSubject(xmp.XmpMeta, "Metadata");

            PdfProperties.SetVersion(xmp.XmpMeta, "1.4");
            xmp.Close();
            stamper.XmpMetadata = os.ToArray();
            stamper.Close();
            reader.Close();
            CompareResults(fileName, fileName);
        }

        [Test]
        virtual public void ManipulatePdfAutomaticTest() {
            String fileName = "xmp_metadata_updated.pdf";
            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            PdfStamper stamper = new PdfAStamper(reader, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                 PdfAConformanceLevel.PDF_A_1A);

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
            PdfWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create),
                                                      PdfAConformanceLevel.PDF_A_2B);
            MemoryStream os = new MemoryStream();
            XmpWriter xmp = new PdfAXmpWriter(os, PdfAConformanceLevel.PDF_A_2B, writer);
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
            document.AddLanguage("en_US");
            // step 4
            Font font = FontFactory.GetFont("../../resources/text/pdfa/FreeMonoBold.ttf", BaseFont.WINANSI,
                                            BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccStream = new FileStream("../../resources/text/pdfa/sRGB Color Space Profile.icm",
                                                 FileMode.Open);
            ICC_Profile icc = ICC_Profile.GetInstance(iccStream);
            iccStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            // step 5
            document.Close();
            CompareResults("xmp_metadata_deprecated.pdf", fileName);
        }

        [Test]
        virtual public void XmpEncodingTest() {
            String fileName = "xmp_utf-8_encoding";
            Document document = new Document();
            PdfSmartCopy copy = new PdfSmartCopy(document, new FileStream(OUT_FOLDER + fileName, FileMode.Create));

            document.Open();

            PdfReader reader = new PdfReader(CMP_FOLDER + "pdf_metadata.pdf");
            int pageCount = reader.NumberOfPages;

            for (int currentPage = 1; currentPage <= pageCount; currentPage++) {
                PdfImportedPage page = copy.GetImportedPage(reader, currentPage);
                copy.AddPage(page);
            }


            PdfAConformanceLevel pdfaLevel = PdfAConformanceLevel.PDF_A_1B;
            MemoryStream os = new MemoryStream();
            PdfAXmpWriter xmp = new PdfAXmpWriter(os, copy.Info, pdfaLevel, copy);
            xmp.Close();

            copy.XmpMetadata = os.ToArray();

            string metadataXml = System.Text.Encoding.GetEncoding("UTF-8").GetString(copy.XmpMetadata);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(metadataXml);  //<-- This is where the exception is thrown

            
            document.Close();
            copy.Close();
            reader.Close();
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
                Assert.Fail("The XMP packages are different!!!");
            }
        }
    }
}
