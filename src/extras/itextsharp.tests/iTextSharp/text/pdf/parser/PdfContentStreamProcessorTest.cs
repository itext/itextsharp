using System;
using System.IO;
using System.util;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class PdfContentStreamProcessorTest
    {
        private DebugRenderListener _renderListener;
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\PdfContentStreamProcessorTest\";

        [SetUp]
        public void SetUp()
        {
            _renderListener = new DebugRenderListener();
        }

        // Replicates iText bug 2817030
        [Test]
        public void TestPositionAfterTstar()
        {
            ProcessBytes("yaxiststar.pdf", 1);
        }


        private void ProcessBytes(
            string resourceName,
            int pageNumber)
        {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, resourceName);

            PdfDictionary pageDictionary = pdfReader.GetPageN(pageNumber);

            PdfDictionary resourceDictionary = pageDictionary.GetAsDict(PdfName.RESOURCES);

            PdfObject contentObject = pageDictionary.Get(PdfName.CONTENTS);
            byte[] contentBytes = ReadContentBytes(contentObject);
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(_renderListener);
            processor.ProcessContent(contentBytes, resourceDictionary);

        }


        private byte[] ReadContentBytes(
             PdfObject contentObject)
        {
            byte[] result;
            switch (contentObject.Type)
            {
                case PdfObject.INDIRECT:
                    PRIndirectReference reference = (PRIndirectReference)contentObject;
                    PdfObject directObject = PdfReader.GetPdfObject(reference);
                    result = ReadContentBytes(directObject);
                    break;
                case PdfObject.STREAM:
                    PRStream stream = (PRStream)PdfReader.GetPdfObject(contentObject);
                    result = PdfReader.GetStreamBytes(stream);
                    break;
                case PdfObject.ARRAY:
                    // Stitch together all content before calling processContent(), because
                    // processContent() resets state.
                    MemoryStream allBytes = new MemoryStream();
                    PdfArray contentArray = (PdfArray)contentObject;
                    ListIterator<PdfObject> iter = contentArray.GetListIterator();
                    while (iter.HasNext())
                    {
                        PdfObject element = iter.Next();
                        byte[] bytes = ReadContentBytes(element);
                        allBytes.Write(bytes, 0, bytes.Length);
                    }
                    result = allBytes.ToArray();
                    break;
                default:
                    String msg = "Unable to handle Content of type " + contentObject.GetType();
                    throw new InvalidOperationException(msg);
            }
            return result;
        }


        private class DebugRenderListener : IRenderListener
        {
            private float _lastY = float.MaxValue;

            public void RenderText(TextRenderInfo renderInfo)
            {
                Vector start = renderInfo.GetBaseline().GetStartPoint();
                float x = start[Vector.I1];
                float y = start[Vector.I2];
                //        System.out.println("Display text: '" + renderInfo.getText() + "' (" + x + "," + y + ")");
                if (y > _lastY)
                {
                    Assert.Fail("Text has jumped back up the page");
                }
                _lastY = y;

            }

            public void BeginTextBlock()
            {
                _lastY = float.MaxValue;
            }

            public void EndTextBlock()
            {
            }

            public void RenderImage(ImageRenderInfo renderInfo)
            {
            }

        }
    }
}
