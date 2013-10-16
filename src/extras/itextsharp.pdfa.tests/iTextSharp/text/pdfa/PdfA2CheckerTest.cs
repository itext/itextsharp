using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdfa
{
    [TestFixture]
    public class PdfA2CheckerTest
    {
        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        public const String TARGET = "PdfA2CheckerTest\\";
        public const String OUT = TARGET + "pdf\\out";


        [SetUp]
        public void Initialize()
        {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
            Document.Compress = false;
        }

        [Test]
        public void TransparencyCheckTest()
        {
            string filename = OUT + "TransparencyCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            document.Open();

            PdfContentByte canvas = writer.DirectContent;

            canvas.SaveState();
            PdfGState gs = new PdfGState();
            gs.BlendMode = PdfGState.BM_DARKEN;
            canvas.SetGState(gs);
            canvas.Rectangle(100, 100, 100, 100);
            canvas.Fill();
            canvas.RestoreState();

            canvas.SaveState();
            gs = new PdfGState();
            gs.BlendMode = new PdfName("Lighten");
            canvas.SetGState(gs);
            canvas.Rectangle(200, 200, 100, 100);
            canvas.Fill();
            canvas.RestoreState();

            bool exception = false;
            canvas.SaveState();
            gs = new PdfGState();
            gs.BlendMode = new PdfName("UnknownBM");
            canvas.SetGState(gs);
            canvas.Rectangle(300, 300, 100, 100);
            canvas.Fill();
            canvas.RestoreState();
            try {
                document.Close();
            }
            catch (PdfAConformanceException) {
                exception = true;
            }
            if (!exception)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");

        }

        [Test, Ignore]
        public void ImageCheckTest1()
        {
            string filename = OUT + "ImageCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2A);
            document.Open();

            String[] pdfaErrors = new String[9];
            for (int i = 1; i <= 9; i++)
            {
                try
                {
                    Image img = Image.GetInstance(String.Format("{0}jpeg2000\\file{1}.jp2", RESOURCES, i));
                    document.Add(img);
                    document.NewPage();
                }
                catch (Exception e)
                {
                    pdfaErrors[i - 1] = e.Message;
                }
            }

            Assert.AreEqual(null, pdfaErrors[0]);
            Assert.AreEqual(null, pdfaErrors[1]);
            Assert.AreEqual(null, pdfaErrors[2]);
            Assert.AreEqual(null, pdfaErrors[3]);
            Assert.AreEqual(true, pdfaErrors[4].Contains("0x01"));
            Assert.AreEqual(null, pdfaErrors[5]);
            Assert.AreEqual(true, pdfaErrors[6].Contains("0x01"));
            Assert.AreEqual(null, pdfaErrors[7]);
            Assert.AreEqual(null, pdfaErrors[8]);

            document.Close();
        }

        [Test, Ignore]
        public void ImageCheckTest2()
        {
            string filename = OUT + "ImageCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2A);
            document.Open();

            List<String> pdfaErrors = new List<String>();
            try
            {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p0_01.j2k");
                document.Add(img);
                document.NewPage();
            }
            catch (Exception e)
            {
                pdfaErrors.Add(e.Message);
            }

            try
            {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p0_02.j2k");
                document.Add(img);
            }
            catch (Exception e)
            {
                pdfaErrors.Add(e.Message);
            }

            try
            {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p1_01.j2k");
                document.Add(img);
            }
            catch (Exception e)
            {
                pdfaErrors.Add(e.Message);
            }

            try
            {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p1_02.j2k");
                document.Add(img);
            }
            catch (Exception e)
            {
                pdfaErrors.Add(e.Message);
            }

            Assert.Equals(4, pdfaErrors.Count);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(true, pdfaErrors[i].Contains("JPX"));
            }

            document.Close();
        }

        [Test]
        public void LayerCheckTest1()
        {
            string filename = OUT + "LayerCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.ViewerPreferences = PdfWriter.PageModeUseOC;
            writer.PdfVersion = PdfWriter.VERSION_1_5;
            document.Open();
            PdfLayer layer = new PdfLayer("Do you see me?", writer);
            layer.On = true;
            BaseFont bf = BaseFont.CreateFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, true);
            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(bf, 18);
            cb.ShowTextAligned(Element.ALIGN_LEFT, "Do you see me?", 50, 790, 0);
            cb.BeginLayer(layer);
            cb.ShowTextAligned(Element.ALIGN_LEFT, "Peek-a-Boo!!!", 50, 766, 0);
            cb.EndLayer();
            cb.EndText();
            document.Close();
        }

        [Test]
        public void LayerCheckTest2()
        {
            string filename = OUT + "LayerCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.ViewerPreferences = PdfWriter.PageModeUseOC;
            writer.PdfVersion = PdfWriter.VERSION_1_5;
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfLayer nested = new PdfLayer("Nested layers", writer);
            PdfLayer nested_1 = new PdfLayer("Nested layer 1", writer);
            PdfLayer nested_2 = new PdfLayer("Nested layer 2", writer);
            nested.AddChild(nested_1);
            nested.AddChild(nested_2);
            writer.LockLayer(nested_2);
            cb.BeginLayer(nested);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, true);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layers", font), 50, 775, 0);
            cb.EndLayer();
            cb.BeginLayer(nested_1);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layer 1", font), 100, 800, 0);
            cb.EndLayer();
            cb.BeginLayer(nested_2);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layer 2", font), 100, 750, 0);
            cb.EndLayer();

            document.Close();
        }

        [Test]
        public void EgsCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "EgsCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR, new PdfName("Test"));
            gs.Put(PdfName.HTP, new PdfName("Test"));
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == gs) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public void EgsCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "EgsCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            PdfDictionary dict = new PdfDictionary();
            dict.Put(PdfName.HALFTONETYPE, new PdfNumber(6));
            gs.Put(PdfName.HT, dict);
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public void EgsCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "EgsCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            PdfDictionary dict = new PdfDictionary();
            dict.Put(PdfName.HALFTONETYPE, new PdfNumber(5));
            dict.Put(PdfName.HALFTONENAME, new PdfName("Test"));
            gs.Put(PdfName.HT, dict);
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }
    }
}