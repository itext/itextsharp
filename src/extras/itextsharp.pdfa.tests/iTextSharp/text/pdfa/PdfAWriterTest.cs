/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
        virtual public void Initialize()
        {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
            Document.Compress = false;
        }

        [Test]
        virtual public void TestCreatePdfA_1()
        {
            Document document;
            PdfAWriter writer;
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

                FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
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
        virtual public void TestCreatePdfA_2()
        {
            bool exceptionThrown = false;
            Document document;
            PdfAWriter writer;
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

                FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
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
        virtual public void TestPdfAStamper1()
        {
            string filename = OUT + "TestPdfAStamper1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
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
        virtual public void TestPdfAStamper2()
        {
            string filename = OUT + "TestPdfAStamper2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
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
        virtual public void TestPdfAStamper3()
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
