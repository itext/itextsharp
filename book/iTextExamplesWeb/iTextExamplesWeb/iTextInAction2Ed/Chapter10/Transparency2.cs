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
  public class Transparency2 : IWriter {
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

        PictureBackdrop(gap, 500, cb, writer);
        PictureBackdrop(200 + 2 * gap, 500, cb, writer);
        PictureBackdrop(gap, 500 - 200 - gap, cb, writer);
        PictureBackdrop(200 + 2 * gap, 500 - 200 - gap, cb, writer);
        PdfTemplate tp;
        PdfTransparencyGroup group;

        tp = cb.CreateTemplate(200, 200);
        PictureCircles(0, 0, tp);
        group = new PdfTransparencyGroup();
        group.Isolated = true;
        group.Knockout = true;
        tp.Group = group;
        cb.AddTemplate(tp, gap, 500);

        tp = cb.CreateTemplate(200, 200);
        PictureCircles(0, 0, tp);
        group = new PdfTransparencyGroup();
        group.Isolated  = true;
        group.Knockout = false;
        tp.Group = group;
        cb.AddTemplate(tp, 200 + 2 * gap, 500);

        tp = cb.CreateTemplate(200, 200);
        PictureCircles(0, 0, tp);
        group = new PdfTransparencyGroup();
        group.Isolated = false;
        group.Knockout = true;
        tp.Group = group;
        cb.AddTemplate(tp, gap, 500 - 200 - gap);

        tp = cb.CreateTemplate(200, 200);
        PictureCircles(0, 0, tp);
        group = new PdfTransparencyGroup();
        group.Isolated = false;
        group.Knockout = false;
        tp.Group = group;
        cb.AddTemplate(tp, 200 + 2 * gap, 500 - 200 - gap);
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
    public static void PictureBackdrop(float x, float y, PdfContentByte cb,
        PdfWriter writer) 
    {
      PdfShading axial = PdfShading.SimpleAxial(writer, x, y, x + 200, y,
        BaseColor.YELLOW, BaseColor.RED
      );
      PdfShadingPattern axialPattern = new PdfShadingPattern(axial);
      cb.SetShadingFill(axialPattern);
      cb.SetColorStroke(BaseColor.BLACK);
      cb.SetLineWidth(2);
      cb.Rectangle(x, y, 200, 200);
      cb.FillStroke();
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
      PdfGState gs = new PdfGState();
      gs.BlendMode = PdfGState.BM_MULTIPLY;
      gs.FillOpacity = 1f;
      cb.SetGState(gs);
      cb.SetColorFill(new CMYKColor(0f, 0f, 0f, 0.15f));
      cb.Circle(x + 75, y + 75, 70);
      cb.Fill();
      cb.Circle(x + 75, y + 125, 70);
      cb.Fill();
      cb.Circle(x + 125, y + 75, 70);
      cb.Fill();
      cb.Circle(x + 125, y + 125, 70);
      cb.Fill();
    }
// ===========================================================================
  }
}