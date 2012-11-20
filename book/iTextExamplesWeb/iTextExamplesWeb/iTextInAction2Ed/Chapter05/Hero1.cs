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
  public class Hero1 : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      Rectangle rect = new Rectangle(-1192, -1685, 1192, 1685);
      using (Document document = new Document(rect)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte content = writer.DirectContent;
        PdfTemplate template = CreateTemplate(content, rect, 4);
        content.AddTemplate(template, -1192, -1685);
        content.MoveTo(-595, 0);
        content.LineTo(595, 0);
        content.MoveTo(0, -842);
        content.LineTo(0, 842);
        content.Stroke();
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
    public virtual PdfTemplate CreateTemplate(
      PdfContentByte content, Rectangle rect, int factor)
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