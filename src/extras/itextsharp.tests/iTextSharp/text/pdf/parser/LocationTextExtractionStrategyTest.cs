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
        virtual public void TestYPosition()
        {
            PdfReader r = CreatePdfWithOverlappingTextVertical(new String[] { "A", "B", "C", "D" }, new String[] { "AA", "BB", "CC", "DD" });

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nAA\nB\nBB\nC\nCC\nD\nDD", text);
        }

        [Test]
        virtual public void TestXPosition()
        {
            byte[] content = CreatePdfWithOverlappingTextHorizontal(new String[] { "A", "B", "C", "D" }, new String[] { "AA", "BB", "CC", "DD" });
            PdfReader r = new PdfReader(content);

            //TestResourceUtils.openBytesAsPdf(content);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A AA B BB C CC D DD", text);
            //        Assert.AreEqual("A\tAA\tB\tBB\tC\tCC\tD\tDD", text);
        }

        [Test]
        virtual public void TestRotatedPage()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate(), "A\nB\nC\nD");

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        virtual public void TestRotatedPage2()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate(), "A\nB\nC\nD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        virtual public void TestRotatedPage3()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate().Rotate(), "A\nB\nC\nD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());

            Assert.AreEqual("A\nB\nC\nD", text);
        }

        [Test]
        virtual public void TestExtractXObjectTextWithRotation()
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
        virtual public void TestNegativeCharacterSpacing()
        {
            byte[] content = CreatePdfWithNegativeCharSpacing("W", 200, "A");
            //TestResourceUtils.openBytesAsPdf(content);
            PdfReader r = new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("WA", text);
        }

        [Test]
        virtual public void TestSanityCheckOnVectorMath()
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
        virtual public void TestSuperscript()
        {
            byte[] content = createPdfWithSupescript("Hel", "lo");
            //TestResourceUtils.openBytesAsPdf(content);
            PdfReader r = new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("Hello", text);


        }
        
        [Test]
        public void TestFontSpacingEqualsCharSpacing()
        {
            byte[] content = CreatePdfWithFontSpacingEqualsCharSpacing();
            PdfReader r= new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("Preface", text);
        }

        [Test]
        public void TestLittleFontSize()
        {
            byte[] content = CreatePdfWithLittleFontSize();
            PdfReader r= new PdfReader(content);
            String text = PdfTextExtractor.GetTextFromPage(r, 1, CreateRenderListenerForTest());
            Assert.AreEqual("Preface Preface ", text);
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
                tx.Rotate(-90 / 180f * Math.PI);
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

        virtual protected byte[] CreatePdfWithOverlappingTextHorizontal(String[] text1, String[] text2)
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
        
        private byte[] CreatePdfWithFontSpacingEqualsCharSpacing()
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            doc.Open();

            BaseFont font = BaseFont.CreateFont();
            int fontSize = 12;
            float charSpace = font.GetWidth(' ') / 1000.0f;

            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();
            canvas.SetFontAndSize(font, fontSize);
            canvas.MoveText(45, doc.PageSize.Height - 45);
            canvas.SetCharacterSpacing(-charSpace * fontSize);

            PdfTextArray textArray = new PdfTextArray();
            textArray.Add("P");
            textArray.Add(-226.2f);
            textArray.Add("r");
            textArray.Add(-231.8f);
            textArray.Add("e");
            textArray.Add(-230.8f);
            textArray.Add("f");
            textArray.Add(-238);
            textArray.Add("a");
            textArray.Add(-238.9f);
            textArray.Add("c");
            textArray.Add(-228.9f);
            textArray.Add("e");

            canvas.ShowText(textArray);
            canvas.EndText();

            doc.Close();
        
            byte[] pdfBytes = baos.ToArray();
        
            return pdfBytes;
        }

        private byte[] CreatePdfWithLittleFontSize()
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            doc.Open();

            BaseFont font = BaseFont.CreateFont();
            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();
            canvas.SetFontAndSize(font, 0.2f);
            canvas.MoveText(45, doc.PageSize.Height - 45);

            PdfTextArray textArray = new PdfTextArray();
            textArray.Add("P");
            textArray.Add("r");
            textArray.Add("e");
            textArray.Add("f");
            textArray.Add("a");
            textArray.Add("c");
            textArray.Add("e");
            textArray.Add(" ");

            canvas.ShowText(textArray);
            canvas.SetFontAndSize(font, 10);
            canvas.ShowText(textArray);

            canvas.EndText();

            doc.Close();

            byte[] pdfBytes = baos.ToArray();
        
            return pdfBytes;
        }
    }
}
