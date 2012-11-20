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
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class LayerMembershipExample2 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PdfVersion = PdfWriter.VERSION_1_6;
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;

        PdfLayer dog = new PdfLayer("layer 1", writer);
        PdfLayer tiger = new PdfLayer("layer 2", writer);
        PdfLayer lion = new PdfLayer("layer 3", writer);
        PdfLayerMembership cat = new PdfLayerMembership(writer);
        PdfVisibilityExpression ve1 = new PdfVisibilityExpression(
          PdfVisibilityExpression.OR
        );
        ve1.Add(tiger);
        ve1.Add(lion);
        cat.VisibilityExpression = ve1;
        PdfLayerMembership no_cat = new PdfLayerMembership(writer);
        PdfVisibilityExpression ve2 = new PdfVisibilityExpression(
          PdfVisibilityExpression.NOT
        );
        ve2.Add(ve1);
        no_cat.VisibilityExpression = ve2;
        cb.BeginLayer(dog);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("dog"),
                50, 775, 0);
        cb.EndLayer();
        cb.BeginLayer(tiger);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("tiger"),
                50, 750, 0);
        cb.EndLayer();
        cb.BeginLayer(lion);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("lion"),
                50, 725, 0);
        cb.EndLayer();
        cb.BeginLayer(cat);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("cat"),
                50, 700, 0);
        cb.EndLayer();
        cb.BeginLayer(no_cat);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
                new Phrase("no cat"), 50, 700, 0);
        cb.EndLayer();
      }
    }
// ===========================================================================
  }
}