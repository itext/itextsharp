/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class ChildFieldEvent : IPdfPCellEvent {
// ===========================================================================
    protected PdfFormField parent;
    protected PdfFormField kid;
    protected float padding;
// ---------------------------------------------------------------------------    
    public ChildFieldEvent(
      PdfFormField parent, PdfFormField kid, float padding) 
    {
      this.parent = parent;
      this.kid = kid;
      this.padding = padding;
    }
// ---------------------------------------------------------------------------    
    /**
     * @see com.lowagie.text.pdf.PdfPCellEvent#cellLayout(
     * com.lowagie.text.pdf.PdfPCell,
     * com.lowagie.text.Rectangle, 
     * com.lowagie.text.pdf.PdfContentByte[]
     * )
     */
    public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] cb)
    {
      parent.AddKid(kid);
      kid.SetWidget(
        new Rectangle(rect.GetLeft(padding), rect.GetBottom(padding),
        rect.GetRight(padding), rect.GetTop(padding)),
        PdfAnnotation.HIGHLIGHT_INVERT
      );
    }
// ===========================================================================
  }
}