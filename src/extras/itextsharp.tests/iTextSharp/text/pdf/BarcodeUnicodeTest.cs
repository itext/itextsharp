using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class BarcodeUnicodeTest : ITextTest {
        private const String OUT_DIR = "com/itextpdf/test/pdf/BarcodeUnicodeTest/";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(OUT_DIR);
        }

        protected override void MakePdf(string outPdf) {
            // step 1
            Document document = new Document(new Rectangle(340, 842));
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            PdfContentByte cb = writer.DirectContent;

            String str = "\u6D4B";

            document.Add(new Paragraph("QR code unicode"));
            IDictionary<EncodeHintType, Object> hints = new Dictionary<EncodeHintType, object>();
            hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
            BarcodeQRCode q = new BarcodeQRCode(str, 100, 100, hints);
            document.Add(q.GetImage());

            // step 5
            document.Close();
        }

        [Test]
        public virtual void Test() {
            RunTest();
        }

        protected override string GetOutPdf() {
            return OUT_DIR + "barcode.pdf";
        }
    }
}
