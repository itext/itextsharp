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
  public class Zhang : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // Create a table and fill it with movies
        IEnumerable<Movie> movies = PojoFactory.GetMovies(3);
        PdfPTable table = new PdfPTable(new float[] { 1, 5, 5, 1});
        foreach (Movie movie in movies) {
          table.AddCell(movie.Year.ToString());
          table.AddCell(movie.MovieTitle);
          table.AddCell(movie.OriginalTitle);
          table.AddCell(movie.Duration.ToString());
        }
        // set the total width of the table
        table.TotalWidth = 600;
        PdfContentByte canvas = writer.DirectContent;
        // draw the first three columns on one page
        table.WriteSelectedRows(0, 2, 0, -1, 236, 806, canvas);
        document.NewPage();
        // draw the next three columns on the next page
        table.WriteSelectedRows(2, -1, 0, -1, 36, 806, canvas);
      }
    }
// ===========================================================================
  }
}