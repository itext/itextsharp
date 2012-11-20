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

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class ColumnTable : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        ColumnText column = new ColumnText(writer.DirectContent);
        List<string> days = PojoFactory.GetDays();
        // COlumn definition
        float[][] x = {
          new float[] { document.Left, document.Left + 380 },
          new float[] { document.Right - 380, document.Right }
        };
        // Loop over the festival days
        foreach (string day in days) {
        // add content to the column
          column.AddElement(GetTable(day));
          int count = 0;
          float height = 0;
          // iText-ONLY, 'Initial value of the status' => 0
          // iTextSharp **DOES NOT** include this member variable
          // int status = ColumnText.START_COLUMN;
          int status = 0;
          // render the column as long as it has content
          while (ColumnText.HasMoreText(status)) {
          // add the top-level header to each new page
            if (count == 0) {
              height = AddHeaderTable(document, day, writer.PageNumber);
            }
            // set the dimensions of the current column
            column.SetSimpleColumn(
              x[count][0], document.Bottom,
              x[count][1], document.Top - height - 10
            );
            // render as much content as possible
            status = column.Go();
            // go to a new page if you've reached the last column
            if (++count > 1) {
              count = 0;
              document.NewPage();
            }
          }
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Add a header table to the document
     * @param document The document to which you want to add a header table
     * @param day The day that needs to be shown in the header table
     * @param page The page number that has to be shown in the header
     * @return the height of the resulting header table
     */
    public float AddHeaderTable(Document document, string day, int page) {
      PdfPTable header = new PdfPTable(3);
      header.WidthPercentage = 100;
      header.DefaultCell.BackgroundColor = BaseColor.BLACK;
      Font font = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE);
      Phrase p = new Phrase("Foobar Film Festival", font);
      header.AddCell(p);
      header.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
      p = new Phrase(day, font);
      header.AddCell(p);
      header.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
      p = new Phrase(string.Format("page {0}", page), font);
      header.AddCell(p);
      document.Add(header);
      return header.TotalHeight;
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a table with movie screenings for a specific day
     * @param connection a connection to the database
     * @param day a day
     * @return a table with screenings
     */
    public PdfPTable GetTable(string day) {
      PdfPTable table = new PdfPTable(new float[] { 2, 1, 2, 5, 1 });
      table.WidthPercentage = 100f;
      table.DefaultCell.UseAscender = true;
      table.DefaultCell.UseDescender = true;
      table.DefaultCell.BackgroundColor = BaseColor.LIGHT_GRAY;
      for (int i = 0; i < 2; i++) {
        table.AddCell("Location");
        table.AddCell("Time");
        table.AddCell("Run Length");
        table.AddCell("Title");
        table.AddCell("Year");
      }
      table.DefaultCell.BackgroundColor = null;
      table.HeaderRows = 2;
      table.FooterRows = 1;
      List<Screening> screenings = PojoFactory.GetScreenings(day);
      Movie movie;
      foreach (Screening screening in screenings) {
        movie = screening.movie;
        table.AddCell(screening.Location);
        table.AddCell(screening.Time.Substring(0, 5));
        table.AddCell(movie.Duration.ToString());
        table.AddCell(movie.MovieTitle);
        table.AddCell(movie.Year.ToString());
      }
      return table;
    }    
// ===========================================================================
  }
}