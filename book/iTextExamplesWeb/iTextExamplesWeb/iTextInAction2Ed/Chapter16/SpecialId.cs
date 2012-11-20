/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class SpecialId : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "special_id.pdf";
    /** An image file. */
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceImage, "bruno.jpg"
    );    
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(RESOURCE, "");
        zip.AddEntry(RESULT, new SpecialId().CreatePdf());       
        zip.Save(stream);             
      }
    } 
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document(new Rectangle(400, 300))) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          Image img = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, "bruno.jpg"
          ));
          img.ScaleAbsolute(400, 300);
          img.SetAbsolutePosition(0, 0);
          PdfImage pi = new PdfImage(img, "", null);
          pi.Put(new PdfName("ITXT_SpecialId"), new PdfName("123456789"));
          PdfIndirectObject pio = writer.AddToBody(pi);
          img.DirectReference = pio.IndirectReference;
          document.Add(img);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}