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
  public class MovieCompositeMode : IWriter {
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
        List list;
        PdfPCell cell;
        string RESOURCE = Utility.ResourcePosters;
        foreach (Movie movie in movies) {
      // a table with two columns
          PdfPTable table = new PdfPTable(new float[]{1, 7});
          table.WidthPercentage = 100;
          table.SpacingBefore = 5;
          // a cell with an image
          cell = new PdfPCell(
            Image.GetInstance(Path.Combine(RESOURCE, movie.Imdb + ".jpg")),
            true
          );
          cell.Border = PdfPCell.NO_BORDER;
          table.AddCell(cell);
          cell = new PdfPCell();
          // a cell with paragraphs and lists
          Paragraph p = new Paragraph(movie.Title, FilmFonts.BOLD);
          p.Alignment = Element.ALIGN_CENTER;
          p.SpacingBefore = 5;
          p.SpacingAfter = 5;
          cell.AddElement(p);
          cell.Border = PdfPCell.NO_BORDER;
          if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
            p = new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC);
            p.Alignment = Element.ALIGN_RIGHT;
            cell.AddElement(p);
          }
          list = PojoToElementFactory.GetDirectorList(movie);
          list.IndentationLeft = 30;
          cell.AddElement(list);
          p = new Paragraph(
            string.Format("Year: {0}", movie.Year), 
            FilmFonts.NORMAL
          );
          p.IndentationLeft = 15;
          p.Leading = 24;
          cell.AddElement(p);
          p = new Paragraph(
            string.Format("Run length: {0}", movie.Duration), 
            FilmFonts.NORMAL
          );
          p.Leading = 14;
          p.IndentationLeft = 30;
          cell.AddElement(p);
          list = PojoToElementFactory.GetCountryList(movie);
          list.IndentationLeft = 40;
          cell.AddElement(list);
          table.AddCell(cell);
          // every movie corresponds with one table
          document.Add(table);
          // but the result looks like one big table
        }
      }
    }
// ===========================================================================
  }
}