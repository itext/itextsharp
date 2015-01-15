using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    public class TextRenderInfoTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\TextRenderInfoTest\";

        [Test]
        public virtual void TestCharacterRenderInfos() {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate(), "ABCD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            PdfReaderContentParser parser = new PdfReaderContentParser(r);
            parser.ProcessContent(1, new CharacterPositionRenderListener());
        }
        
        /**
         * Test introduced to exclude a bug related to a Unicode quirk for 
         * Japanese. TextRenderInfo threw an AIOOBE for some characters.
         * @throws java.lang.Exception
         * @since 5.5.5-SNAPSHOT
         */
        [Test]
        public void TestUnicodeEmptyString()  {
            StringBuilder sb = new StringBuilder();
            String inFile = "japanese_text.pdf";

    
            PdfReader p = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, inFile);
            ITextExtractionStrategy strat = new SimpleTextExtractionStrategy();

            sb.Append(PdfTextExtractor.GetTextFromPage(p, 1, strat));

            String result = sb.ToString(0, sb.ToString().IndexOf('\n'));
            String origText =
                    "\u76f4\u8fd1\u306e\u0053\uff06\u0050\u0035\u0030\u0030"
                    + "\u914d\u5f53\u8cb4\u65cf\u6307\u6570\u306e\u30d1\u30d5"
                    + "\u30a9\u30fc\u30de\u30f3\u30b9\u306f\u0053\uff06\u0050"
                    + "\u0035\u0030\u0030\u6307\u6570\u3092\u4e0a\u56de\u308b";
            Assert.AreEqual(result, origText);
        }

        private class CharacterPositionRenderListener : ITextExtractionStrategy {
            public virtual void BeginTextBlock() {
            }

            public virtual void RenderText(TextRenderInfo renderInfo) {
                IList<TextRenderInfo> subs = renderInfo.GetCharacterRenderInfos();
                TextRenderInfo previousCharInfo = subs[0];

                for (int i = 1; i < subs.Count; i++) {
                    TextRenderInfo charInfo = subs[i];
                    Vector previousEndPoint = previousCharInfo.GetBaseline().GetEndPoint();
                    Vector currentStartPoint = charInfo.GetBaseline().GetStartPoint();
                    AssertVectorsEqual(previousEndPoint, currentStartPoint, charInfo.GetText());
                    previousCharInfo = charInfo;
                }
            }

            private void AssertVectorsEqual(Vector v1, Vector v2, String message) {
                Assert.AreEqual(v1[0], v2[0], 1/72f, message);
                Assert.AreEqual(v1[1], v2[1], 1/72f, message);
            }

            public virtual void EndTextBlock() {
            }

            public virtual void RenderImage(ImageRenderInfo renderInfo) {
            }

            public virtual String GetResultantText() {
                return null;
            }
        }

        private byte[] CreateSimplePdf(Rectangle pageSize, params string[] text) {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(pageSize);
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            foreach (string str in text) {
                document.Add(new Paragraph(str));
                document.NewPage();
            }

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
    }
}
