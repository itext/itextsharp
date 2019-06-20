/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
        virtual public void TestStandardFont()
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
        virtual public void TestEncodedFont()
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
        virtual public void TestUnicodeFont()
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
