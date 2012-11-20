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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter11 {
  public class Type3Example : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Type3Font t3 = new Type3Font(writer, true);
        PdfContentByte d = t3.DefineGlyph('D', 600, 0, 0, 600, 700);
        d.SetColorStroke(new BaseColor(0xFF, 0x00, 0x00));
        d.SetColorFill(new GrayColor(0.7f));
        d.SetLineWidth(100);
        d.MoveTo(5, 5);
        d.LineTo(300, 695);
        d.LineTo(595, 5);
        d.ClosePathFillStroke();
        PdfContentByte s = t3.DefineGlyph('S', 600, 0, 0, 600, 700);
        s.SetColorStroke(new BaseColor(0x00, 0x80, 0x80));
        s.SetLineWidth(100);
        s.MoveTo(595,5);
        s.LineTo(5, 5);
        s.LineTo(300, 350);
        s.LineTo(5, 695);
        s.LineTo(595, 695);
        s.Stroke();
        Font f = new Font(t3, 12);
        Paragraph p = new Paragraph();
        p.Add("This is a String with a Type3 font that contains a fancy Delta (");
        p.Add(new Chunk("D", f));
        p.Add(") and a custom Sigma (");
        p.Add(new Chunk("S", f));
        p.Add(").");
        document.Add(p);        
      }
    }
// ===========================================================================
  }
}