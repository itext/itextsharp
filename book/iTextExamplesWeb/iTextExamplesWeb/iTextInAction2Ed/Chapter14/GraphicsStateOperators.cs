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
  public class GraphicsStateOperators : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // line widths
        canvas.SaveState();
        for (int i = 25; i > 0; i--) {
          canvas.SetLineWidth((float) i / 10);
          canvas.MoveTo(50, 806 - (5 * i));
          canvas.LineTo(320, 806 - (5 * i));
          canvas.Stroke();
        }
        canvas.RestoreState();
        
        // line cap
        canvas.MoveTo(350, 800);
        canvas.LineTo(350, 750);
        canvas.MoveTo(540, 800);
        canvas.LineTo(540, 750);
        canvas.Stroke();
        
        canvas.SaveState();
        canvas.SetLineWidth(8);
        canvas.SetLineCap(PdfContentByte.LINE_CAP_BUTT);
        canvas.MoveTo(350, 790);
        canvas.LineTo(540, 790);
        canvas.Stroke();
        canvas.SetLineCap(PdfContentByte.LINE_CAP_ROUND);
        canvas.MoveTo(350, 775);
        canvas.LineTo(540, 775);
        canvas.Stroke();
        canvas.SetLineCap(PdfContentByte.LINE_CAP_PROJECTING_SQUARE);
        canvas.MoveTo(350, 760);
        canvas.LineTo(540, 760);
        canvas.Stroke();
        canvas.RestoreState();
        
        // join miter
        canvas.SaveState();
        canvas.SetLineWidth(8);
        canvas.SetLineJoin(PdfContentByte.LINE_JOIN_MITER);
        canvas.MoveTo(387, 700);
        canvas.LineTo(402, 730);
        canvas.LineTo(417, 700);
        canvas.Stroke();
        canvas.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);
        canvas.MoveTo(427, 700);
        canvas.LineTo(442, 730);
        canvas.LineTo(457, 700);
        canvas.Stroke();
        canvas.SetLineJoin(PdfContentByte.LINE_JOIN_BEVEL);
        canvas.MoveTo(467, 700);
        canvas.LineTo(482, 730);
        canvas.LineTo(497, 700);
        canvas.Stroke();
        canvas.RestoreState();

        // line dash
        canvas.SaveState();
        canvas.SetLineWidth(3);
        canvas.MoveTo(50, 660);
        canvas.LineTo(320, 660);
        canvas.Stroke();
        canvas.SetLineDash(6, 0);
        canvas.MoveTo(50, 650);
        canvas.LineTo(320, 650);
        canvas.Stroke();
        canvas.SetLineDash(6, 3);
        canvas.MoveTo(50, 640);
        canvas.LineTo(320, 640);
        canvas.Stroke();
        canvas.SetLineDash(15, 10, 5);
        canvas.MoveTo(50, 630);
        canvas.LineTo(320, 630);
        canvas.Stroke();
        float[] dash1 = { 10, 5, 5, 5, 20 };
        canvas.SetLineDash(dash1, 5);
        canvas.MoveTo(50, 620);
        canvas.LineTo(320, 620);
        canvas.Stroke();
        float[] dash2 = { 9, 6, 0, 6 };
        canvas.SetLineCap(PdfContentByte.LINE_CAP_ROUND);
        canvas.SetLineDash(dash2, 0);
        canvas.MoveTo(50, 610);
        canvas.LineTo(320, 610);
        canvas.Stroke();
        canvas.RestoreState();

        // miter limit
        PdfTemplate hooks = canvas.CreateTemplate(300, 120);
        hooks.SetLineWidth(8);
        hooks.MoveTo(46, 50);
        hooks.LineTo(65, 80);
        hooks.LineTo(84, 50);
        hooks.Stroke();
        hooks.MoveTo(87, 50);
        hooks.LineTo(105, 80);
        hooks.LineTo(123, 50);
        hooks.Stroke();
        hooks.MoveTo(128, 50);
        hooks.LineTo(145, 80);
        hooks.LineTo(162, 50);
        hooks.Stroke();
        hooks.MoveTo(169, 50);
        hooks.LineTo(185, 80);
        hooks.LineTo(201, 50);
        hooks.Stroke();
        hooks.MoveTo(210, 50);
        hooks.LineTo(225, 80);
        hooks.LineTo(240, 50);
        hooks.Stroke();
        
        canvas.SaveState();
        canvas.SetMiterLimit(2);
        canvas.AddTemplate(hooks, 300, 600);
        canvas.RestoreState();
        
        canvas.SaveState();
        canvas.SetMiterLimit(2.1f);
        canvas.AddTemplate(hooks, 300, 550);
        canvas.RestoreState();
      }
    }
// ===========================================================================
  }
}