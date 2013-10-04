using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;
using Image = iTextSharp.text.Image;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfCopyTest
    {
        [SetUp]
        public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        
        /**
         * Test to demonstrate issue https://sourceforge.net/tracker/?func=detail&aid=3013642&group_id=15255&atid=115255
         */
        [Test]
#if DRAWING
        [Ignore]
#endif// !NO_DRAWING
        public void TestExtraXObjects()
        {
#if DRAWING
            PdfReader sourceR = new PdfReader(CreateImagePdf());
            try
            {
                int sourceXRefCount = sourceR.XrefSize;

                Document document = new Document();
                MemoryStream outStream = new MemoryStream();
                PdfCopy copy = new PdfCopy(document, outStream);
                document.Open();
                PdfImportedPage importedPage = copy.GetImportedPage(sourceR, 1);
                copy.AddPage(importedPage);
                document.Close();

                PdfReader targetR = new PdfReader(outStream.ToArray());
                int destinationXRefCount = targetR.XrefSize;

                //        TestResourceUtils.saveBytesToFile(createImagePdf(), new File("./source.pdf"));
                //        TestResourceUtils.saveBytesToFile(out.toByteArray(), new File("./result.pdf"));

                Assert.AreEqual(sourceXRefCount, destinationXRefCount);
            }
            finally
            {
                sourceR.Close();
            }
#endif// DRAWING
        }

#if DRAWING
        private static byte[] CreateImagePdf()
        {
            MemoryStream byteStream = new MemoryStream();


            Document document = new Document();
            document.SetPageSize(PageSize.LETTER);

            document.Open();


            Bitmap awtImg = new Bitmap(100, 100, PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(awtImg);
            g.FillRectangle(new SolidBrush(Color.Green), 10, 10, 80, 80);
            g.Save();
            Image itextImg = Image.GetInstance(awtImg, (BaseColor)null);
            document.Add(itextImg);

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
#endif// DRAWING
    }
}
