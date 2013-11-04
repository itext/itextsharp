using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfCopyTest {
        private const string RESOURCES = @"..\..\resources\text\pdf\PdfCopyTest\";

        [SetUp]
        public void SetUp() {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        public void TearDown() {
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


            System.Drawing.Bitmap awtImg = new System.Drawing.Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(awtImg);
            g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Green), 10, 10, 80, 80);
            g.Save();
            Image itextImg = Image.GetInstance(awtImg, (BaseColor)null);
            document.Add(itextImg);

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
#endif// DRAWING


        [Test]
        /**
         * Test to make sure that the following issue is fixed: http://sourceforge.net/mailarchive/message.php?msg_id=30891213
         */
        public void TestDecodeParmsArrayWithNullItems() {
            Document document = new Document();
            MemoryStream byteStream = new MemoryStream();
            PdfSmartCopy pdfSmartCopy = new PdfSmartCopy(document, byteStream);
            document.Open();

            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(RESOURCES, "imgWithDecodeParms.pdf");
            pdfSmartCopy.AddPage(pdfSmartCopy.GetImportedPage(reader, 1));

            document.Close();
            reader.Close();

            reader = new PdfReader(byteStream.ToArray());
            PdfDictionary page = reader.GetPageN(1);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xObject = resources.GetAsDict(PdfName.XOBJECT);
            PdfStream img = xObject.GetAsStream(new PdfName("Im0"));
            PdfArray decodeParms = img.GetAsArray(PdfName.DECODEPARMS);
            Assert.AreEqual(2, decodeParms.Size);
            Assert.IsTrue(decodeParms[0] is PdfNull);

            reader.Close();
        }
    }
}
