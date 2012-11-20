using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class PdfActionEvent : IPdfPCellEvent {
// ===========================================================================
    /** The writer to which we are going to add the action. */
    protected PdfWriter writer;
    /** The action we're going to add. */
    protected PdfAction action;
// ---------------------------------------------------------------------------    
    /** Creates a new Action event. */
    public PdfActionEvent(PdfWriter writer, PdfAction action) {
      this.writer = writer;
      this.action = action;
    }
// ---------------------------------------------------------------------------    
    /** Implementation of the cellLayout method. */
    public void CellLayout(
        PdfPCell cell, Rectangle position, PdfContentByte[] canvases) 
    {
      writer.AddAnnotation(new PdfAnnotation(
        writer,
        position.Left, position.Bottom, position.Right, position.Top,
        action
      ));
    }
// ===========================================================================
	}
}
