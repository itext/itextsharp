using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class TextRenderInfoTest
    {
        [Test]
        virtual public void TestCharacterRenderInfos()
        {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate(), "ABCD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            PdfReaderContentParser parser = new PdfReaderContentParser(r);
            parser.ProcessContent(1, new CharacterPositionRenderListener());

        }

        private class CharacterPositionRenderListener : ITextExtractionStrategy
        {

            virtual public void BeginTextBlock()
            {
            }

            virtual public void RenderText(TextRenderInfo renderInfo)
            {
                List<TextRenderInfo> subs = renderInfo.GetCharacterRenderInfos();
                TextRenderInfo previousCharInfo = subs[0];

                for (int i = 1; i < subs.Count; i++)
                {
                    TextRenderInfo charInfo = subs[i];
                    Vector previousEndPoint = previousCharInfo.GetBaseline().GetEndPoint();
                    Vector currentStartPoint = charInfo.GetBaseline().GetStartPoint();
                    AssertVectorsEqual(previousEndPoint, currentStartPoint, charInfo.GetText());
                    previousCharInfo = charInfo;
                }

            }

            private void AssertVectorsEqual(Vector v1, Vector v2, String message)
            {
                Assert.AreEqual(v1[0], v2[0], 1 / 72f, message);
                Assert.AreEqual(v1[1], v2[1], 1 / 72f, message);
            }
            virtual public void EndTextBlock()
            {
            }

            virtual public void RenderImage(ImageRenderInfo renderInfo)
            {
            }

            virtual public String GetResultantText()
            {
                return null;
            }

        }

        private byte[] CreateSimplePdf(Rectangle pageSize, params string[] text)
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(pageSize);
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            foreach (string str in text)
            {
                document.Add(new Paragraph(str));
                document.NewPage();
            }

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
    }
}
