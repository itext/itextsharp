using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class LocalDestinationEvent : IPdfPCellEvent {
// ===========================================================================
    /** The writer to which we are going to add the action. */
    protected PdfWriter writer;
    /** The name of the local destination. */
    protected String name;
// ---------------------------------------------------------------------------    
    /** Creates a new Action event. */
    public LocalDestinationEvent(PdfWriter writer,  String name) {
      this.writer = writer;
      this.name = name;
    }
// ---------------------------------------------------------------------------    
    /** Implementation of the cellLayout method. */
    public void CellLayout(
        PdfPCell cell, Rectangle position, PdfContentByte[] canvases) 
    {
      writer.DirectContent.LocalDestination(
        name,
        new PdfDestination(PdfDestination.FITH, position.Top)
      );
    }
// ===========================================================================
	}
}