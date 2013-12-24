using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class DocumentFontTest
    {
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\DocumentFontTest\";

        [SetUp]
        virtual public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        virtual public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        virtual public void TestConstructionForType0WithoutToUnicodeMap()
        {
            int pageNum = 2;
            PdfName fontIdName = new PdfName("TT9");

            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "type0FontWithoutToUnicodeMap.pdf");
            RandomAccessFileOrArray f = new RandomAccessFileOrArray(testFile);
            PdfReader reader = new PdfReader(f, null);

            try
            {
                PdfDictionary fontsDic = reader.GetPageN(pageNum).GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.FONT);
                PdfDictionary fontDicDirect = fontsDic.GetAsDict(fontIdName);
                PRIndirectReference fontDicIndirect = (PRIndirectReference)fontsDic.Get(fontIdName);

                Assert.AreEqual(PdfName.TYPE0, fontDicDirect.GetAsName(PdfName.SUBTYPE));
                Assert.AreEqual("/Identity-H", fontDicDirect.GetAsName(PdfName.ENCODING).ToString());
                Assert.IsNull(fontDicDirect.Get(PdfName.TOUNICODE), "This font should not have a ToUnicode map");

                new DocumentFont(fontDicIndirect); // this used to throw an NPE
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
