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
  public class OptionalContentExample : IWriter {
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
        PdfLayer nested = new PdfLayer("Nested layers", writer);
        PdfLayer nested_1 = new PdfLayer("Nested layer 1", writer);
        PdfLayer nested_2 = new PdfLayer("Nested layer 2", writer);
        nested.AddChild(nested_1);
        nested.AddChild(nested_2);
        writer.LockLayer(nested_2);
        cb.BeginLayer(nested);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
          new Phrase("nested layers"), 50, 775, 0
        );
        cb.EndLayer();
        cb.BeginLayer(nested_1);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT,
          new Phrase("nested layer 1"), 100, 800, 0
        );
        cb.EndLayer();
        cb.BeginLayer(nested_2);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("nested layer 2"), 100, 750, 0
        );
        cb.EndLayer();

        PdfLayer group = PdfLayer.CreateTitle("Grouped layers", writer);
        PdfLayer layer1 = new PdfLayer("Group: layer 1", writer);
        PdfLayer layer2 = new PdfLayer("Group: layer 2", writer);
        group.AddChild(layer1);
        group.AddChild(layer2);
        cb.BeginLayer(layer1);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase( "layer 1 in the group"), 50, 700, 0
        );
        cb.EndLayer();
        cb.BeginLayer(layer2);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("layer 2 in the group"), 50, 675, 0
        );
        cb.EndLayer();

        PdfLayer radiogroup = PdfLayer.CreateTitle("Radio group", writer);
        PdfLayer radio1 = new PdfLayer("Radiogroup: layer 1", writer);
        radio1.On = true;
        PdfLayer radio2 = new PdfLayer("Radiogroup: layer 2", writer);
        radio2.On = false;
        PdfLayer radio3 = new PdfLayer("Radiogroup: layer 3", writer);
        radio3.On = false;
        radiogroup.AddChild(radio1);
        radiogroup.AddChild(radio2);
        radiogroup.AddChild(radio3);
        List<PdfLayer> options = new List<PdfLayer>();
        options.Add(radio1);
        options.Add(radio2);
        options.Add(radio3);
        writer.AddOCGRadioGroup(options);
        cb.BeginLayer(radio1);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("option 1"), 50, 600, 0
        );
        cb.EndLayer();
        cb.BeginLayer(radio2);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase("option 2"), 50, 575, 0
        );
        cb.EndLayer();
        cb.BeginLayer(radio3);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, 
          new Phrase(
                "option 3"
          ), 50, 550, 0
        );
        cb.EndLayer();

        PdfLayer not_printed = new PdfLayer("not printed", writer);
        not_printed.OnPanel = false;
        not_printed.SetPrint("Print", false);
        cb.BeginLayer(not_printed);
        ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, 
          new Phrase(
                "PRINT THIS PAGE"
          ), 300, 700, 90
        );
        cb.EndLayer();

        PdfLayer zoom = new PdfLayer("Zoom 0.75-1.25", writer);
        zoom.OnPanel = false;
        zoom.SetZoom(0.75f, 1.25f);
        cb.BeginLayer(zoom);
        ColumnText.ShowTextAligned(
          cb, Element.ALIGN_LEFT, 
          new Phrase(
            "Only visible if the zoomfactor is between 75 and 125%"
          ), 30, 530, 90
        );
        cb.EndLayer();
      }
    }
// ===========================================================================
  }
}