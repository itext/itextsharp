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
  public class TemplateClip : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "template_clip.pdf";
    /** One of the resources. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceImage, "bruno_ingeborg.jpg"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TemplateClip t = new TemplateClip();
        zip.AddEntry(RESULT, t.CreatePdf());
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {    
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          Image img = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, RESOURCE)
          );
          float w = img.ScaledWidth;
          float h = img.ScaledHeight;
          PdfTemplate t = writer.DirectContent.CreateTemplate(850, 600);
          t.AddImage(img, w, 0, 0, h, 0, -600);
          Image clipped = Image.GetInstance(t);
          clipped.ScalePercent(50);
          document.Add(clipped);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}