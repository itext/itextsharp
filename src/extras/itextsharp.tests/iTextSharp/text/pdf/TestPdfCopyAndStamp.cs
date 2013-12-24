using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class TestPdfCopyAndStamp
    {
        String[] input;
        Dictionary<String, byte[]> pdfContent = new Dictionary<String, byte[]>();
        String output;
        String stamp;
        String multiPageStamp;

        private void CreateReader(String name, String[] pageContents)
        {
            MemoryStream ms = new MemoryStream();

            Document document = new Document();
            PdfWriter.GetInstance(document, ms);
            document.Open();

            for (int i = 0; i < pageContents.Length; i++)
            {
                if (i != 0)
                    document.NewPage();

                String content = pageContents[i];
                Chunk contentChunk = new Chunk(content);
                document.Add(contentChunk);
            }


            document.Close();

            pdfContent[name] = ms.ToArray();
        }


        [SetUp]
        virtual public void SetUp()
        {
            input = new String[]{
                "content1.pdf",
                "content2.pdf",
                };

            stamp = "Stamp.PDF";
            multiPageStamp = "MultiStamp.PDF";
            output = "TestOut.pdf";

            CreateReader(input[0], new String[] { "content 1" });
            CreateReader(input[1], new String[] { "content 2" });

            CreateReader(stamp, new String[] { "          This is a stamp" });
            CreateReader(multiPageStamp, new String[] { "          This is a stamp - page 1", "          This is a stamp - page 2" });
        }

        [TearDown]
        virtual public void TearDown()
        {
        }

        virtual public void MergeAndStampPdf(bool resetStampEachPage, String[] input, String output, String stamp)
        {
            PdfReader stampReader = new PdfReader(pdfContent[stamp]);
            List<PdfReader> readersToClose = new List<PdfReader>();
            readersToClose.Add(stampReader);

            MemoryStream baos = new MemoryStream();

            try
            {
                Document document = new Document();

                PdfCopy writer = new PdfSmartCopy(document, baos);
                try
                {

                    document.Open();

                    int stampPageNum = 1;

                    foreach (string element in input)
                    {
                        // create a reader for the input document
                        PdfReader documentReader = new PdfReader(
                                new RandomAccessFileOrArray(
                                    new RandomAccessSourceFactory().CreateSource(pdfContent[element])
                                )
                            , null);

                        for (int pageNum = 1; pageNum <= documentReader.NumberOfPages; pageNum++)
                        {

                            // import a page from the main file
                            PdfImportedPage mainPage = writer.GetImportedPage(documentReader, pageNum);

                            // make a stamp from the page and get under content...
                            PdfCopy.PageStamp pageStamp = writer.CreatePageStamp(mainPage);

                            // import a page from a file with the stamp...
                            if (resetStampEachPage)
                            {
                                stampReader = new PdfReader(pdfContent[stamp]);
                                readersToClose.Add(stampReader);
                            }
                            PdfImportedPage stampPage = writer.GetImportedPage(stampReader, stampPageNum++);

                            // add the stamp template, update stamp, and add the page
                            pageStamp.GetOverContent().AddTemplate(stampPage, 0, 0);
                            pageStamp.AlterContents();
                            writer.AddPage(mainPage);

                            if (stampPageNum > stampReader.NumberOfPages)
                                stampPageNum = 1;
                        }
                    }
                }
                finally
                {
                    writer.Close();
                    document.Close();
                }
            }
            finally
            {
                foreach (PdfReader stampReaderToClose in readersToClose)
                {
                    stampReaderToClose.Close();
                }
            }
            pdfContent[output] = baos.ToArray();
        }

        virtual protected void TestXObject(bool shouldExist, int page, String xObjectName)
        {
            PdfReader reader = null;
            RandomAccessFileOrArray raf = null;
            raf = new RandomAccessFileOrArray(pdfContent[output]);
            reader = new PdfReader(raf, null);
            try
            {
                PdfDictionary dictionary = reader.GetPageN(page);

                PdfDictionary resources = (PdfDictionary)dictionary.Get(PdfName.RESOURCES);
                PdfDictionary xobject = (PdfDictionary)resources.Get(PdfName.XOBJECT);
                PdfObject directXObject = xobject.GetDirectObject(new PdfName(xObjectName));
                PdfObject indirectXObject = xobject.Get(new PdfName(xObjectName));

                if (shouldExist)
                {
                    Assert.NotNull(indirectXObject);
                    Assert.NotNull(directXObject);
                }
                else
                {
                    Assert.IsNull(indirectXObject);
                    Assert.IsNull(directXObject);
                }
            }
            finally
            {
                reader.Close();
            }


        }

        [Test]
        virtual public void TestWithReloadingStampReader()
        {
            MergeAndStampPdf(true, input, output, stamp);

            TestXObject(true, 1, "Xi0");
            TestXObject(true, 2, "Xi1");

        }


        [Test]
        virtual public void TestWithoutReloadingStampReader()
        {
            MergeAndStampPdf(false, input, output, stamp);

            //openFile(out); // if you open the resultant PDF at this point and go to page 2, you will get a nice error message

            TestXObject(true, 1, "Xi0");
            TestXObject(true, 2, "Xi1"); // if we are able to optimize iText so it re-uses the same XObject for multiple imports of the same page from the same PdfReader, then switch this to false

        }


        [Test]
        virtual public void RestMultiPageStampWithoutReloadingStampReader()
        {
            MergeAndStampPdf(false, input, output, multiPageStamp);

            // openFile(out); // if you open the resultant PDF at this point and go to page 2, you will get a nice error message

            TestXObject(true, 1, "Xi0");
            TestXObject(true, 2, "Xi1");

        }

        [Test]
        virtual public void TestMultiPageStampWithReloadingStampReader()
        {
            MergeAndStampPdf(true, input, output, multiPageStamp);

            // openFile(out); // if you open the resultant PDF at this point and go to page 2, you will get a nice error message

            TestXObject(true, 1, "Xi0");
            TestXObject(true, 2, "Xi1");

        }


        //    private void openFile(File f) throws IOException{
        //        String[] params = new String[]{
        //                "rundll32",
        //                "url.dll,FileProtocolHandler",
        //                "\"" + f.getCanonicalPath() + "\""
        //        };
        //        Runtime.getRuntime().exec(params);
        //    }
    }
}
