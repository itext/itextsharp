using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class TextMarginFinderTest
    {
        [Test]
        public void TestBasics()
        {
            Rectangle rToDraw = new Rectangle(1.42f * 72f, 2.42f * 72f, 7.42f * 72f, 10.42f * 72f);
            rToDraw.Border = Rectangle.BOX;
            rToDraw.BorderWidth = 1.0f;

            byte[] content = CreatePdf(rToDraw);
            //TestResourceUtils.openBytesAsPdf(content);

            TextMarginFinder finder = new TextMarginFinder();

            new PdfReaderContentParser(new PdfReader(content)).ProcessContent(1, finder);

            Assert.AreEqual(1.42f * 72f, finder.GetLlx(), 0.01f);
            Assert.AreEqual(7.42f * 72f, finder.GetUrx(), 0.01f);
            Assert.AreEqual(2.42f * 72f, finder.GetLly(), 0.01f);
            Assert.AreEqual(10.42f * 72f, finder.GetUry(), 0.01f);

        }

        private byte[] CreatePdf(Rectangle recToDraw)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();


            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();
            float fontsiz = 12;

            float llx = 1.42f * 72f;
            float lly = 2.42f * 72f;
            float urx = 7.42f * 72f;
            float ury = 10.42f * 72f;

            BaseFont font = BaseFont.CreateFont();
            canvas.SetFontAndSize(font, fontsiz);

            float ascent = font.GetFontDescriptor(BaseFont.ASCENT, fontsiz);
            float descent = font.GetFontDescriptor(BaseFont.DESCENT, fontsiz);

            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "LowerLeft", llx, lly - descent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "LowerRight", urx, lly - descent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "UpperLeft", llx, ury - ascent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "UpperRight", urx, ury - ascent, 0.0f);
            canvas.EndText();

            if (recToDraw != null)
            {
                doc.Add(recToDraw);
            }

            doc.Close();

            return baos.ToArray();
        }
    }
}
