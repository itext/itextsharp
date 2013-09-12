using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;
using System.IO;

namespace itextsharp.tests.iTextSharp.text
{
    class RemoteGifImageTest
    {
        private String[] GIF_LOCATION = {
            "http://itextpdf.com/img/logo.gif",
            "http://itextsupport.com/files/testresources/img/remote_gif_test.gif",
            @"..\..\resources\text\ChunkTest\logo.gif" // non-remote gif
        };

        private String OUTPUTFOLDER = "RemoteGifImageTest\\";
        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(OUTPUTFOLDER);
        }

        [Test]
        public void RemoteGifTest()
        {
            for (int i = 0; i < GIF_LOCATION.Length; i++)
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, new FileStream(OUTPUTFOLDER + "gif_remote[" + i + "].pdf", FileMode.Create));
                document.Open();

                Image img = Image.GetInstance(GIF_LOCATION[i]);
                document.Add(img);

                document.Close();
            }
        }
    }
}
