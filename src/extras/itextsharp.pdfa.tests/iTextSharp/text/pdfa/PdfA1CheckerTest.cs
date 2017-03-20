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
using System.Text;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace iTextSharp.text.pdfa
{

    [TestFixture]
    public class PdfA1CheckerTest
    {
        static PdfA1CheckerTest() {
            try {
                MessageLocalization.SetLanguage("en", "US");
            }
            catch (IOException e) {
            }
        }

        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        public const String TARGET = "PdfA1CheckerTest\\";
        public const String OUT = TARGET + "pdf\\out";

        private bool initialByteBufferHightPrecisionState;

        [SetUp]
        virtual public void Initialize()
        {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");

            initialByteBufferHightPrecisionState = ByteBuffer.HIGH_PRECISION;
        }

        [TearDown]
        virtual public void TearDown()
        {
            ByteBuffer.HIGH_PRECISION = initialByteBufferHightPrecisionState;
        }

        [Test]
        virtual public void MetadaCheckTest() {
            string filename = OUT + "metadaPDFA1CheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            document.Open();
            PdfContentByte canvas = writer.DirectContent;

            canvas.SetColorFill(BaseColor.LIGHT_GRAY);
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.LineTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.Fill();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException exc) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");
        }

        [Test]
        virtual public void TrailerCheckTest()
        {
            string filename = OUT + "TrailerCheckTest.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();
            writer.SetEncryption(null, null, 1, PdfWriter.STANDARD_ENCRYPTION_40);

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.Message.Contains("Encrypt"))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void FileSpecCheckTest()
        {
            string filename = OUT + "FileSpecCheckTest.pdf";
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

            PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(writer, RESOURCES + "sRGB Color Space Profile.icm", "sRGB Color Space Profile.icm", null);
            writer.ExtraCatalog.Put(new PdfName("EmbeddedFile"), fs);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(fs))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void PdfObjectCheckTest1()
        {

            string filename = OUT + "PdfObjectCheckTest1.pdf";
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

            ByteBuffer.HIGH_PRECISION = true;
            PdfNumber num = new PdfNumber(65535.12);
            writer.ExtraCatalog.Put(new PdfName("TestNumber"), num);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(num))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void PdfObjectCheckTest2()
        {
            string filename = OUT + "PdfObjectCheckTest2.pdf";
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

            String str = "";
            for (int i = 0; i < 65536; i++)
            {
                str += 'a';
            }
            PdfString pdfStr = new PdfString(str);
            writer.ExtraCatalog.Put(new PdfName("TestString"), pdfStr);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(pdfStr))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void PdfObjectCheckTest3()
        {
            string filename = OUT + "PdfObjectCheckTest3.pdf";
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

            String str = "";
            for (int i = 0; i < 65535; i++)
            {
                str += 'a';
            }
            PdfString pdfStr = new PdfString(str);
            writer.ExtraCatalog.Put(new PdfName("TestString"), pdfStr);
            document.Close();
        }

        // This method is used in the PdfA2CheckerTest and PdfA3CheckerTest, too
        public static void PdfObjectCheck(string output, PdfAConformanceLevel level, bool exceptionExpected)
        {
            FileStream fos = new FileStream(output, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, level);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfArray array = new PdfArray();
            for (int i = 0; i < 8192; i++)
            {
                array.Add(new PdfNull());
            }
            writer.ExtraCatalog.Put(new PdfName("TestArray"), array);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(array))
                    exceptionThrown = true;
            }
            if (exceptionThrown != exceptionExpected) {
                String error = exceptionExpected ? "" : " not";
                error = String.Format("PdfAConformanceException should{0} be thrown.", error);

                Assert.Fail(error);
            }
        }

        [Test]
        public void PdfObjectCheckTest4() {
            PdfObjectCheck(OUT + "PdfObjectCheckTest4.pdf", PdfAConformanceLevel.PDF_A_1B, true);
        }

        [Test]
        public void PdfNamedDestinationsOverflow() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfNamedDestinationsOverflow.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();
            writer.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfDocument pdf = writer.PdfDocument;
            for (int i = 0; i < 8200; i++) {
                PdfDestination dest = new PdfDestination(PdfDestination.FITV);
                pdf.LocalDestination("action" + i, dest);
            }
            document.Close();
        }

        [Test]
        virtual public void PdfObjectCheckTest5()
        {
            string filename = OUT + "PdfObjectCheckTest5.pdf";
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

            PdfArray array = new PdfArray();
            for (int i = 0; i < 8191; i++)
            {
                array.Add(new PdfNull());
            }
            writer.ExtraCatalog.Put(new PdfName("TestArray"), array);
            document.Close();
        }

        [Test]
        virtual public void PdfObjectCheckTest6()
        {
            string filename = OUT + "PdfObjectCheckTest6.pdf";
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

            PdfDictionary dictionary = new PdfDictionary();
            for (int i = 0; i < 4096; i++)
            {
                dictionary.Put(new PdfName("a" + i.ToString()), new PdfName("b" + i.ToString()));
            }
            writer.ExtraCatalog.Put(new PdfName("TestDictionary"), dictionary);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(dictionary))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");

        }

        [Test]
        virtual public void PdfObjectCheckTest7()
        {
            string filename = OUT + "PdfObjectCheckTest7.pdf";
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

            PdfDictionary dictionary = new PdfDictionary();
            for (int i = 0; i < 4095; i++)
            {
                dictionary.Put(new PdfName("a" + i.ToString()), new PdfName("b" + i.ToString()));
            }
            writer.ExtraCatalog.Put(new PdfName("TestDictionary"), dictionary);
            document.Close();

        }

        [Test]
        virtual public void CanvasCheckTest1()
        {
            string filename = OUT + "CanvasCheckTest1.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            bool exceptionThrown = false;
            try
            {
                for (int i = 0; i < 29; i++)
                {
                    canvas.SaveState();
                }
            }
            catch (PdfAConformanceException e)
            {
                if ("q".Equals(e.GetObject()))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
            for (int i = 0; i < 28; i++)
            {
                canvas.RestoreState();
            }

            document.Close();

        }

        [Test]
        virtual public void CanvasCheckTest2()
        {
            string filename = OUT + "pdfa1CanvasCheckTest2.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            for (int i = 0; i < 28; i++)
            {
                canvas.SaveState();
            }
            for (int i = 0; i < 28; i++)
            {
                canvas.RestoreState();
            }
            document.Close();
        }

        [Test]
        virtual public void ColorCheckTest1()
        {
            string filename = OUT + "pdfa1ColorCheckTest1.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            bool exceptionThrown = false;
            try
            {
                canvas.SetColorFill(new CMYKColor(0.1f, 0.1f, 0.1f, 0.1f));
                canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
                canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
                canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
                canvas.Fill();
                canvas.SetColorFill(BaseColor.RED);
                canvas.MoveTo(writer.PageSize.Right, writer.PageSize.Top);
                canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
                canvas.LineTo(writer.PageSize.Left, writer.PageSize.Bottom);
                canvas.Fill();
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (BaseColor.RED.Equals(e.GetObject()))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void ColorCheckTest2()
        {
            string filename = OUT + "pdfa1ColorCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12, Font.NORMAL, BaseColor.RED);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfContentByte canvas = writer.DirectContent;
            bool exceptionThrown = false;
            canvas.SetColorFill(new CMYKColor(0.1f, 0.1f, 0.1f, 0.1f));
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (BaseColor.RED.Equals(e.GetObject()))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void ColorCheckTest3() {
            Document document = new Document();
            string filename = OUT + "pdfa1ColorCheckTest3.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf",
                BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            canvas.SetColorFill(BaseColor.GREEN);
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");

            fos.Close();

            document = new Document();
            fos = new FileStream(filename, FileMode.Create);
            writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();
            document.Open();

            font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI,
                BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            canvas = writer.DirectContent;
            canvas.SetColorFill(BaseColor.GREEN);
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            document.Close();
        }

        [Test]
        virtual public void ColorCheckTest4() {
            Document document = new Document();
            string filename = OUT + "pdfa1ColorCheckTest4.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();
            
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf",
                BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            document.Close();

            PdfReader reader = new PdfReader(OUT + "pdfa1ColorCheckTest4.pdf");
            PdfAStamper stamper = new PdfAStamper(reader,
                new FileStream(OUT + "pdfa1ColorCheckTest4_updating_failed.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            bool exceptionThrown = false;
            try {
                iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
                icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                stamper.Writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
                font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf",
                    BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
                font.Color = BaseColor.RED;
                PdfContentByte canvas = stamper.GetOverContent(1);
                canvas.SetFontAndSize(font.BaseFont, 12);
                canvas.SetColorFill(BaseColor.RED);
                ColumnText.ShowTextAligned(canvas,
                    Element.ALIGN_LEFT, new Paragraph("Hello World", font), 36, 775, 760);
                stamper.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        virtual public void EgsCheckTest1()
        {
            string filename = OUT + "EgsCheckTest1.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR, new PdfName("Test"));
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(gs))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");

        }


        [Test]
        virtual public void EgsCheckTest2()
        {
            string filename = OUT + "EgsCheckTest2.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR2, PdfName.DEFAULT);
            canvas.SetGState(gs);
            document.Close();

        }

        [Test]
        virtual public void EgsCheckTest3()
        {
            string filename = OUT + "EgsCheckTest3.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR2, new PdfName("Test"));
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(gs))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");

        }

        [Test]
        virtual public void EgsCheckTest4()
        {
            string filename = OUT + "EgsCheckTest4.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.RI, new PdfName("Test"));
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(gs))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void TransparencyCheckTest1()
        {
            string filename = OUT + "TransparencyCheckTest1.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfTemplate template = PdfTemplate.CreateTemplate(writer, 100, 100);
            PdfTransparencyGroup group = new PdfTransparencyGroup();
            template.Group = group;
            canvas.AddTemplate(template, 100, 100);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (((PdfFormXObject)e.GetObject()).GetAsDict(PdfName.GROUP).Equals(group))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void TransparencyCheckTest2()
        {
            string filename = OUT + "TransparencyCheckTest2.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.SMASK, new PdfName("Test"));
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(gs))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void TransparencyCheckTest3()
        {
            string filename = OUT + "TransparencyCheckTest3.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.SMASK, PdfName.NONE);

            canvas.SetGState(gs);
            document.Close();
        }

        [Test]
        virtual public void TransparencyCheckTest4()
        {
            string filename = OUT + "TransparencyCheckTest4.pdf";
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

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();

            canvas.SetGState(gs);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest1()
        {
            string filename = OUT + "AnnotationCheckTest1.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, new PdfName("Movie"));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /Movie not allowed.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest2()
        {
            string filename = OUT + "AnnotationCheckTest2.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.CA, new PdfNumber(0.5));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject().Equals(annot) &&
                    e.Message.Equals("An annotation dictionary shall not contain the CA key with a value other than 1.0.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest3()
        {
            string filename = OUT + "annotationCheckTest3.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(0));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject().Equals(annot) && e.Message.Equals("The F key's Print flag bit shall be set to 1 and its Hidden, Invisible and NoView flag bits shall be set to 0.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest4()
        {
            string filename = OUT + "AnnotationCheckTest4.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT & PdfAnnotation.FLAGS_INVISIBLE));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject().Equals(annot) && e.Message.Equals("The F key's Print flag bit shall be set to 1 and its Hidden, Invisible and NoView flag bits shall be set to 0.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest5()
        {
            string filename = OUT + "AnnotationCheckTest5.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.ASCII.GetBytes("Hello World"));
            ap.Put(PdfName.D, new PdfDictionary());
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Appearance dictionary shall contain only the N key with stream value."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest6()
        {
            string filename = OUT + "AnnotationCheckTest6.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfDictionary ap = new PdfDictionary();
            ap.Put(PdfName.N, new PdfDictionary());
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Appearance dictionary shall contain only the N key with stream value."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest7()
        {
            string filename = OUT + "AnnotationCheckTest7.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_NOZOOM | PdfAnnotation.FLAGS_NOROTATE));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest8()
        {
            string filename = OUT + "AnnotationCheckTest8.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.STAMP);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation of type /Stamp should have Contents key."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest9()
        {
            string filename = OUT + "annotationCheckTest9.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();
            writer.SetTagged();

            document.Open();
            document.AddLanguage("en-US");

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close(); 

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.STAMP);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.CONTENTS, new PdfString("Hello World"));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest10()
        {
            string filename = OUT + "AnnotationCheckTest10.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.POLYGON);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /Polygon not allowed."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest11()
        {
            string filename = OUT + "AnnotationCheckTest11.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.POLYLINE);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /PolyLine not allowed."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest12()
        {
            string filename = OUT + "AnnotationCheckTest12.pdf";
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

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.CARET);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /Caret not allowed."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest13()
        {
            string filename = OUT + "AnnotationCheckTest13.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.WATERMARK);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /Watermark not allowed."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest14()
        {
            string filename = OUT + "AnnotationCheckTest14.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.FILEATTACHMENT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().Equals(annot) && e.Message.Equals("Annotation type /FileAttachment not allowed."))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void FieldCheckTest1()
        {
            String[] LANGUAGES = { "Russian", "English", "Dutch", "French", "Spanish", "German" };

            string filename = OUT + "fieldCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);


            PdfContentByte canvas = writer.DirectContent;
            Rectangle rect;
            PdfFormField field;
            PdfFormField radiogroup
                    = PdfFormField.CreateRadioButton(writer, true);
            radiogroup.FieldName = "language";
            RadioCheckField radio;
            for (int i = 0; i < LANGUAGES.Length; i++)
            {
                rect = new Rectangle(
                        40, 806 - i * 40, 60, 788 - i * 40);
                radio = new PdfARadioCheckField(
                        writer, rect, null, LANGUAGES[i]);
                radio.BorderColor = GrayColor.GRAYBLACK;
                radio.BackgroundColor = GrayColor.GRAYWHITE;
                radio.CheckType = i + 1;
                radio.Checked = true;
                field = radio.RadioField;
                radiogroup.AddKid(field);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT,
                        new Phrase(LANGUAGES[i], font), 70, 790 - i * 40, 0);
            }
            writer.AddAnnotation(radiogroup);


            document.Close();
        }

        [Test]
        virtual public void FieldCheckTest2()
        {
            String[] LANGUAGES = { "Russian", "English", "Dutch", "French" };

            string filename = OUT + "FieldCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);


            PdfFormField radiogroup
                    = PdfFormField.CreateRadioButton(writer, true);
            radiogroup.FieldName = "language";
            Rectangle rect = new Rectangle(40, 806, 60, 788);
            RadioCheckField radio;
            PdfFormField radiofield;
            for (int page = 0; page < LANGUAGES.Length; )
            {
                radio = new PdfARadioCheckField(writer, rect, null, LANGUAGES[page]);
                radio.BackgroundColor = new GrayColor(0.8f);
                radiofield = radio.RadioField;
                radiofield.PlaceInPage = ++page;
                radiogroup.AddKid(radiofield);
            }
            writer.AddAnnotation(radiogroup);
            PdfContentByte cb = writer.DirectContent;
            for (int i = 0; i < LANGUAGES.Length; i++)
            {
                cb.BeginText();
                cb.SetFontAndSize(font.BaseFont, 18);
                cb.ShowTextAligned(Element.ALIGN_LEFT, LANGUAGES[i], 70, 790, 0);
                cb.EndText();
                document.NewPage();
            }
            document.Close();
        }

        [Test]
        virtual public void FieldCheckTest3()
        {
            string filename = OUT + "FieldCheckTest3.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);


            Rectangle rect = new Rectangle(300, 806, 360, 788);
            PushbuttonField button
                    = new PushbuttonField(writer, rect, "Buttons");
            button.Font = font.BaseFont;
            button.BackgroundColor = new GrayColor(0.75f);
            button.BorderColor = GrayColor.GRAYBLACK;
            button.BorderWidth = 1;
            button.BorderStyle =
                    PdfBorderDictionary.STYLE_BEVELED;
            button.TextColor = GrayColor.GRAYBLACK;
            button.FontSize = 12;
            button.Text = "Push me";
            button.Layout =
                    PushbuttonField.LAYOUT_ICON_LEFT_LABEL_RIGHT;
            button.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
            button.ProportionalIcon = true;
            button.IconHorizontalAdjustment = 0;
            PdfFormField field = button.Field;
            writer.AddAnnotation(field);

            document.Close();
        }

        [Test]
        virtual public void FieldCheckTest4()
        {
            string filename = OUT + "FieldCheckTest4.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);


            Rectangle rect = new Rectangle(300, 806, 360, 788);
            TextField text
                    = new TextField(writer, rect, String.Format("text_{0}", 1));
            text.BackgroundColor = new GrayColor(0.75f);
            text.Font = font.BaseFont;
            text.BorderStyle = PdfBorderDictionary.STYLE_BEVELED;
            text.Text = "Enter your name here...";
            text.FontSize = 0;
            text.Alignment = Element.ALIGN_CENTER;
            text.Options = TextField.REQUIRED;
            PdfFormField field = text.GetTextField();
            writer.AddAnnotation(field);

            document.Close();
        }

        [Test]
        virtual public void FieldCheckTest5()
        {
            String[] LANGUAGES = { "Russian", "English", "Dutch", "French" };

            string filename = OUT + "FieldCheckTest5.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);


            PdfContentByte canvas = writer.DirectContent;
            Rectangle rect;
            PdfFormField field;
            PdfFormField radiogroup
                    = PdfFormField.CreateRadioButton(writer, true);
            radiogroup.FieldName = "language";
            RadioCheckField radio;
            for (int i = 0; i < LANGUAGES.Length; i++)
            {
                rect = new Rectangle(
                        40, 806 - i * 40, 60, 788 - i * 40);
                radio = new RadioCheckField(
                        writer, rect, null, LANGUAGES[i]);
                radio.Font = font.BaseFont;
                radio.BorderColor = GrayColor.GRAYBLACK;
                radio.BackgroundColor = GrayColor.GRAYWHITE;
                field = radio.RadioField;
                radiogroup.AddKid(field);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT,
                        new Phrase(LANGUAGES[i], font), 70, 790 - i * 40, 0);
            }
            writer.AddAnnotation(radiogroup);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.Message.Contains(".n.") || e.Message.Contains(" N "))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void MarkInfoCheckTest1()
        {
            string filename = OUT + "MarkInfoCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.SetTagged();
            writer.CreateXmpMetadata();

            document.Open();
            document.AddLanguage("en-us");

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 72);
            Chunk c = new Chunk("Document Header", font);
            Paragraph h1 = new Paragraph(c);
            h1.Role = PdfName.H1;
            document.Add(h1);

            document.Add(new Paragraph("Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            document.Close();
        }

        [Test]
        virtual public void MarkInfoCheckTest2()
        {
            string filename = OUT + "MarkInfoCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.CreateXmpMetadata();

            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close(); 

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.Message.Contains("markinfo") || e.Message.Contains("MarkInfo"))
                    exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void RoleMapCheckTest1()
        {
            string filename = OUT + "RoleMapCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.SetTagged();
            writer.CreateXmpMetadata();

            document.Open();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close(); 

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 72);
            Paragraph p = new Paragraph("Hello World", font);
            p.Role = new PdfName("HW");

            bool exceptionThrown = false;
            try
            {
                document.Add(p);
            }
            catch (DocumentException e)
            {
                if (e.Message.Contains("/HW"))
                {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
            {
                Assert.Fail("DocumentException should be thrown");
            }
        }

        [Test]
        virtual public void RoleMapCheckTest2()
        {
            string filename = OUT + "RoleMapCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.SetTagged();
            writer.CreateXmpMetadata();

            document.Open();
            document.AddLanguage("en-us");

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close(); 

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 72);
            Paragraph p = new Paragraph("Hello World", font);
            p.Role = new PdfName("HW");
            writer.StructureTreeRoot.MapRole(new PdfName("HW"), PdfName.P);

            document.Add(p);
            document.Close();
        }

        [Test]
        virtual public void RoleMapCheckTest3()
        {
            string filename = OUT + "RoleMapCheckTest3.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);

            Document document = new Document();

            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_1A);
            writer.SetTagged();
            writer.CreateXmpMetadata();

            document.Open();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close(); 

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 72);
            Paragraph p = new Paragraph("Hello World", font);
            p.Role = new PdfName("HW");
            writer.StructureTreeRoot.MapRole(new PdfName("HW"), PdfName.P);

            document.Add(p);

            bool exceptionThrown = false;
            try
            {
                document.Close();
            }
            catch (PdfAConformanceException e)
            {
                if (e.GetObject().ToString().Contains("/Catalog"))
                {
                    exceptionThrown = true;
                }
            }
            if (exceptionThrown)
            {
                Assert.Fail("PdfAConformanceException should be thrown");
            }
        }

        [Test]
        virtual public void FontCheckTest1() {
            bool exceptionThrown = false;
            try {
                Document document = new Document();
                PdfAWriter writer = PdfAWriter.GetInstance(document,
                    new FileStream(OUT + "pdfa2FontCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_1A);
                writer.CreateXmpMetadata();
                writer.SetTagged();
                document.Open();
                document.AddLanguage("en-US");

                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf",
                    BaseFont.WINANSI, false /*BaseFont.EMBEDDED*/, 12);
                document.Add(new Paragraph("Hello World", font));

                FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
                ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close(); 
                
                writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

                document.Close();
            }
            catch (DocumentException docExc) {
                exceptionThrown = true;
            }
            catch (PdfAConformanceException exc) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        public void StamperColorCheckTest() {
            bool exceptionThrown = false;
            try {
                PdfReader reader = new PdfReader(RESOURCES + "pdfa1.pdf");
                PdfAStamper stamper = new PdfAStamper(reader, new MemoryStream(), PdfAConformanceLevel.PDF_A_1A);
                PdfContentByte canvas = stamper.GetOverContent(1);
                Rectangle rect = stamper.Writer.PageSize;
                canvas.SetColorFill(new CMYKColor(0.1f, 0.1f, 0.1f, 0.1f));
                canvas.MoveTo(rect.Left, rect.Bottom);
                canvas.LineTo(rect.Right, rect.Bottom);
                canvas.LineTo(rect.Right, rect.Top);
                canvas.Fill();
                stamper.Close();
                reader.Close();
            } catch (PdfAConformanceException exc) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        public void StamperTextTest() {
            PdfReader reader = new PdfReader(RESOURCES + "pdfa1.pdf");
            PdfAStamper stamper = new PdfAStamper(reader, new FileStream(OUT + "stamperTextTest.pdf", FileMode.Create),
                PdfAConformanceLevel.PDF_A_1A);
            PdfArtifact artifact = new PdfArtifact();
            BaseFont bf = BaseFont.CreateFont(RESOURCES + "FreeMonoBold.ttf",
                BaseFont.WINANSI, BaseFont.EMBEDDED);
            artifact.SetType(PdfArtifact.ArtifactType.LAYOUT);
            PdfContentByte canvas = stamper.GetOverContent(1);
            canvas.OpenMCBlock(artifact);
            canvas.BeginText();
            canvas.SetFontAndSize(bf, 120);
            canvas.ShowTextAligned(Element.ALIGN_CENTER, "TEST", 200, 400, 45);
            canvas.EndText();
            canvas.CloseMCBlock(artifact);

            stamper.Close();
            reader.Close();
        }
    }
}
