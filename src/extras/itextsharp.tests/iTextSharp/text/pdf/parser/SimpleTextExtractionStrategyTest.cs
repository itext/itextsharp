using System;
using System.IO;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class SimpleTextExtractionStrategyTest
    {
        String TEXT1;
        String TEXT2;

        [SetUp]
        virtual public void SetUp()
        {
            TEXT1 = "TEXT1 TEXT1";
            TEXT2 = "TEXT2 TEXT2";
        }

        virtual public ITextExtractionStrategy CreateRenderListenerForTest()
        {
            return new SimpleTextExtractionStrategy();
        }

        [Test]
        virtual public void TestCoLinnearText()
        {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 0);

            Assert.AreEqual(TEXT1 + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        virtual public void TestCoLinnearTextWithSpace()
        {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 2);
            //saveBytesToFile(bytes, new File("c:/temp/test.pdf"));

            Assert.AreEqual(TEXT1 + " " + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        virtual public void TestCoLinnearTextEndingWithSpaceCharacter()
        {
            // in this case, we shouldn't be inserting an extra space
            TEXT1 = TEXT1 + " ";
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 2);

            //TestResourceUtils.openBytesAsPdf(bytes);

            Assert.AreEqual(TEXT1 + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));

        }
        [Test]
        virtual public void TestUnRotatedText()
        {

            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));

        }


        [Test]
        virtual public void TestRotatedText()
        {

            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, -90, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));

        }

        [Test]
        virtual public void TestRotatedText2()
        {

            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 90, true, -20);
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            Assert.AreEqual(TEXT1 + "\n" + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));

        }

        [Test]
        virtual public void TestPartiallyRotatedText()
        {

            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 33, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));

        }

        [Test]
        virtual public void TestWordSpacingCausedByExplicitGlyphPositioning()
        {
            byte[] bytes = CreatePdfWithArrayText(TEXT1, TEXT2, 250);

            Assert.AreEqual(TEXT1 + " " + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }


        [Test]
        virtual public void TestWordSpacingCausedByExplicitGlyphPositioning2()
        {

            byte[] bytes = CreatePdfWithArrayText("[(S)3.2(an)-255.0(D)13.0(i)8.3(e)-10.1(g)1.6(o)-247.5(C)2.4(h)5.8(ap)3.0(t)10.7(er)]TJ");

            Assert.AreEqual("San Diego Chapter", PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }


        [Test]
        virtual public void TestTrailingSpace()
        {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1 + " ", TEXT2, 0, false, 6);

            Assert.AreEqual(TEXT1 + " " + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        virtual public void TestLeadingSpace()
        {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, " " + TEXT2, 0, false, 6);

            Assert.AreEqual(TEXT1 + " " + TEXT2, PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        virtual public void TestExtractXObjectText()
        {
            String text1 = "X";
            byte[] bytes = CreatePdfWithXObject(text1);
            String text = PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest());
            Assert.IsTrue(text.IndexOf(text1) >= 0, "extracted text (" + text + ") must contain '" + text1 + "'");
        }



        byte[] CreatePdfWithXObject(String xobjectText)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();

            doc.Add(new Paragraph("A"));
            doc.Add(new Paragraph("B"));

            PdfTemplate template = writer.DirectContent.CreateTemplate(100, 100);

            template.BeginText();
            template.SetFontAndSize(BaseFont.CreateFont(), 12);
            template.MoveText(5, template.Height - 5);
            template.ShowText(xobjectText);
            template.EndText();

            Image xobjectImage = Image.GetInstance(template);

            doc.Add(xobjectImage);

            doc.Add(new Paragraph("C"));

            doc.Close();

            return baos.ToArray();
        }

        private static byte[] CreatePdfWithArrayText(String directContentTj)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.SetPageSize(PageSize.LETTER);

            document.Open();

            PdfContentByte cb = writer.DirectContent;

            BaseFont font = BaseFont.CreateFont();

            cb.Transform(AffineTransform.GetTranslateInstance(100, 500));
            cb.BeginText();
            cb.SetFontAndSize(font, 12);

            cb.InternalBuffer.Append(directContentTj + "\n");

            cb.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;

        }

        private static byte[] CreatePdfWithArrayText(String text1, String text2, int spaceInPoints)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.SetPageSize(PageSize.LETTER);

            document.Open();

            PdfContentByte cb = writer.DirectContent;

            BaseFont font = BaseFont.CreateFont();


            cb.BeginText();
            cb.SetFontAndSize(font, 12);

            cb.InternalBuffer.Append("[(" + text1 + ")" + (-spaceInPoints) + "(" + text2 + ")]TJ\n");

            cb.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;

        }

        private static byte[] CreatePdfWithRotatedText(String text1, String text2, float rotation, bool moveTextToNextLine, float moveTextDelta)
        {

            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.SetPageSize(PageSize.LETTER);

            document.Open();

            PdfContentByte cb = writer.DirectContent;

            BaseFont font = BaseFont.CreateFont();

            float x = document.PageSize.Width / 2;
            float y = document.PageSize.Height / 2;

            cb.Transform(AffineTransform.GetTranslateInstance(x, y));

            cb.MoveTo(-10, 0);
            cb.LineTo(10, 0);
            cb.MoveTo(0, -10);
            cb.LineTo(0, 10);
            cb.Stroke();

            cb.BeginText();
            cb.SetFontAndSize(font, 12);
            cb.Transform(AffineTransform.GetRotateInstance(rotation / 180f * Math.PI));
            cb.ShowText(text1);
            if (moveTextToNextLine)
                cb.MoveText(0, moveTextDelta);
            else
            {
                cb.Transform(AffineTransform.GetTranslateInstance(moveTextDelta, 0));
            }
            cb.ShowText(text2);
            cb.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
    }
}
