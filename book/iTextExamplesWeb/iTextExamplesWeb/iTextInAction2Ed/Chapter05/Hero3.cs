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
  public class Hero3 : Hero1 {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        Rectangle art = new Rectangle(50, 50, 545, 792);
        writer.SetBoxSize("art", art);        
        // step 3
        document.Open();
        // step 4
        PdfContentByte content = writer.DirectContent;
        PdfTemplate template = CreateTemplate(content, PageSize.A4, 1);
        content.AddTemplate(template, 0, 0);
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
    public override PdfTemplate CreateTemplate(
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
      using ( StreamReader sr = fi.OpenText() ) {
        while (sr.Peek() >= 0) {
          template.SetLiteral((char) sr.Read());
        }
      }
      return template;
    }    
// ===========================================================================
  }
}