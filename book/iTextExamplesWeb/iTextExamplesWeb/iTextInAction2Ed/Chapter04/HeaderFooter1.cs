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
  public class HeaderFooter1 : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        List<string> days = PojoFactory.GetDays();
        foreach (string day in days) {
          document.Add(GetTable(day));
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table with screenings.
     * @param day a film festival day
     * @return a table with screenings
     */
    public PdfPTable GetTable(string day) {
    // Create a table with 7 columns
      PdfPTable table = new PdfPTable(new float[] { 2, 1, 2, 5, 1, 3, 2 });
      table.WidthPercentage = 100f;
      table.DefaultCell.UseAscender = true;
      table.DefaultCell.UseDescender = true;
      // Add the first header row
      Font f = new Font();
      f.Color = BaseColor.WHITE;
      PdfPCell cell = new PdfPCell(new Phrase(day, f));
      cell.BackgroundColor = BaseColor.BLACK;
      cell.HorizontalAlignment = Element.ALIGN_CENTER;
      cell.Colspan = 7;
      table.AddCell(cell);
      // Add the second header row twice
      table.DefaultCell.BackgroundColor = BaseColor.LIGHT_GRAY;
      for (int i = 0; i < 2; i++) {
        table.AddCell("Location");
        table.AddCell("Time");
        table.AddCell("Run Length");
        table.AddCell("Title");
        table.AddCell("Year");
        table.AddCell("Directors");
        table.AddCell("Countries");
      }
      table.DefaultCell.BackgroundColor = null;
      // There are three special rows
      table.HeaderRows = 3;
      // One of them is a footer
      table.FooterRows = 1;
      // Now let's loop over the screenings
      List<Screening> screenings = PojoFactory.GetScreenings(day);
      Movie movie;
      foreach (Screening screening in screenings) {
        movie = screening.movie;
        table.AddCell(screening.Location);
        table.AddCell(screening.Time.Substring(0, 5));
        table.AddCell(movie.Duration.ToString() + " '");
        table.AddCell(movie.MovieTitle);
        table.AddCell(movie.Year.ToString());
        cell = new PdfPCell();
        cell.UseAscender = true;
        cell.UseDescender = true;
        cell.AddElement(PojoToElementFactory.GetDirectorList(movie));
        table.AddCell(cell);
        cell = new PdfPCell();
        cell.UseAscender = true;
        cell.UseDescender = true;
        cell.AddElement(PojoToElementFactory.GetCountryList(movie));
        table.AddCell(cell);
      }
      return table;
    }    
// ===========================================================================
  }
}