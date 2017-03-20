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
using iTextSharp.awt.geom;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    public class SimpleTextExtractionStrategyTest {
        private String TEXT1;
        private String TEXT2;

        protected const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\SimpleTextExtractionStrategyTest\";

        [SetUp]
        public virtual void SetUp() {
            TEXT1 = "TEXT1 TEXT1";
            TEXT2 = "TEXT2 TEXT2";
        }

        public virtual ITextExtractionStrategy CreateRenderListenerForTest() {
            return new SimpleTextExtractionStrategy();
        }

        [Test]
        public virtual void TestCoLinnearText() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 0);

            Assert.AreEqual(TEXT1 + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestCoLinnearTextWithSpace() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 2);
            //saveBytesToFile(bytes, new File("c:/temp/test.pdf"));

            Assert.AreEqual(TEXT1 + " " + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestCoLinnearTextEndingWithSpaceCharacter() {
            // in this case, we shouldn't be inserting an extra space
            TEXT1 = TEXT1 + " ";
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, false, 2);

            //TestResourceUtils.openBytesAsPdf(bytes);

            Assert.AreEqual(TEXT1 + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestUnRotatedText() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 0, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }


        [Test]
        public virtual void TestRotatedText() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, -90, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestRotatedText2() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 90, true, -20);
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            Assert.AreEqual(TEXT1 + "\n" + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestPartiallyRotatedText() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, TEXT2, 33, true, -20);

            Assert.AreEqual(TEXT1 + "\n" + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestWordSpacingCausedByExplicitGlyphPositioning() {
            byte[] bytes = CreatePdfWithArrayText(TEXT1, TEXT2, 250);

            Assert.AreEqual(TEXT1 + " " + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }


        [Test]
        public virtual void TestWordSpacingCausedByExplicitGlyphPositioning2() {
            byte[] bytes =
                CreatePdfWithArrayText("[(S)3.2(an)-255.0(D)13.0(i)8.3(e)-10.1(g)1.6(o)-247.5(C)2.4(h)5.8(ap)3.0(t)10.7(er)]TJ");

            Assert.AreEqual("San Diego Chapter",
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }


        [Test]
        public virtual void TestTrailingSpace() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1 + " ", TEXT2, 0, false, 6);

            Assert.AreEqual(TEXT1 + " " + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestLeadingSpace() {
            byte[] bytes = CreatePdfWithRotatedText(TEXT1, " " + TEXT2, 0, false, 6);

            Assert.AreEqual(TEXT1 + " " + TEXT2,
                PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest()));
        }

        [Test]
        public virtual void TestExtractXObjectText() {
            String text1 = "X";
            byte[] bytes = CreatePdfWithXObject(text1);
            String text = PdfTextExtractor.GetTextFromPage(new PdfReader(bytes), 1, CreateRenderListenerForTest());
            Assert.IsTrue(text.IndexOf(text1) >= 0, "extracted text (" + text + ") must contain '" + text1 + "'");
        }

        [Test]
        public virtual void ExtractFromPage229() {
            if (this.GetType() != typeof (SimpleTextExtractionStrategyTest))
                return;
            FileStream @is = TestResourceUtils.GetResourceAsStream(TEST_RESOURCES_PATH, "page229.pdf");
            PdfReader reader = new PdfReader(@is);
            String text1 = PdfTextExtractor.GetTextFromPage(reader, 1, new SimpleTextExtractionStrategy());
            String text2 = PdfTextExtractor.GetTextFromPage(reader, 1, new SingleCharacterSimpleTextExtractionStrategy());
            Assert.AreEqual(text1, text2);
            reader.Close();
        }

        [Test]
        public virtual void ExtractFromIsoTc171() {
            if (this.GetType() != typeof (SimpleTextExtractionStrategyTest))
                return;
            FileStream @is = TestResourceUtils.GetResourceAsStream(TEST_RESOURCES_PATH,
                "ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda.pdf");
            PdfReader reader = new PdfReader(@is);
            String text1 = PdfTextExtractor.GetTextFromPage(reader, 1, new SimpleTextExtractionStrategy()) +
                           "\n" +
                           PdfTextExtractor.GetTextFromPage(reader, 2, new SimpleTextExtractionStrategy());
            String text2 = PdfTextExtractor.GetTextFromPage(reader, 1, new SingleCharacterSimpleTextExtractionStrategy()) +
                           "\n" +
                           PdfTextExtractor.GetTextFromPage(reader, 2, new SingleCharacterSimpleTextExtractionStrategy());
            Assert.AreEqual(text1, text2);
            reader.Close();
        }

        private byte[] CreatePdfWithXObject(String xobjectText) {
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

        private static byte[] CreatePdfWithArrayText(String directContentTj) {
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

        private static byte[] CreatePdfWithArrayText(String text1, String text2, int spaceInPoints) {
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

        private static byte[] CreatePdfWithRotatedText(String text1, String text2, float rotation, bool moveTextToNextLine,
            float moveTextDelta) {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.SetPageSize(PageSize.LETTER);

            document.Open();

            PdfContentByte cb = writer.DirectContent;

            BaseFont font = BaseFont.CreateFont();

            float x = document.PageSize.Width/2;
            float y = document.PageSize.Height/2;

            cb.Transform(AffineTransform.GetTranslateInstance(x, y));

            cb.MoveTo(-10, 0);
            cb.LineTo(10, 0);
            cb.MoveTo(0, -10);
            cb.LineTo(0, 10);
            cb.Stroke();

            cb.BeginText();
            cb.SetFontAndSize(font, 12);
            cb.Transform(AffineTransform.GetRotateInstance(rotation/180f*Math.PI));
            cb.ShowText(text1);
            if (moveTextToNextLine)
                cb.MoveText(0, moveTextDelta);
            else {
                cb.Transform(AffineTransform.GetTranslateInstance(moveTextDelta, 0));
            }
            cb.ShowText(text2);
            cb.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }

        private class SingleCharacterSimpleTextExtractionStrategy : SimpleTextExtractionStrategy {
            public override void RenderText(TextRenderInfo renderInfo) {
                foreach (TextRenderInfo tri in renderInfo.GetCharacterRenderInfos())
                    base.RenderText(tri);
            }
        }
    }
}
