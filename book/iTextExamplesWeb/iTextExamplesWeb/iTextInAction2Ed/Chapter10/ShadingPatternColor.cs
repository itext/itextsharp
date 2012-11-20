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
  public class ShadingPatternColor : DeviceColor {
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
        PdfShading axial = PdfShading.SimpleAxial(writer, 36, 716, 396,
          788, BaseColor.ORANGE, BaseColor.BLUE
        );
        canvas.PaintShading(axial);
        document.NewPage();
        PdfShading radial = PdfShading.SimpleRadial(writer,
          200, 700, 50, 300, 700, 100,
          new BaseColor(0xFF, 0xF7, 0x94),
          new BaseColor(0xF7, 0x8A, 0x6B),
          false, false
        );
        canvas.PaintShading(radial);

        PdfShadingPattern shading = new PdfShadingPattern(axial);
        ColorRectangle(canvas, new ShadingColor(shading), 150, 420, 126, 126);
        canvas.SetShadingFill(shading);
        canvas.Rectangle(300, 420, 126, 126);
        canvas.FillStroke();
      }
    }
// ===========================================================================
  }
}