using System;
using System.Collections.Generic;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class CMapAwareDocumentFontTest
    {
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\CMapAwareDocumentFontTest\";

        [Test]
        public void TestWidths()
        {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "fontwithwidthissue.pdf");

            try
            {
                PdfDictionary fontsDic = pdfReader.GetPageN(1).GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.FONT);
                PRIndirectReference fontDicIndirect = (PRIndirectReference)fontsDic.Get(new PdfName("F1"));

                CMapAwareDocumentFont f = new CMapAwareDocumentFont(fontDicIndirect);
                Assert.IsTrue(f.GetWidth('h') != 0, "Width should not be 0");
            }
            finally
            {
                pdfReader.Close();
            }
        }

    }
}
