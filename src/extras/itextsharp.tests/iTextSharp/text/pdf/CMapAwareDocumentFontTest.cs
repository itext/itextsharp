using System;
using System.Collections.Generic;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    internal class CMapAwareDocumentFontTest {
        private string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\CMapAwareDocumentFontTest\";

        [Test]
        virtual public void TestWidths() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "fontwithwidthissue.pdf");

            try {
                PdfDictionary fontsDic = pdfReader.GetPageN(1).GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.FONT);
                PRIndirectReference fontDicIndirect = (PRIndirectReference) fontsDic.Get(new PdfName("F1"));

                CMapAwareDocumentFont f = new CMapAwareDocumentFont(fontDicIndirect);
                Assert.IsTrue(f.GetWidth('h') != 0, "Width should not be 0");
            }
            finally {
                pdfReader.Close();
            }
        }

        [Test]
        virtual public void WeirdHyphensTest() {
            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "WeirdHyphens.pdf");
            List<String> textChunks = new List<String>();
            IRenderListener listener = new MyTextRenderListener(textChunks);
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            /**
             * This assertion makes sure that encoding has been read properly from FontDescriptor.
             * If not the vallue will be "\u0000 14".
             */
            Assert.AreEqual("\u0096 14", textChunks[18]);
            reader.Close();
        }

        private class MyTextRenderListener : IRenderListener {
            private List<String> textChunks;

            public MyTextRenderListener(List<String> textChunks) {
                this.textChunks = textChunks;
            }

            virtual public void BeginTextBlock() {
            }

            virtual public void EndTextBlock() {
            }

            virtual public void RenderImage(ImageRenderInfo renderInfo) {
            }

            virtual public void RenderText(TextRenderInfo renderInfo) {
                textChunks.Add(renderInfo.GetText());
            }
        }
    }
}
