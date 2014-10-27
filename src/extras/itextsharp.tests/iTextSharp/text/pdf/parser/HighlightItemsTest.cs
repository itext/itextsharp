using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    public class HighlightItemsTest {

        private const string outputPath = @"itextpdf\text\pdf\parser\HighlightItemsTest\";
        private const string inputPath = @"..\..\resources\text\pdf\parser\HighlightItemsTest\";

        [SetUp]
        public virtual void Before() {
            Directory.CreateDirectory(outputPath);
        }

        [Test]
        public virtual void HighlightPage229() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "page229.pdf";
            String cmp = inputPath + "cmp_page229.pdf";
            ParseAndHighlight(input, output, false);
            Assert.AreEqual(null, new CompareTool().Compare(output, cmp, outputPath, "diff"));
        }

        [Test]
        public virtual void HighlightCharactersPage229() {
            String input = inputPath + "page229.pdf";
            String output = outputPath + "page229_characters.pdf";
            String cmp = inputPath + "cmp_page229_characters.pdf";
            ParseAndHighlight(input, output, true);
            Assert.AreEqual(null, new CompareTool().Compare(output, cmp, outputPath, "diff"));
        }

        [Test]
        public virtual void HighlightIsoTc171() {
            String input = inputPath + "ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda.pdf";
            String output = outputPath + "SC2_N0896_SC2WG5_Edinburgh_Agenda.pdf";
            String cmp = inputPath + "cmp_ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda.pdf";
            ParseAndHighlight(input, output, false);
            Assert.AreEqual(null, new CompareTool().CompareByContent(output, cmp, outputPath, "diff"));
        }

        [Test]
        public virtual void HighlightCharactersIsoTc171() {
            String input = inputPath + "ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda.pdf";
            String output = outputPath + "ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda_characters.pdf";
            String cmp = inputPath + "cmp_ISO-TC171-SC2_N0896_SC2WG5_Edinburgh_Agenda_characters.pdf";
            ParseAndHighlight(input, output, true);
            Assert.AreEqual(null, new CompareTool().CompareByContent(output, cmp, outputPath, "diff"));
        }

        [Test]
        public virtual void HighlightHeaderFooter() {
            String input = inputPath + "HeaderFooter.pdf";
            String output = outputPath + "HeaderFooter.pdf";
            String cmp = inputPath + "cmp_HeaderFooter.pdf";
            ParseAndHighlight(input, output, false);
            Assert.AreEqual(null, new CompareTool().CompareByContent(output, cmp, outputPath, "diff"));
        }

        [Test]
        public virtual void HighlightCharactersHeaderFooter() {
            String input = inputPath + "HeaderFooter.pdf";
            String output = outputPath + "HeaderFooter_characters.pdf";
            String cmp = inputPath + "cmp_HeaderFooter_characters.pdf";
            ParseAndHighlight(input, output, true);
            Assert.AreEqual(null, new CompareTool().CompareByContent(output, cmp, outputPath, "diff"));
        }

        private void ParseAndHighlight(String input, String output, bool singleCharacters) {
            PdfReader reader = new PdfReader(input);
            FileStream fos = new FileStream(output, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fos);

            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            MyRenderListener myRenderListener = singleCharacters ? new MyCharacterRenderListener() : new MyRenderListener();
            for (int pageNum = 1; pageNum <= reader.NumberOfPages; pageNum++) {
                List<Rectangle> rectangles = parser.ProcessContent(pageNum, myRenderListener).GetRectangles();
                PdfContentByte canvas = stamper.GetOverContent(pageNum);
                canvas.SetLineWidth(0.5f);
                canvas.SetColorStroke(BaseColor.RED);
                foreach (Rectangle rectangle in rectangles) {
                    canvas.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                    canvas.Stroke();
                }
            }
            stamper.Close();
            fos.Close();
            reader.Close();
        }

//        private static String GetOutputPdfPath(Type c, String inputPdf, String suffix) {
//            File f = new File(c.getClassLoader().getResource(TestResourceUtils.GetFullyQualifiedResourceName(c, inputPdf)).toURI());
//            return f.getAbsolutePath() + suffix;
//        }
//
//        private static String GetOutputPdfPath(Type c, String inputPdf) {
//            File f = new File(c.getClassLoader().getResource(TestResourceUtils.GetFullyQualifiedResourceName(c, inputPdf)).toURI());
//            return f.getAbsolutePath().replaceAll(inputPdf, "");
//        }

        private class MyRenderListener : IRenderListener {
            private List<Rectangle> rectangles = new List<Rectangle>();

            public virtual void BeginTextBlock() {
            }

            public virtual void RenderText(TextRenderInfo renderInfo) {
                Vector startPoint = renderInfo.GetDescentLine().GetStartPoint();
                Vector endPoint = renderInfo.GetAscentLine().GetEndPoint();
                float x1 = Math.Min(startPoint[0], endPoint[0]);
                float x2 = Math.Max(startPoint[0], endPoint[0]);
                float y1 = Math.Min(startPoint[1], endPoint[1]);
                float y2 = Math.Max(startPoint[1], endPoint[1]);
                rectangles.Add(new Rectangle(x1, y1, x2, y2));
            }

            public virtual void EndTextBlock() {
            }

            public virtual void RenderImage(ImageRenderInfo renderInfo) {
            }

            public virtual List<Rectangle> GetRectangles() {
                return rectangles;
            }
        }

        private class MyCharacterRenderListener : MyRenderListener {
            public override void RenderText(TextRenderInfo renderInfo) {
                foreach (TextRenderInfo tri in renderInfo.GetCharacterRenderInfos())
                    base.RenderText(tri);
            }
        }
    }
}
