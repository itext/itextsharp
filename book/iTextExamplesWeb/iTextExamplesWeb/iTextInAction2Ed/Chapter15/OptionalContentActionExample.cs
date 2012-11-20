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
  public class OptionalContentActionExample : IWriter {
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
        PdfLayer a1 = new PdfLayer("answer 1", writer);
        PdfLayer a2 = new PdfLayer("answer 2", writer);
        PdfLayer a3 = new PdfLayer("answer 3", writer);
        a1.On = false;
        a2.On = false;
        a3.On = false;

        BaseFont bf = BaseFont.CreateFont();
        PdfContentByte cb = writer.DirectContent;
        cb.BeginText();
        cb.SetFontAndSize(bf, 18);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
            "Q1: Who is the director of the movie 'Paths of Glory'?", 50, 766, 0);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
            "Q2: Who directed the movie 'Lawrence of Arabia'?", 50, 718, 0);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
            "Q3: Who is the director of 'House of Flying Daggers'?", 50, 670, 0);
        cb.EndText();
        cb.SaveState();
        cb.SetRGBColorFill(0xFF, 0x00, 0x00);
        cb.BeginText();
        cb.BeginLayer(a1);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
                "A1: Stanley Kubrick", 50, 742, 0);
        cb.EndLayer();
        cb.BeginLayer(a2);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
                "A2: David Lean", 50, 694, 0);
        cb.EndLayer();
        cb.BeginLayer(a3);
        cb.ShowTextAligned(Element.ALIGN_LEFT,
                "A3: Zhang Yimou", 50, 646, 0);
        cb.EndLayer();
        cb.EndText();
        cb.RestoreState();

        List<Object> stateOn = new List<Object>();
        stateOn.Add("ON");
        stateOn.Add(a1);
        stateOn.Add(a2);
        stateOn.Add(a3);
        PdfAction actionOn = PdfAction.SetOCGstate(stateOn, true);
        List<Object> stateOff = new List<Object>();
        stateOff.Add("OFF");
        stateOff.Add(a1);
        stateOff.Add(a2);
        stateOff.Add(a3);
        PdfAction actionOff = PdfAction.SetOCGstate(stateOff, true);
        List<Object> stateToggle = new List<Object>();
        stateToggle.Add("Toggle");
        stateToggle.Add(a1);
        stateToggle.Add(a2);
        stateToggle.Add(a3);
        PdfAction actionToggle = PdfAction.SetOCGstate(stateToggle, true);
        Phrase p = new Phrase("Change the state of the answers:");
        Chunk on = new Chunk(" on ").SetAction(actionOn);
        p.Add(on);
        Chunk off = new Chunk("/ off ").SetAction(actionOff);
        p.Add(off);
        Chunk toggle = new Chunk("/ toggle").SetAction(actionToggle);
        p.Add(toggle);
        document.Add(p);
      }
    }
// ===========================================================================
  }
}                                 
