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
  public class AlternatingBackground : IWriter, IPdfPTableEvent {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        List<string> days = PojoFactory.GetDays();
        IPdfPTableEvent Pevent = new AlternatingBackground();
        foreach (string day in days) {
          PdfPTable table = GetTable(day);
          table.TableEvent = Pevent;
          document.Add(table);
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a table with film festival screenings.
     * @param day a film festival day
     * @return a table with screenings.
     */
    public PdfPTable GetTable(string day) {
      PdfPTable table = new PdfPTable(new float[] { 2, 1, 2, 5, 1 });
      table.WidthPercentage = 100f;
      table.DefaultCell.Padding = 3;
      table.DefaultCell.UseAscender = true;
      table.DefaultCell.UseDescender = true;
      table.DefaultCell.Colspan = 5;
      table.DefaultCell.BackgroundColor = BaseColor.RED;
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
      table.AddCell(day);
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
      table.DefaultCell.Colspan = 1;
      table.DefaultCell.BackgroundColor = BaseColor.ORANGE;
      for (int i = 0; i < 2; i++) {
        table.AddCell("Location");
        table.AddCell("Time");
        table.AddCell("Run Length");
        table.AddCell("Title");
        table.AddCell("Year");
      }
      table.DefaultCell.BackgroundColor = null;
      table.HeaderRows = 3;
      table.FooterRows = 1;
      List<Screening> screenings = PojoFactory.GetScreenings(day);
      Movie movie;
      foreach (Screening screening in screenings) {
        movie = screening.movie;
        table.AddCell(screening.Location);
        table.AddCell(screening.Time.Substring(0, 5));
        table.AddCell(movie.Duration.ToString() + " '");
        table.AddCell(movie.MovieTitle);
        table.AddCell(movie.Year.ToString());
      }
      return table;
    }
// ---------------------------------------------------------------------------
    /**
     * Draws a background for every other row.
     * @see com.itextpdf.text.pdf.PdfPTableEvent#tableLayout(
     *      com.itextpdf.text.pdf.PdfPTable, float[][], float[], int, int,
     *      com.itextpdf.text.pdf.PdfContentByte[])
     */
    public void TableLayout(
      PdfPTable table, float[][] widths, float[] heights,
      int headerRows, int rowStart, PdfContentByte[] canvases
    ) {
      int columns;
      Rectangle rect;
      int footer = widths.Length - table.FooterRows;
      int header = table.HeaderRows - table.FooterRows + 1;
      for (int row = header; row < footer; row += 2) {
        columns = widths[row].Length - 1;
        rect = new Rectangle(
          widths[row][0], heights[row],
          widths[row][columns], heights[row + 1]
        );
        rect.BackgroundColor = BaseColor.YELLOW;
        rect.Border = Rectangle.NO_BORDER;
        canvases[PdfPTable.BASECANVAS].Rectangle(rect);
      }
    }    
// ===========================================================================
  }
}