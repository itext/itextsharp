using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class PdfTextExtractorEncodingsTest
    {
        /** Basic Latin characters, with Unicode values less than 128 */
        private static String TEXT1 = "AZaz09*!";
        /** Latin-1 characters */
        private static String TEXT2 = "\u0027\u0060\u00a4\u00a6";

        // the following will cause failures
        //  private static  String TEXT2 = "\u0027\u0060\u00a4\u00a6\00b5\u2019";


        [TestFixtureSetUp]
        public static void InitializeFontFactory()
        {
            FontFactory.RegisterDirectories();
        }

        protected static Font GetSomeTTFont(String encoding, bool embedded, float size)
        {

            string[] fontNames = { "arial" };
            foreach (string name in fontNames)
            {
                Font foundFont = FontFactory.GetFont(name, encoding, embedded, size);
                if (foundFont != null)
                {
                    switch (foundFont.BaseFont.FontType)
                    {
                        case BaseFont.FONT_TYPE_TT:
                        case BaseFont.FONT_TYPE_TTUNI:
                            return foundFont; // SUCCESS
                    }
                }
            }
            throw new InvalidOperationException("Unable to find TrueType font to test with - add the name of a TT font on the system to the fontNames array in this method");
        }


        private static byte[] CreatePdf(Font font)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            document.Add(new Paragraph(TEXT1, font));
            document.NewPage();
            document.Add(new Paragraph(TEXT2, font));
            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }


        /**
         * Used for testing only if we need to open the PDF itself
         * @param bytes
         * @param file
         * @
         */
        private void SaveBytesToFile(byte[] bytes, string filePath)
        {
            FileStream outputStream = new FileStream(filePath, FileMode.Create);
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Close();
            Console.WriteLine("PDF dumped to " + filePath);
        }

        /**
         * Test parsing a document which uses a standard non-embedded font.
         *
         * @ any exception will cause the test to fail
         */
        [Test]
        public void TestStandardFont()
        {
            Font font = new Font(Font.FontFamily.TIMES_ROMAN, 12);
            byte[] pdfBytes = CreatePdf(font);

            if (false)
            {
                //saveBytesToFile(pdfBytes, new File("testout", "test.pdf"));
            }

            CheckPdf(pdfBytes);

        }


        /**
         * Test parsing a document which uses a font encoding which creates a /Differences
         * PdfArray in the PDF.
         *
         * @ any exception will cause the test to fail
         */
        [Test, Ignore("Failing on hudson, not locally")]
        public void TestEncodedFont()
        {
            Font font = GetSomeTTFont("ISO-8859-1", BaseFont.EMBEDDED, 12);
            byte[] pdfBytes = CreatePdf(font);
            CheckPdf(pdfBytes);
        }


        /**
         * Test parsing a document which uses a Unicode font encoding which creates a /ToUnicode
         * PdfArray.
         *
         * @ any exception will cause the test to fail
         */
        [Test, Ignore("ailing on hudson, not locally")]
        public void TestUnicodeFont()
        {
            Font font = GetSomeTTFont(BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 12);
            byte[] pdfBytes = CreatePdf(font);
            CheckPdf(pdfBytes);
        }


        private void CheckPdf(byte[] pdfBytes)
        {

            PdfReader pdfReader = new PdfReader(pdfBytes);
            // Characters from http://unicode.org/charts/PDF/U0000.pdf
            Assert.AreEqual(TEXT1, PdfTextExtractor.GetTextFromPage(pdfReader, 1));
            // Characters from http://unicode.org/charts/PDF/U0080.pdf
            Assert.AreEqual(TEXT2, PdfTextExtractor.GetTextFromPage(pdfReader, 2));
        }

    }
}
