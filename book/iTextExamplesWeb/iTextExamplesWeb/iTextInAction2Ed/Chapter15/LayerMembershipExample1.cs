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

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class LayerMembershipExample1 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PdfVersion = PdfWriter.VERSION_1_5;
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;

        PdfLayer dog = new PdfLayer("layer 1", writer);
        PdfLayer tiger = new PdfLayer("layer 2", writer);
        PdfLayer lion = new PdfLayer("layer 3", writer);
        PdfLayerMembership cat = new PdfLayerMembership(writer);
        cat.AddMember(tiger);
        cat.AddMember(lion);
        PdfLayerMembership no_cat = new PdfLayerMembership(writer);
        no_cat.AddMember(tiger);
        no_cat.AddMember(lion);
        no_cat.VisibilityPolicy = PdfLayerMembership.ALLOFF;
        cb.BeginLayer(dog);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("dog"), 50, 775, 0
        );
        cb.EndLayer();
        cb.BeginLayer(tiger);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("tiger"), 50, 750, 0
        );
        cb.EndLayer();
        cb.BeginLayer(lion);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("lion"), 50, 725, 0
        );
        cb.EndLayer();
        cb.BeginLayer(cat);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("cat"), 50, 700, 0
        );
        cb.EndLayer();
        cb.BeginLayer(no_cat);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
          new Phrase("no cat"), 50, 700, 0
        );
        cb.EndLayer();
      }
    }
// ===========================================================================
  }
}