using System;
using System.Collections.Generic;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    internal class PdfTextExtractorUnicodeIdentityTest {

        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\PdfTextExtractorUnicodeIdentityTest\";

        [Test]
        virtual public void test() {
            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "user10.pdf");
            Rectangle rectangle = new Rectangle(71, 792 - 84, 225, 792 - 75);
            RenderFilter filter = new RegionTextRenderFilter(rectangle);
            String txt = PdfTextExtractor.GetTextFromPage(reader, 1, new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter));

            Assert.AreEqual("Pname Dname Email Address", txt);
        }
    }
}
