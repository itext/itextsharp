/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter10 {
  public class Barcodes : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(new Rectangle(340, 842))) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;

        // EAN 13
        document.Add(new Paragraph("Barcode EAN.UCC-13"));
        BarcodeEAN codeEAN = new BarcodeEAN();
        codeEAN.Code = "4512345678906";
        document.Add(new Paragraph("default:"));
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));
        codeEAN.GuardBars = false;
        document.Add(new Paragraph("without guard bars:"));
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));
        codeEAN.Baseline = -1f;
        codeEAN.GuardBars = true;
        document.Add(new Paragraph("text above:"));
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));
        codeEAN.Baseline = codeEAN.Size;

        // UPC A
        document.Add(new Paragraph("Barcode UCC-12 (UPC-A)"));
        codeEAN.CodeType = Barcode.UPCA;
        codeEAN.Code = "785342304749";
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));

        // EAN 8
        document.Add(new Paragraph("Barcode EAN.UCC-8"));
        codeEAN.CodeType = Barcode.EAN8;
        codeEAN.BarHeight = codeEAN.Size * 1.5f;
        codeEAN.Code = "34569870";
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));

        // UPC E
        document.Add(new Paragraph("Barcode UPC-E"));
        codeEAN.CodeType = Barcode.UPCE;
        codeEAN.Code = "03456781";
        document.Add(codeEAN.CreateImageWithBarcode(cb, null, null));
        codeEAN.BarHeight = codeEAN.Size * 3f;

        // EANSUPP
        document.Add(new Paragraph("Bookland"));
        document.Add(new Paragraph("ISBN 0-321-30474-8"));
        codeEAN.CodeType = Barcode.EAN13;
        codeEAN.Code = "9781935182610";
        BarcodeEAN codeSUPP = new BarcodeEAN();
        codeSUPP.CodeType = Barcode.SUPP5;
        codeSUPP.Code = "55999";
        codeSUPP.Baseline = -2;
        BarcodeEANSUPP eanSupp = new BarcodeEANSUPP(codeEAN, codeSUPP);
        document.Add(eanSupp.CreateImageWithBarcode(cb, null, BaseColor.BLUE));

        // CODE 128
        document.Add(new Paragraph("Barcode 128"));
        Barcode128 code128 = new Barcode128();
        code128.Code = "0123456789 hello";
        document.Add(code128.CreateImageWithBarcode(cb, null, null));
        code128.Code = "0123456789\uffffMy Raw Barcode (0 - 9)";
        code128.CodeType = Barcode.CODE128_RAW;
        document.Add(code128.CreateImageWithBarcode(cb, null, null));

        // Data for the barcode :
        String code402 = "24132399420058289";
        String code90 = "3700000050";
        String code421 = "422356";
        StringBuilder data = new StringBuilder(code402);
        data.Append(Barcode128.FNC1);
        data.Append(code90);
        data.Append(Barcode128.FNC1);
        data.Append(code421);
        Barcode128 shipBarCode = new Barcode128();
        shipBarCode.X = 0.75f;
        shipBarCode.N = 1.5f;
        shipBarCode.Size = 10f;
        shipBarCode.TextAlignment = Element.ALIGN_CENTER;
        shipBarCode.Baseline = 10f;
        shipBarCode.BarHeight = 50f;
        shipBarCode.Code = data.ToString();
        document.Add(shipBarCode.CreateImageWithBarcode(
          cb, BaseColor.BLACK, BaseColor.BLUE
        ));

        // it is composed of 3 blocks whith AI 01, 3101 and 10
        Barcode128 uccEan128 = new Barcode128();
        uccEan128.CodeType = Barcode.CODE128_UCC;
        uccEan128.Code = "(01)00000090311314(10)ABC123(15)060916";
        document.Add(uccEan128.CreateImageWithBarcode(
          cb, BaseColor.BLUE, BaseColor.BLACK
        ));
        uccEan128.Code = "0191234567890121310100035510ABC123";
        document.Add(uccEan128.CreateImageWithBarcode(
          cb, BaseColor.BLUE, BaseColor.RED
        ));
        uccEan128.Code = "(01)28880123456788";
        document.Add(uccEan128.CreateImageWithBarcode(
          cb, BaseColor.BLUE, BaseColor.BLACK
        ));

        // INTER25
        document.Add(new Paragraph("Barcode Interleaved 2 of 5"));
        BarcodeInter25 code25 = new BarcodeInter25();
        code25.GenerateChecksum = true;
        code25.Code = "41-1200076041-001";
        document.Add(code25.CreateImageWithBarcode(cb, null, null));
        code25.Code = "411200076041001";
        document.Add(code25.CreateImageWithBarcode(cb, null, null));
        code25.Code = "0611012345678";
        code25.ChecksumText = true;
        document.Add(code25.CreateImageWithBarcode(cb, null, null));

        // POSTNET
        document.Add(new Paragraph("Barcode Postnet"));
        BarcodePostnet codePost = new BarcodePostnet();
        document.Add(new Paragraph("ZIP"));
        codePost.Code = "01234";
        document.Add(codePost.CreateImageWithBarcode(cb, null, null));
        document.Add(new Paragraph("ZIP+4"));
        codePost.Code = "012345678";
        document.Add(codePost.CreateImageWithBarcode(cb, null, null));
        document.Add(new Paragraph("ZIP+4 and dp"));
        codePost.Code = "01234567890";
        document.Add(codePost.CreateImageWithBarcode(cb, null, null));

        document.Add(new Paragraph("Barcode Planet"));
        BarcodePostnet codePlanet = new BarcodePostnet();
        codePlanet.Code = "01234567890";
        codePlanet.CodeType = Barcode.PLANET;
        document.Add(codePlanet.CreateImageWithBarcode(cb, null, null));

        // CODE 39
        document.Add(new Paragraph("Barcode 3 of 9"));
        Barcode39 code39 = new Barcode39();
        code39.Code = "ITEXT IN ACTION";
        document.Add(code39.CreateImageWithBarcode(cb, null, null));

        document.Add(new Paragraph("Barcode 3 of 9 extended"));
        Barcode39 code39ext = new Barcode39();
        code39ext.Code = "iText in Action";
        code39ext.StartStopText = false;
        code39ext.Extended = true;
        document.Add(code39ext.CreateImageWithBarcode(cb, null, null));

        // CODABAR
        document.Add(new Paragraph("Codabar"));
        BarcodeCodabar codabar = new BarcodeCodabar();
        codabar.Code = "A123A";
        codabar.StartStopText = true;
        document.Add(codabar.CreateImageWithBarcode(cb, null, null));

        // PDF417
        document.Add(new Paragraph("Barcode PDF417"));
        BarcodePDF417 pdf417 = new BarcodePDF417();
        String text = "Call me Ishmael. Some years ago--never mind how long "
        + "precisely --having little or no money in my purse, and nothing "
              + "particular to interest me on shore, I thought I would sail about "
              + "a little and see the watery part of the world."
            ;
        pdf417.SetText(text);
        Image img = pdf417.GetImage();
        img.ScalePercent(50, 50 * pdf417.YHeight);
        document.Add(img);

        document.Add(new Paragraph("Barcode Datamatrix"));
        BarcodeDatamatrix datamatrix = new BarcodeDatamatrix();
        datamatrix.Generate(text);
        img = datamatrix.CreateImage();
        document.Add(img);

        document.Add(new Paragraph("Barcode QRCode"));
        BarcodeQRCode qrcode = new BarcodeQRCode(
          "Moby Dick by Herman Melville", 1, 1, null
        );
        img = qrcode.GetImage();
        document.Add(img);        
      }
    }
// ===========================================================================
  }
}