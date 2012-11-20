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
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class PeekABoo : IWriter {
// ===========================================================================
    /** The first resulting PDF. */
    public const String RESULT1 = "peek-a-boo1.pdf";
    /** The second resulting PDF. */
    public const String RESULT2 = "peek-a-boo2.pdf";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        PeekABoo peekaboo = new PeekABoo();
        zip.AddEntry(RESULT1, peekaboo.CreatePdf(true));       
        zip.AddEntry(RESULT2, peekaboo.CreatePdf(false));       
        zip.Save(stream);             
      }
    }    
// ---------------------------------------------------------------------------    
    public byte[] CreatePdf(bool on) {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {    
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.ViewerPreferences = PdfWriter.PageModeUseOC;
          writer.PdfVersion = PdfWriter.VERSION_1_5;        
          // step 3
          document.Open();
          // step 4
          PdfLayer layer = new PdfLayer("Do you see me?", writer);
          layer.On = on;
          BaseFont bf = BaseFont.CreateFont();
          PdfContentByte cb = writer.DirectContent;
          cb.BeginText();
          cb.SetFontAndSize(bf, 18);
          cb.ShowTextAligned(Element.ALIGN_LEFT, "Do you see me?", 50, 790, 0);
          cb.BeginLayer(layer);
          cb.ShowTextAligned(Element.ALIGN_LEFT, "Peek-a-Boo!!!", 50, 766, 0);
          cb.EndLayer();
          cb.EndText();
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}