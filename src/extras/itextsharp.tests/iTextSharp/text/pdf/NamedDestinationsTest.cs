using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class NamedDestinationsTest {

        private String srcFolder = @"..\..\resources\text\pdf\NamedDestinationsTest\";
        private String outFolder = "com/itextpdf/test/pdf/NamedDestinationsTest/";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(outFolder);
        }

    [Test]
    public void AddNavigationTest()  {
        String src = srcFolder + "primes.pdf";
        String dest = outFolder + "primes_links.pdf";
        PdfReader reader = new PdfReader(src);
        PdfStamper stamper = new PdfStamper(reader, new FileStream(dest, FileMode.Create));
        PdfDestination d = new PdfDestination(PdfDestination.FIT);
        Rectangle rect = new Rectangle(0, 806, 595, 842);
        PdfAnnotation a10 = PdfAnnotation.CreateLink(stamper.Writer, rect, PdfAnnotation.HIGHLIGHT_INVERT, 2, d);
        stamper.AddAnnotation(a10, 1);
        PdfAnnotation a1 = PdfAnnotation.CreateLink(stamper.Writer, rect, PdfAnnotation.HIGHLIGHT_PUSH, 1, d);
        stamper.AddAnnotation(a1, 2);
        stamper.Close();
        CompareTool compareTool = new CompareTool();
        String errorMessage = compareTool.CompareByContent(dest, srcFolder + "cmp_primes_links.pdf", outFolder, "diff_");
        if (errorMessage != null) {
            Assert.Fail(errorMessage);
        }
    }

    }
}
