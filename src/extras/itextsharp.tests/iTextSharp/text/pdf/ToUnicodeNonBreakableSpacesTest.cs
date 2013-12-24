using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class ToUnicodeNonBreakableSpacesTest
    {
        private BaseFont fontWithToUnicode;
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\ToUnicodeNonBreakableSpacesTest\";
        private const string TARGET = @"ToUnicodeNonBreakableSpacesTest\";

        [SetUp]
        virtual public void SetUp()
        {
            Directory.CreateDirectory(TARGET);

            PdfReader reader = new PdfReader(
                TestResourceUtils.GetResourceAsStream(TEST_RESOURCES_PATH, "fontWithToUnicode.pdf"));
            PdfDictionary resourcesDict = reader.GetPageResources(1);
            PdfDictionary fontsDict = resourcesDict.GetAsDict(PdfName.FONT);
            foreach (PdfName key in fontsDict.Keys)
            {
                PdfObject pdfFont = fontsDict.Get(key);

                if (pdfFont is PRIndirectReference)
                {
                    fontWithToUnicode = BaseFont.CreateFont((PRIndirectReference)pdfFont);
                    break;
                }
            }
        }

        [Test]
        virtual public void WriteTextWithWordSpacing()
        {
            Document document = new Document();
            FileStream output = new FileStream(TARGET + "textWithWorldSpacing.pdf", FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(document, output);
            document.Open();
            document.SetPageSize(PageSize.A4);
            document.NewPage();
            writer.DirectContent.SetWordSpacing(10);
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(new Rectangle(30, 0, document.PageSize.Right, document.PageSize.Top - 30));
            columnText.UseAscender = true;
            columnText.AddText(new Chunk("H H H H H H H H H  !", new Font(fontWithToUnicode, 30)));
            columnText.Go();
            document.Close();
        }
    }
}
