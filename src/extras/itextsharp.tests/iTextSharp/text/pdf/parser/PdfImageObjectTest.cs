using System;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class PdfImageObjectTest
    {
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\PdfImageObjectTest\";

        private void TestFile(String filename, int page, String objectid)
        {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, filename);
            try
            {
                PdfDictionary resources = pdfReader.GetPageResources(page);
                PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
                PdfIndirectReference objRef = xobjects.GetAsIndirectObject(new PdfName(objectid));
                if (objRef == null)
                    throw new NullReferenceException("Reference " + objectid + " not found - Available keys are " + xobjects.Keys);
                PRStream stream = (PRStream)PdfReader.GetPdfObject(objRef);
                PdfDictionary colorSpaceDic = resources != null ? resources.GetAsDict(PdfName.COLORSPACE) : null;
                PdfImageObject img = new PdfImageObject(stream, colorSpaceDic);
                byte[] result = img.GetImageAsBytes();
                Assert.NotNull(result);
                int zeroCount = 0;
                foreach (byte b in result)
                {
                    if (b == 0) zeroCount++;
                }
                Assert.IsTrue(zeroCount > 0);
            }
            finally
            {
                pdfReader.Close();
            }
        }

        [Test]
        virtual public void TestMultiStageFilters()
        {
            TestFile("multistagefilter1.pdf", 1, "Obj13");
        }

        [Test]
        virtual public void TestAscii85Filters()
        {
            TestFile("ASCII85_RunLengthDecode.pdf", 1, "Im9");
        }

        [Test]
        virtual public void TestCcittFilters()
        {
            TestFile("ccittfaxdecode.pdf", 1, "background0");
        }

        [Test]
        virtual public void TestFlateDecodeFilters()
        {
            TestFile("flatedecode_runlengthdecode.pdf", 1, "Im9");
        }

        [Test]
        virtual public void TestDctDecodeFilters()
        {
            TestFile("dctdecode.pdf", 1, "im1");
        }

        [Test]
        virtual public void Testjbig2Filters()
        {
            TestFile("jbig2decode.pdf", 1, "2");
        }
    }
}
