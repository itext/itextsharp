using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class FilteredTextRenderListenerTest
    {

        [Test]
        virtual public void TestRegion()
        {
            byte[] pdf = CreatePdfWithCornerText();

            PdfReader reader = new PdfReader(pdf);
            float pageHeight = reader.GetPageSize(1).Height;
            Rectangle upperLeft = new Rectangle(0, (int)pageHeight - 30, 250, (int)pageHeight);

            Assert.IsTrue(TextIsInRectangle(reader, "Upper Left", upperLeft));
            Assert.IsFalse(TextIsInRectangle(reader, "Upper Right", upperLeft));
        }

        private bool TextIsInRectangle(PdfReader reader, String text, Rectangle rect)
        {

            FilteredTextRenderListener filterListener = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), new RegionTextRenderFilter(rect));

            String extractedText = PdfTextExtractor.GetTextFromPage(reader, 1, filterListener);

            return extractedText.Equals(text);
        }

        private byte[] CreatePdfWithCornerText()
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(PageSize.LETTER);
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.Open();

            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();

            canvas.SetFontAndSize(BaseFont.CreateFont(), 12);

            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Upper Left", 10, document.PageSize.Height - 10 - 12, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Upper Right", document.PageSize.Width - 10, document.PageSize.Height - 10 - 12, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Lower Left", 10, 10, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Lower Right", document.PageSize.Width - 10, 10, 0);

            canvas.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;

        }
    }
}
