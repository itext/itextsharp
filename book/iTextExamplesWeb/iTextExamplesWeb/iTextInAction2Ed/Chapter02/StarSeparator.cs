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
using iTextSharp.text.pdf.draw;

/**
 * does **NOT** write PDF file; class used for drawing only.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class StarSeparator : IDrawInterface {
// ===========================================================================
    /** The font that will be used to draw the arrow. */
    protected BaseFont bf;
// --------------------------------------------------------------------------- 
    public static StarSeparator LINE {
      get { return new StarSeparator(); }
    }
// ---------------------------------------------------------------------------    
    /**
     * Constructs a positioned Arrow mark.
     * @param    left    if true, an arrow will be drawn on the left;
     * otherwise, it will be drawn on the right.
     * @throws IOException 
     * @throws DocumentException 
     */
    public StarSeparator() {
      try {
        bf = BaseFont.CreateFont();
      } catch {
        bf = null;
        throw;
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws three stars to separate two paragraphs.
     * @see com.itextpdf.text.pdf.draw.DrawInterface#draw(
     * com.itextpdf.text.pdf.PdfContentByte, float, float, float, float, float)
     */
    public void Draw(
      PdfContentByte canvas, float llx, float lly, float urx, float ury, float y
    ) {
      float middle = (llx + urx) / 2;
      canvas.BeginText();
      canvas.SetFontAndSize(bf, 10);
      canvas.ShowTextAligned(Element.ALIGN_CENTER, "*", middle, y, 0);
      canvas.ShowTextAligned(Element.ALIGN_CENTER, "*  *", middle, y -10, 0);
      canvas.EndText();
    }
// ===========================================================================
  }
}