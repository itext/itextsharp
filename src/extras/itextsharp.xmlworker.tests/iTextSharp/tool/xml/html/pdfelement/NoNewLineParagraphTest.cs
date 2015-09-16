using iTextSharp.tool.xml.html.pdfelement;
using System;
using iTextSharp.text;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.pdfelement {
    public class NoNewLineParagraphTest {
        private static String IMAGE = @"..\..\resources\images.jpg";
        private NoNewLineParagraph paragraph;
        private IElement jpegImage;

        [SetUp]
        public void Setup() {
            jpegImage = Image.GetInstance(IMAGE);
            paragraph = new NoNewLineParagraph();
        }

        /**
         * Test method for
         * {@link com.itextpdf.tool.xml.html.pdfelement.NoNewLineParagraph#add(com.itextpdf.text.Element)}.
         */
        [Test]
        public void TestAddImageToParagraph() {
            String message = "Could not add " + jpegImage.GetType().Name
                             + " to " + paragraph.GetType().Name;
            Assert.IsTrue(paragraph.Add(jpegImage), message);
        }
    }
}
