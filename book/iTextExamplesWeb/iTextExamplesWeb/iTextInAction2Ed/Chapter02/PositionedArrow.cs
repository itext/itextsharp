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
 * Subclass of VerticalPositionMark that draws an arrow in the left
 * or right margin.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class PositionedArrow : VerticalPositionMark {
// ===========================================================================
    /** Indicates if the arrow needs to be drawn to the left. */
    protected bool left;
    
    /** Thee font that will be used to draw the arrow. */
    protected BaseFont zapfdingbats;

    /** An arrow pointing to the right will be added on the left. */
    public static PositionedArrow LEFT {
      get { return new PositionedArrow(true); }
    }
    /** An arrow pointing to the left will be added on the right. */
    public static PositionedArrow RIGHT {
      get { return new PositionedArrow(false); }
    }
// ---------------------------------------------------------------------------    
    /**
     * Constructs a positioned Arrow mark.
     * @param    left    if true, an arrow will be drawn on the left;
     * otherwise, it will be drawn on the right.
     * @throws IOException 
     * @throws DocumentException 
     */
    public PositionedArrow(bool left) {
      this.left = left;
      try {
        zapfdingbats = BaseFont.CreateFont(
          BaseFont.ZAPFDINGBATS, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED
        );
      }
      catch {
        zapfdingbats = null;
        throw;
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws a character representing an arrow at the current position.
     * @see com.itextpdf.text.pdf.draw.VerticalPositionMark#draw(
     *      com.itextpdf.text.pdf.PdfContentByte, float, float, float, float, float)
     */
    // i'm so stupid; originally forgot to override parent method and wondered
    // why the arrows weren't being drawn...
    public override void Draw(
      PdfContentByte canvas, float llx, float lly, float urx, float ury, float y
    ) {
      canvas.BeginText();
      canvas.SetFontAndSize(zapfdingbats, 12);
      if (left) {
        canvas.ShowTextAligned(Element.ALIGN_CENTER,
          ((char) 220).ToString(), llx - 10, y, 0
        );
      }
      else {
        canvas.ShowTextAligned(Element.ALIGN_CENTER,
          ((char) 220).ToString(), urx + 10, y + 8, 180
        );
      }
      canvas.EndText();
    }
// ===========================================================================
  }
}