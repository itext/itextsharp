/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class Hero2 : Hero1 {
// ===========================================================================
    public override void Write(Stream stream) {
      float w = PageSize.A4.Width;
      float h = PageSize.A4.Height;
      Rectangle rect = new Rectangle(-2*w, -2*h, 2*w, 2*h);
      Rectangle crop = new Rectangle(-2 * w, h, -w, 2 * h);      
      // step 1
      using (Document document = new Document(rect)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.CropBoxSize = crop;
        // step 3
        document.Open();
        // step 4
        PdfContentByte content = writer.DirectContent;
        PdfTemplate template = CreateTemplate(content, rect, 4);
        float adjust;
        while(true) {
          content.AddTemplate(template, -2*w, -2*h);
          adjust = crop.Right + w;
          if (adjust > 2 * w) {
            adjust = crop.Bottom - h;
            if (adjust < - 2 * h)
                break;
            crop = new Rectangle(-2*w, adjust, -w, crop.Bottom);
          }
          else {
            crop = new Rectangle(crop.Right, crop.Bottom, adjust, crop.Top);
          }
          writer.CropBoxSize = crop;
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a template based on a stream of PDF syntax.
     * @param content The direct content
     * @param rect The dimension of the templare
     * @param factor A magnification factor
     * @return A PdfTemplate
     */
    public override  PdfTemplate CreateTemplate(
      PdfContentByte content, Rectangle rect, int factor
    )
    {
      PdfTemplate template = content.CreateTemplate(rect.Width, rect.Height);
      template.ConcatCTM(factor, 0, 0, factor, 0, 0);
      string hero = Path.Combine(Utility.ResourceText, "hero.txt");
      if (!File.Exists(hero)) {
        throw new ArgumentException(hero + " NOT FOUND!");
      }   
      var fi = new FileInfo(hero);
      using ( var sr = fi.OpenText() ) {
        while (sr.Peek() >= 0) {
          template.SetLiteral((char) sr.Read());
        }
      }
      return template;
    }    
// ===========================================================================
  }
}