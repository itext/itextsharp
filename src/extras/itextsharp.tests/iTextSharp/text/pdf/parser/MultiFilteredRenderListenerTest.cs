using System;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    internal class MultiFilteredRenderListenerTest {
        private string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\MultiFilteredRenderListenerTest\";

        [Test]
        public void Test() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "test.pdf");

            String[] expectedText = new String[] {
                "PostScript Compatibility",
                "Because the PostScript language does not support the transparent imaging \n" +
                "model, PDF 1.4 consumer applications must have some means for converting the \n" +
                "appearance of a document that uses transparency to a purely opaque description \n" +
                "for printing on PostScript output devices. Similar techniques can also be used to \n" +
                "convert such documents to a form that can be correctly viewed by PDF 1.3 and \n" +
                "earlier consumers. ",
                "Otherwise, flatten the colors to some assumed device color space with pre-\n" +
                "determined calibration. In the generated PostScript output, paint the flattened \n" +
                "colors in a CIE-based color space having that calibration. "
            };

            Rectangle[] regions = new Rectangle[] {
                new Rectangle(90, 605, 220, 581),
                new Rectangle(80, 578, 450, 486), new Rectangle(103, 196, 460, 143)
            };

            RegionTextRenderFilter[] regionFilters = new RegionTextRenderFilter[regions.Length];
            for (int i = 0; i < regions.Length; i++)
                regionFilters[i] = new RegionTextRenderFilter(regions[i]);


            MultiFilteredRenderListener listener = new MultiFilteredRenderListener();
            LocationTextExtractionStrategy[] extractionStrategies = new LocationTextExtractionStrategy[regions.Length];
            for (int i = 0; i < regions.Length; i++)
                extractionStrategies[i] =
                    (LocationTextExtractionStrategy)
                        listener.AttachRenderListener(new LocationTextExtractionStrategy(), regionFilters[i]);

            new PdfReaderContentParser(pdfReader).ProcessContent(1, listener);

            for (int i = 0; i < regions.Length; i++) {
                String actualText = extractionStrategies[i].GetResultantText();
                Assert.AreEqual(expectedText[i], actualText);
            }
        }

        [Test]
        public void MultipleFiltersForOneRegionTest() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "test.pdf");

            Rectangle[] regions = new Rectangle[] {
                new Rectangle(0, 0, 500, 650),
                new Rectangle(0, 0, 400, 400), new Rectangle(200, 200, 500, 600), new Rectangle(100, 100, 450, 400)
            };

            RegionTextRenderFilter[] regionFilters = new RegionTextRenderFilter[regions.Length];
            for (int i = 0; i < regions.Length; i++)
                regionFilters[i] = new RegionTextRenderFilter(regions[i]);

            MultiFilteredRenderListener listener = new MultiFilteredRenderListener();
            LocationTextExtractionStrategy extractionStrategy =
                (LocationTextExtractionStrategy)
                    listener.AttachRenderListener(new LocationTextExtractionStrategy(), regionFilters);
            new PdfReaderContentParser(pdfReader).ProcessContent(1, listener);
            String actualText = extractionStrategy.GetResultantText();

            String expectedText = PdfTextExtractor.GetTextFromPage(pdfReader, 1,
                new FilteredTextRenderListener(new LocationTextExtractionStrategy(), regionFilters));

            Assert.AreEqual(expectedText, actualText);
        }
    }
}