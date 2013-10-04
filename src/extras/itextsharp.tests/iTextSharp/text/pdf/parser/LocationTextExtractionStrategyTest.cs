using System;
using System.IO;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class LocationTextExtractionStrategyTest : SimpleTextExtractionStrategyTest
    {
        public new ITextExtractionStrategy CreateRenderListenerForTest()
        {
            return new LocationTextExtractionStrategy();
        }

        [Test]
        public void TestYPosition()
        {
            PdfReader r = CreatePdfWithOverlappingTextVertical(new String[] { "A", "B", "C", "D" }, new String[] { "AA", "BB", "CC", "DD" });

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nAA\nB\nBB\nC\nCC\nD\nDD", text);
        }

        [Test]
        public void TestXPosition()
        {
            byte[] content = CreatePdfWithOverlappingTextHorizontal(new String[] { "A", "B", "C", "D" }, new String[] { "AA", "BB", "CC", "DD" });
            PdfReader r = new PdfReader(content);

            //TestResourceUtils.openBytesAsPdf(content);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A AA B BB C CC D DD", text);
            //        Assert.AreEqual("A\tAA\tB\tBB\tC\tCC\tD\tDD", text);
        }

        [Test]
        public void TestRotatedPage()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate(), "A\nB\nC\nD");

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        public void TestRotatedPage2()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate(), "A\nB\nC\nD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        public void TestRotatedPage3()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate().Rotate(), "A\nB\nC\nD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        public void TestExtractXObjectTextWithRotation()
        {
            //LocationAwareTextExtractingPdfContentRenderListener.DUMP_STATE = true;
            String text1 = "X";
            byte[] content = CreatePdfWithRotatedXObject(text1);
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));
            PdfReader r = new PdfReader(content);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("A\nB\nX\nC", text);
        }

        [Test]
        public void TestNegativeCharacterSpacing()
        {
            byte[] content = CreatePdfWithNegativeCharSpacing("W", 200, "A");
            //TestResourceUtils.openBytesAsPdf(content);
            PdfReader r = new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("WA", text);
        }

        [Test]
        public void TestSanityCheckOnVectorMath()
        {
            Vector start = new Vector(0, 0, 1);
            Vector end = new Vector(1, 0, 1);
            Vector antiparallelStart = new Vector(0.9f, 0, 1);
            Vector parallelStart = new Vector(1.1f, 0, 1);

            float rsltAntiParallel = antiparallelStart.Subtract(end).Dot(end.Subtract(start).Normalize());
            Assert.AreEqual(-0.1f, rsltAntiParallel, 0.0001);

            float rsltParallel = parallelStart.Subtract(end).Dot(end.Subtract(start).Normalize());
            Assert.AreEqual(0.1f, rsltParallel, 0.0001);

        }

        [Test]
        public void TestSuperscript()
        {
            byte[] content = createPdfWithSupescript("Hel", "lo");
            //TestResourceUtils.openBytesAsPdf(content);
            PdfReader r = new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("Hello", text);


        }

        private byte[] CreatePdfWithNegativeCharSpacing(String str1, float charSpacing, String str2)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();

            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12);
            canvas.MoveText(45, doc.PageSize.Height - 45);
            PdfTextArray ta = new PdfTextArray();
            ta.Add(str1);
            ta.Add(charSpacing);
            ta.Add(str2);
            canvas.ShowText(ta);
            canvas.EndText();

            doc.Close();

            return baos.ToArray();
        }

        private byte[] CreatePdfWithRotatedXObject(String xobjectText)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();

            doc.Add(new Paragraph("A"));
            doc.Add(new Paragraph("B"));

            bool rotate = true;

            PdfTemplate template = writer.DirectContent.CreateTemplate(20, 100);
            template.SetColorStroke(BaseColor.GREEN);
            template.Rectangle(0, 0, template.Width, template.Height);
            template.Stroke();
            AffineTransform tx = new AffineTransform();
            if(rotate) {
                tx.Translate(0, template.Height);
                tx.rotate(-90 / 180f * Math.PI);
            }
            template.Transform(tx);
            template.BeginText();
            template.SetFontAndSize(BaseFont.CreateFont(), 12);
            if (rotate)
                template.MoveText(0, template.Width - 12);
            else
                template.MoveText(0, template.Height - 12);
            template.ShowText(xobjectText);

            template.EndText();

            Image xobjectImage = Image.GetInstance(template);
            if (rotate)
                xobjectImage.RotationDegrees = 90;
            doc.Add(xobjectImage);

            doc.Add(new Paragraph("C"));

            doc.Close();

            return baos.ToArray();
        }

        private byte[] CreateSimplePdf(Rectangle pageSize, params string[] text)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(pageSize);
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            foreach (String str in text)
            {
                document.Add(new Paragraph(str));
                document.NewPage();
            }

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }

        protected byte[] CreatePdfWithOverlappingTextHorizontal(String[] text1, String[] text2)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();

            PdfContentByte canvas = writer.DirectContent;
            float ystart = 500;
            float xstart = 50;

            canvas.BeginText();
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12);
            float x = xstart;
            float y = ystart;
            foreach (string text in text1)
            {
                canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
                x += 70.0f;
            }

            x = xstart + 12;
            y = ystart;
            foreach (string text in text2)
            {
                canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
                x += 70.0f;
            }
            canvas.EndText();

            doc.Close();


            return baos.ToArray();

        }

        private PdfReader CreatePdfWithOverlappingTextVertical(String[] text1, String[] text2)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();

            PdfContentByte canvas = writer.DirectContent;
            float ystart = 500;

            canvas.BeginText();
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12);
            float x = 50;
            float y = ystart;
            foreach (String text in text1)
            {
                canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
                y -= 25.0f;
            }

            y = ystart - 13;
            foreach (String text in text2)
            {
                canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
                y -= 25.0f;
            }
            canvas.EndText();

            doc.Close();

            return new PdfReader(baos.ToArray());

        }

        private byte[] createPdfWithSupescript(String regularText, String superscriptText)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            document.Add(new Chunk(regularText));
            Chunk c2 = new Chunk(superscriptText);
            c2.SetTextRise(7.0f);
            document.Add(c2);

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
    }
}
