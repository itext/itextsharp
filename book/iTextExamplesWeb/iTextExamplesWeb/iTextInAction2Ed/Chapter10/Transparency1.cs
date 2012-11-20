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
  public class Transparency1 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;
        float gap = (document.PageSize.Width - 400) / 3;

        PictureBackdrop(gap, 500, cb);
        PictureBackdrop(200 + 2 * gap, 500, cb);
        PictureBackdrop(gap, 500 - 200 - gap, cb);
        PictureBackdrop(200 + 2 * gap, 500 - 200 - gap, cb);

        PictureCircles(gap, 500, cb);
        cb.SaveState();
        PdfGState gs1 = new PdfGState();
        gs1.FillOpacity = 0.5f;
        cb.SetGState(gs1);
        PictureCircles(200 + 2 * gap, 500, cb);
        cb.RestoreState();

        cb.SaveState();
        PdfTemplate tp = cb.CreateTemplate(200, 200);
        PdfTransparencyGroup group = new PdfTransparencyGroup();
        tp.Group = group;
        PictureCircles(0, 0, tp);
        cb.SetGState(gs1);
        cb.AddTemplate(tp, gap, 500 - 200 - gap);
        cb.RestoreState();

        cb.SaveState();
        tp = cb.CreateTemplate(200, 200);
        tp.Group = group;
        PdfGState gs2 = new PdfGState();
        gs2.FillOpacity = 0.5f;
        gs2.BlendMode = PdfGState.BM_HARDLIGHT;
        tp.SetGState(gs2);
        PictureCircles(0, 0, tp);
        cb.AddTemplate(tp, 200 + 2 * gap, 500 - 200 - gap);
        cb.RestoreState();

        cb.ResetRGBColorFill();
        ColumnText ct = new ColumnText(cb);
        Phrase ph = new Phrase("Ungrouped objects\nObject opacity = 1.0");
        ct.SetSimpleColumn(ph, gap, 0, gap + 200, 500, 18,
          Element.ALIGN_CENTER
        );
        ct.Go();

        ph = new Phrase("Ungrouped objects\nObject opacity = 0.5");
        ct.SetSimpleColumn(ph, 200 + 2 * gap, 0, 200 + 2 * gap + 200, 500,
          18, Element.ALIGN_CENTER
        );
        ct.Go();

        ph = new Phrase(
          "Transparency group\nObject opacity = 1.0\nGroup opacity = 0.5\nBlend mode = Normal"
        );
        ct.SetSimpleColumn(ph, gap, 0, gap + 200, 500 - 200 - gap, 18,
          Element.ALIGN_CENTER
        );
        ct.Go();

        ph = new Phrase(
          "Transparency group\nObject opacity = 0.5\nGroup opacity = 1.0\nBlend mode = HardLight"
        );
        ct.SetSimpleColumn(ph, 200 + 2 * gap, 0, 200 + 2 * gap + 200,
          500 - 200 - gap, 18, Element.ALIGN_CENTER
        );
        ct.Go();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Prints a square and fills half of it with a gray rectangle.
     * 
     * @param x
     * @param y
     * @param cb
     * @throws Exception
     */
    public static void PictureBackdrop(float x, float y, PdfContentByte cb) {
      cb.SetColorStroke(GrayColor.GRAYBLACK);
      cb.SetColorFill(new GrayColor(0.8f));
      cb.Rectangle(x, y, 100, 200);
      cb.Fill();
      cb.SetLineWidth(2);
      cb.Rectangle(x, y, 200, 200);
      cb.Stroke();
    }
// ---------------------------------------------------------------------------
    /**
     * Prints 3 circles in different colors that intersect with eachother.
     * 
     * @param x
     * @param y
     * @param cb
     * @throws Exception
     */
    public static void PictureCircles(float x, float y, PdfContentByte cb) {
      cb.SetColorFill(BaseColor.RED);
      cb.Circle(x + 70, y + 70, 50);
      cb.Fill();
      cb.SetColorFill(BaseColor.YELLOW);
      cb.Circle(x + 100, y + 130, 50);
      cb.Fill();
      cb.SetColorFill(BaseColor.BLUE);
      cb.Circle(x + 130, y + 70, 50);
      cb.Fill();
    }
// ===========================================================================
  }
}