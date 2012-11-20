/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class GraphicsStateStack : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(new Rectangle(200, 120))) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // state 1:
        canvas.SetRGBColorFill(0xFF, 0x45, 0x00);
        // fill a rectangle in state 1
        canvas.Rectangle(10, 10, 60, 60);
        canvas.Fill();
        canvas.SaveState();
        // state 2;
        canvas.SetLineWidth(3);
        canvas.SetRGBColorFill(0x8B, 0x00, 0x00);
        // fill and stroke a rectangle in state 2
        canvas.Rectangle(40, 20, 60, 60);
        canvas.FillStroke();
        canvas.SaveState();
        // state 3:
        canvas.ConcatCTM(1, 0, 0.1f, 1, 0, 0);
        canvas.SetRGBColorStroke(0xFF, 0x45, 0x00);
        canvas.SetRGBColorFill(0xFF, 0xD7, 0x00);
        // fill and stroke a rectangle in state 3
        canvas.Rectangle(70, 30, 60, 60);
        canvas.FillStroke();
        canvas.RestoreState();
        // stroke a rectangle in state 2
        canvas.Rectangle(100, 40, 60, 60);
        canvas.Stroke();
        canvas.RestoreState();
        // fill and stroke a rectangle in state 1
        canvas.Rectangle(130, 50, 60, 60);
        canvas.FillStroke();        
      }
    }
// ===========================================================================
  }
}