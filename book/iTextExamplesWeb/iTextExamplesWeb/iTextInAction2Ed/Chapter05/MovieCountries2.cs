/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class MovieCountries2 : MovieCountries1 {
// ===========================================================================
    /**
     * Inner class to add a watermark to every page.
     */
    class Watermark : PdfPageEventHelper {
      Font FONT = new Font(Font.FontFamily.HELVETICA, 52, Font.BOLD, new GrayColor(0.75f));
      
      public override void OnEndPage(PdfWriter writer, Document document) {
        ColumnText.ShowTextAligned(
          writer.DirectContentUnder,
          Element.ALIGN_CENTER, new Phrase("FOOBAR FILM FESTIVAL", FONT),
          297.5f, 421, writer.PageNumber % 2 == 1 ? 45 : -45
        );
      }
    }
// ---------------------------------------------------------------------------
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4, 36, 36, 54, 36)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        TableHeader tevent = new TableHeader();
        writer.PageEvent = tevent;
        writer.PageEvent = new Watermark();
        // step 3
        document.Open();
        // step 4 
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;        
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                tevent.Header = r["country"].ToString();
                foreach (Movie movie 
                  in PojoFactory.GetMovies(r["id"].ToString(), true)) 
                {
                  document.Add(new Paragraph(
                    movie.MovieTitle, FilmFonts.BOLD
                  ));
                  if (!string.IsNullOrEmpty(movie.OriginalTitle)) document.Add(
                    new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC)
                  );
                  document.Add(new Paragraph(
                    String.Format("Year: {0}; run length: {1} minutes",
                      movie.Year, movie.Duration
                    ), FilmFonts.NORMAL
                  ));
                  document.Add(PojoToElementFactory.GetDirectorList(movie));
                }
                document.NewPage();              
              }      
            }
          }
        }
      }
    }    
// ===========================================================================
  }
}