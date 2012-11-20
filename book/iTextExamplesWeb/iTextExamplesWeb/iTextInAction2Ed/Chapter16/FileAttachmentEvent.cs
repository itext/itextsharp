using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class FileAttachmentEvent : IPdfPCellEvent {
// ===========================================================================
    /** The writer to which we are going to add the action. */
    protected PdfWriter writer;
    /** The file specification that will be used to create an annotation. */
    protected PdfFileSpecification fs;
    /** The description that comes with the annotation. */
    protected String description;
// ---------------------------------------------------------------------------    
    /**
     * Creates a FileAttachmentEvent.
     * 
     * @param writer      the writer to which the file attachment has to be added.
     * @param fs          the file specification.
     * @param description a description for the file attachment.
     */
    public FileAttachmentEvent(
        PdfWriter writer, PdfFileSpecification fs, String description)
    {
      this.writer = writer;
      this.fs = fs;
      this.description = description;
    }
// ---------------------------------------------------------------------------    
    /**
     * Implementation of the cellLayout method.
     * @see com.itextpdf.text.pdf.PdfPCellEvent#cellLayout(
     * com.itextpdf.text.pdf.PdfPCell, com.itextpdf.text.Rectangle, 
     * com.itextpdf.text.pdf.PdfContentByte[])
     */
    public void CellLayout(
        PdfPCell cell, Rectangle position, PdfContentByte[] canvases) 
    {
      try {
        PdfAnnotation annotation = PdfAnnotation.CreateFileAttachment(
          writer,
          new Rectangle(
            position.Left - 20, position.Top - 15,
            position.Left - 5, position.Top - 5
          ),
          description, fs
        );
        annotation.Name = description;
        writer.AddAnnotation(annotation);
      }
      catch {
        throw;
      }
    }
// ===========================================================================
	}
}