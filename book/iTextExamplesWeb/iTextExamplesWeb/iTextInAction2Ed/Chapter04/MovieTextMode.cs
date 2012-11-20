/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class MovieTextMode : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("Movies:"));
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
          PdfPTable table = new PdfPTable(2);
          table.SetWidths(new int[]{1, 4});
          PdfPCell cell;
          cell = new PdfPCell(new Phrase(movie.Title, FilmFonts.BOLD));
          cell.HorizontalAlignment = Element.ALIGN_CENTER;
          cell.Colspan = 2;
          table.AddCell(cell);
          if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
            cell = new PdfPCell(PojoToElementFactory.GetOriginalTitlePhrase(movie));
            cell.Colspan = 2;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            table.AddCell(cell);
          }
          List<Director> directors = movie.Directors;
          cell = new PdfPCell(new Phrase("Directors:"));
          cell.Rowspan = directors.Count;
          cell.VerticalAlignment = Element.ALIGN_MIDDLE;
          table.AddCell(cell);
          int count = 0;
          foreach (Director pojo in directors) {
            cell = new PdfPCell(PojoToElementFactory.GetDirectorPhrase(pojo));
            cell.Indent = (10 * count++);
            table.AddCell(cell);
          }
          table.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
          table.AddCell("Year:");
          table.AddCell(movie.Year.ToString());
          table.AddCell("Run length:");
          table.AddCell(movie.Duration.ToString());
          List<Country> countries = movie.Countries;
          cell = new PdfPCell(new Phrase("Countries:"));
          cell.Rowspan = countries.Count;
          cell.VerticalAlignment = Element.ALIGN_BOTTOM;
          table.AddCell(cell);
          table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
          foreach (Country country in countries) {
            table.AddCell(country.Name);
          }
          document.Add(table);
        }        
      }
    }
// ===========================================================================
  }
}