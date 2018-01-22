using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class BarcodeMacroPDF417Test : ITextTest {
        private const String CMP_DIR = @"..\..\resources\text\pdf\BarcodeMacroPDF417Test\";
        private const String OUT_DIR = "com/itextpdf/test/pdf/BarcodeMacroPDF417Test/";


        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(OUT_DIR);
        }


        protected override void MakePdf(String outPdf) {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            Image img = CreateBarcode(cb, "This is PDF417 segment 0", 1, 1, 0);
            document.Add(new Paragraph("This is PDF417 segment 0"));
            document.Add(img);

            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));

            img = CreateBarcode(cb, "This is PDF417 segment 1", 1, 1, 1);
            document.Add(new Paragraph("This is PDF417 segment 1"));
            document.Add(img);
            document.Close();
        }

        public Image CreateBarcode(PdfContentByte cb, String text, float mh, float mw, int segmentId) {
            BarcodePDF417 pf = new BarcodePDF417();

            // MacroPDF417 setup
            pf.Options = BarcodePDF417.PDF417_USE_MACRO;
            pf.MacroFileId = "12";
            pf.MacroSegmentCount = 2;
            pf.MacroSegmentId = segmentId;

            pf.SetText(text);
            Rectangle size = pf.GetBarcodeSize();
            PdfTemplate template = cb.CreateTemplate(mw * size.Width, mh * size.Height);
            pf.PlaceBarcode(template, BaseColor.BLACK, mh, mw);
            return Image.GetInstance(template);
        }

        [Test]
        public void Test() {
            RunTest();
        }

        protected override void ComparePdf(String outPdf, String cmpPdf) {
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outPdf, cmpPdf, OUT_DIR, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        protected override String GetOutPdf() {
            return OUT_DIR + "barcode_macro_pdf417.pdf";
        }


        protected override String GetCmpPdf() {
            return CMP_DIR + "cmp_barcode_macro_pdf417.pdf";
        }
    }
}
