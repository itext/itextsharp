/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class PressPreviews : IWriter, IPdfPCellEvent, IPdfPTableEvent {
// ===========================================================================
    /**
     * @see com.itextpdf.text.pdf.PdfPTableEvent#tableLayout(com.itextpdf.text.pdf.PdfPTable,
     *      float[][], float[], int, int, com.itextpdf.text.pdf.PdfContentByte[])
     */
    public void TableLayout(
      PdfPTable table, float[][] width, float[] height,
      int headerRows, int rowStart, PdfContentByte[] canvas
    ) {
      float[] widths = width[0];
      float x1 = widths[0];
      float x2 = widths[widths.Length - 1];
      float y1 = height[0];
      float y2 = height[height.Length - 1];
      PdfContentByte cb = canvas[PdfPTable.LINECANVAS];
      cb.Rectangle(x1, y1, x2 - x1, y2 - y1);
      cb.Stroke();
      cb.ResetRGBColorStroke();
    }

    /**
     * @see com.lowagie.text.pdf.PdfPCellEvent#cellLayout(com.lowagie.text.pdf.PdfPCell,
     *      com.lowagie.text.Rectangle, com.lowagie.text.pdf.PdfContentByte[])
     */
    public void CellLayout(
      PdfPCell cell, Rectangle position, PdfContentByte[] canvases
    ) {
      float x1 = position.Left + 2;
      float x2 = position.Right - 2;
      float y1 = position.Top - 2;
      float y2 = position.Bottom + 2;
      PdfContentByte canvas = canvases[PdfPTable.LINECANVAS];
      canvas.Rectangle(x1, y1, x2 - x1, y2 - y1);
      canvas.Stroke();
      canvas.ResetRGBColorStroke();
    }
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(GetTable()); 
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a table that mimics cellspacing using table and cell events.
     * @param connection
     * @return a table
     * @throws SQLException
     * @throws DocumentException
     * @throws IOException
     */
    public PdfPTable GetTable() {
      PdfPTable table = new PdfPTable(new float[] { 1, 2, 2, 5, 1 });
      table.TableEvent = new PressPreviews();
      table.WidthPercentage = 100f;
      table.DefaultCell.Padding = 5;
      table.DefaultCell.Border = PdfPCell.NO_BORDER;
      table.DefaultCell.CellEvent = new PressPreviews();
      for (int i = 0; i < 2; i++) {
        table.AddCell("Location");
        table.AddCell("Date/Time");
        table.AddCell("Run Length");
        table.AddCell("Title");
        table.AddCell("Year");
      }
      table.DefaultCell.BackgroundColor = null;
      table.HeaderRows = 2;
      table.FooterRows = 1;
      List<Screening> screenings = PojoFactory.GetPressPreviews();
      foreach (Screening screening in screenings) {
        Movie movie = screening.movie;
        table.AddCell(screening.Location);
        table.AddCell(String.Format(
          "{0}   {1}", screening.Date, 
          screening.Time.Substring(0, 5)
        ));
        table.AddCell(String.Format("{0} '", movie.Duration));
        table.AddCell(movie.MovieTitle);
        table.AddCell(movie.Year.ToString());
      }
      return table;
    }    
// ===========================================================================
  }
}