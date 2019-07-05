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
using iTextSharp.text.pdf;
using iTextSharp.text.xml.xmp;
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

        [Test]
        public void PdfObjectCheckTest() {
            PdfA1CheckerTest.PdfObjectCheck(OUT + "pdfObjectCheckTest.pdf", PdfAConformanceLevel.PDF_A_3B, false);
        }

        [Test]
        public void BarcodesTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new
                FileStream(OUT + "barcodesTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_3A);

            writer.SetTagged();
            document.Open();
            writer.ViewerPreferences = PdfWriter.DisplayDocTitle;
            document.AddTitle("Some title");
            document.AddLanguage("en-us");
            writer.CreateXmpMetadata();

            document.NewPage();

            // Set output intent. PDF/A requirement.
            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open,
                FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            // All fonts shall be embedded. PDF/A requirement.
            Font normal9 = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 9);
            Font normal8 = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 8);

            BaseColor color = new BaseColor(111, 211, 11);
            normal8.Color = color;

            PdfContentByte cb = writer.DirectContent;

            String code = "119716-500023718";
            Barcode barcode = new Barcode39();
            barcode.Code = code;
            barcode.StartStopText = false;
            barcode.Font = normal9.BaseFont;
            barcode.Extended = true;

            Image image = barcode.CreateImageWithBarcode(cb, color, color);
            image.Alt = "Bla Bla";
            document.Add(image);

            document.Close();
        }

        [Test]
        public void ZugferdInvoiceTest() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "zugferdInvoiceTest.pdf", FileMode.Create),
                PdfAConformanceLevel.ZUGFeRD);
            writer.CreateXmpMetadata();
            writer.XmpWriter.SetProperty(PdfAXmpWriter.zugferdSchemaNS, PdfAXmpWriter.zugferdDocumentFileName, "invoice.xml");
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open,
                FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfDictionary parameters = new PdfDictionary();
            parameters.Put(PdfName.MODDATE, new PdfDate());
            PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(
                writer, RESOURCES + "invoice.xml",
                "invoice.xml", null, "application/xml", parameters, 0);
            fileSpec.Put(PdfName.AFRELATIONSHIP, AFRelationshipValue.Alternative);
            writer.AddFileAttachment("invoice.xml", fileSpec);
            PdfArray array = new PdfArray();
            array.Add(fileSpec.Reference);
            writer.ExtraCatalog.Put(new PdfName("AF"), array);

            document.Close();
        }

    }
}
