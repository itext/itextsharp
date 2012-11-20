/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter14 {
  public class TextMethods : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // draw helper lines
        PdfContentByte cb = writer.DirectContent;
        cb.SetLineWidth(0f);
        cb.MoveTo(150, 600);
        cb.LineTo(150, 800);
        cb.MoveTo(50, 760);
        cb.LineTo(250, 760);
        cb.MoveTo(50, 700);
        cb.LineTo(250, 700);
        cb.MoveTo(50, 640);
        cb.LineTo(250, 640);
        cb.Stroke();
        // draw text
        String text = "AWAY again ";
        BaseFont bf = BaseFont.CreateFont();
        cb.BeginText();
        cb.SetFontAndSize(bf, 12);
        cb.SetTextMatrix(50, 800);
        cb.ShowText(text);
        cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, text + " Center", 150, 760, 0);
        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, text + " Right", 150, 700, 0);
        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text + " Left", 150, 640, 0);
        cb.ShowTextAlignedKerned(PdfContentByte.ALIGN_LEFT, text + " Left", 150, 628, 0);
        cb.SetTextMatrix(0, 1, -1, 0, 300, 600);
        cb.ShowText("Position 300,600, rotated 90 degrees.");
        for (int i = 0; i < 360; i += 30) {
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, 400, 700, i);
        }
        cb.EndText();
      }
    }
// ===========================================================================
  }
}