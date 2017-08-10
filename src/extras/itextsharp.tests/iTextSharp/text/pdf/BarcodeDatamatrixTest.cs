using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.awt.geom;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class BarcodeDatamatrixTest {
        private String cmpFolder = @"..\..\resources\text\pdf\BarcodeDatamatrix\";
        private String outFolder = @"BarcodeDatamatrix\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(outFolder);
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void BarcodeTest01() {
            String filename = "barcodeDataMatrix01.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename,FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 01"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest02() {
            String filename = "barcodeDataMatrix02.pdf";
            String code = "дима";
            String encoding = "UTF-8";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 02"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Generate(code,encoding);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest03() {
            String filename = "barcodeDataMatrix03.pdf";
            String code = "AbcdFFghijklmnopqrstuWXSQ";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 03"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Width= 36;
            barcode.Height = 12;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.BLACK, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest04() {
            String filename = "barcodeDataMatrix04.pdf";
            String code = "01AbcdefgAbcdefg123451231231234";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 04"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Width = 36;
            barcode.Height = 12;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.BLACK, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest05() {
            String filename = "barcodeDataMatrix05.pdf";
            String code = "aaabbbcccdddAAABBBAAABBaaabbbcccdddaaa";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 05"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Width = 40;
            barcode.Height = 40;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.BLACK, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest06() {
            String filename = "barcodeDataMatrix06.pdf";
            String code = ">>>\r>>>THIS VERY TEXT>>\r>";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 06"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Width = 36;
            barcode.Height = 12;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.BLACK, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest07() {
            String filename = "barcodeDataMatrix07.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 07"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_ASCII;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest08() {
            String filename = "barcodeDataMatrix08.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 08"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_C40;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest09() {
            String filename = "barcodeDataMatrix09.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 09"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_TEXT;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest10() {
            String filename = "barcodeDataMatrix10.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 10"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_B256;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest11() {
            String filename = "barcodeDataMatrix11.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 11"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_X12;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest12() {
            String filename = "barcodeDataMatrix12.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 12"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_EDIFACT;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }

        [Test]
        public void BarcodeTest13() {
            String filename = "barcodeDataMatrix13.pdf";
            String code = "AAAAAAAAAA;BBBBAAAA3;00028;BBBAA05;AAAA;AAAAAA;1234567;AQWXSZ;JEAN;;;;7894561;AQWXSZ;GEO;;;;1;1;1;1;0;0;1;0;1;0;0;0;1;0;1;0;0;0;0;0;0;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1;1";

            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outFolder + filename, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Datamatrix test 13"));
            PdfContentByte cb = writer.DirectContent;
            cb.ConcatCTM(AffineTransform.GetTranslateInstance(PageSize.A4.Width / 2 - 100, PageSize.A4.Height / 2 - 100));
            BarcodeDatamatrix barcode = new BarcodeDatamatrix();
            barcode.Options = BarcodeDatamatrix.DM_RAW;
            barcode.Generate(code);
            barcode.PlaceBarcode(cb, BaseColor.GREEN, 5, 5);
            document.Close();

            CompareDocuments(filename);
        }



        /**
        * Utility method that checks the created file against the cmp file
        * @param file name of the output document
        * @throws DocumentException
        * @throws InterruptedException
        * @throws IOException
        */
        private void CompareDocuments(String file) {
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFolder + file, cmpFolder + file, outFolder, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }


    }
}
