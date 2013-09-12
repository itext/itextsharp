using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf.fonts.cmaps;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts.cmaps
{
    class CMapParserExTest
    {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\fonts\cmaps\CMapParserExTest\";

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestCMapWithDefDictionaryKey()
        {
            byte[] touni = TestResourceUtils.GetResourceAsByteArray(TEST_RESOURCES_PATH, "cmap_with_def_dictionary_key.txt");
            CidLocationFromByte lb = new CidLocationFromByte(touni);
            CMapToUnicode cmapRet = new CMapToUnicode();
            CMapParserEx.ParseCid("", cmapRet, lb);
        }
    }
}
