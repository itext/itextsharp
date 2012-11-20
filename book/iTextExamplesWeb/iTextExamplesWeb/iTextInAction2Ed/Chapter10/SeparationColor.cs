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
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter10 {
  public class SeparationColor : DeviceColor {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        PdfSpotColor psc_g = new PdfSpotColor(
          "iTextSpotColorGray", new GrayColor(0.9f)
        );
        PdfSpotColor psc_rgb = new PdfSpotColor(
          "iTextSpotColorRGB", new BaseColor(0x64, 0x95, 0xed)
        );
        PdfSpotColor psc_cmyk = new PdfSpotColor(
          "iTextSpotColorCMYK", new CMYKColor(0.3f, .9f, .3f, .1f)
        );
        ColorRectangle(canvas, new SpotColor(psc_g, 0.5f), 
          36, 770, 36, 36
        );
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.1f), 
          90, 770, 36, 36
        );
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.2f), 
          144, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.3f), 
          198, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.4f), 
          252, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.5f), 
          306, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.6f), 
          360, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_rgb, 0.7f), 
          416, 770, 36, 36
        ); 
        ColorRectangle(canvas, new SpotColor(psc_cmyk, 0.25f), 
          470, 770, 36, 36
        );
        canvas.SetColorFill(psc_g, 0.5f);
        canvas.Rectangle(36, 716, 36, 36);
        canvas.FillStroke();
        canvas.SetColorFill(psc_g, 0.9f);
        canvas.Rectangle(90, 716, 36, 36);
        canvas.FillStroke();
        canvas.SetColorFill(psc_rgb, 0.5f);
        canvas.Rectangle(144, 716, 36, 36);
        canvas.FillStroke();
        canvas.SetColorFill(psc_rgb, 0.9f);
        canvas.Rectangle(198, 716, 36, 36);
        canvas.FillStroke();
        canvas.SetColorFill(psc_cmyk, 0.5f);
        canvas.Rectangle(252, 716, 36, 36);
        canvas.FillStroke();
        canvas.SetColorFill(psc_cmyk, 0.9f);
        canvas.Rectangle(306, 716, 36, 36);
        canvas.FillStroke();
      }
    }
// ===========================================================================
  }
}