using System;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    internal class GlyphTextRenderListenerTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\GlyphTextRenderListenerTest\";

        [Test]
        public void Test1() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "test.pdf");
            PdfReaderContentParser parser = new PdfReaderContentParser(pdfReader);

            float x1, y1, x2, y2;

            x1 = 203;
            x2 = 224;
            y1 = 842 - 44;
            y2 = 842 - 93;
            String extractedText =
                parser.ProcessContent(1,
                    new GlyphTextRenderListener(new FilteredTextRenderListener(new LocationTextExtractionStrategy(),
                        new RegionTextRenderFilter(new Rectangle(x1, y1, x2, y2))))).GetResultantText();
            Assert.AreEqual("1234\nt5678", extractedText);
        }

        [Test]
        public void Test2() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "Sample.pdf");

            PdfReaderContentParser parser = new PdfReaderContentParser(pdfReader);
            String extractedText =
                parser.ProcessContent(1,
                    new GlyphTextRenderListener(new FilteredTextRenderListener(new LocationTextExtractionStrategy(),
                        new RegionTextRenderFilter(new Rectangle(111, 855, 136, 867))))).GetResultantText();

            Assert.AreEqual("Your ", extractedText);
        }

        [Test]
        public void TestWithMultiFilteredRenderListener() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "test.pdf");
            PdfReaderContentParser parser = new PdfReaderContentParser(pdfReader);

            float x1, y1, x2, y2;

            MultiFilteredRenderListener listener = new MultiFilteredRenderListener();
            x1 = 122;
            x2 = 144;
            y1 = 841.9f - 151;
            y2 = 841.9f - 163;
            ITextExtractionStrategy region1Listener = listener.AttachRenderListener(
                new LocationTextExtractionStrategy(), new RegionTextRenderFilter(new Rectangle(x1, y1, x2, y2)));

            x1 = 156;
            x2 = 169;
            y1 = 841.9f - 151;
            y2 = 841.9f - 163;
            ITextExtractionStrategy region2Listener = listener.AttachRenderListener(
                new LocationTextExtractionStrategy(), new RegionTextRenderFilter(new Rectangle(x1, y1, x2, y2)));

            parser.ProcessContent(1, new GlyphRenderListener(listener));
            Assert.AreEqual("Your", region1Listener.GetResultantText());
            Assert.AreEqual("dju", region2Listener.GetResultantText());
        }
    }
}