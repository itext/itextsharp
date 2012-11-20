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
using iTextSharp.text.pdf.draw;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieColumns3 : MovieColumns1 {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        ColumnText ct = new ColumnText(writer.DirectContent);
        int column = 0;
        ct.SetSimpleColumn(
          COLUMNS[column][0], COLUMNS[column][1],
          COLUMNS[column][2], COLUMNS[column][3]
        );
        // iText-ONLY, 'Initial value of the status' => 0
        // iTextSharp **DOES NOT** include this member variable
        // int status = ColumnText.START_COLUMN;
        int status = 0;
        Phrase p;
        float y;        
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
          y = ct.YLine;
          p = CreateMovieInformation(movie);        
          ct.AddText(p);
          status = ct.Go(true);
          if (ColumnText.HasMoreText(status)) {
            column = Math.Abs(column - 1);
            if (column == 0) {
              document.NewPage();
            }
            ct.SetSimpleColumn(
              COLUMNS[column][0], COLUMNS[column][1],
              COLUMNS[column][2], COLUMNS[column][3]
            );
            y = COLUMNS[column][3];
          }
          ct.YLine = y;
          ct.SetText(p);
          status = ct.Go(false);
        }
      }
    }
// ===========================================================================
  }
}