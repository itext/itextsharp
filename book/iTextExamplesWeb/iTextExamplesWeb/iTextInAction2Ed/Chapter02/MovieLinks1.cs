/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MovieLinks1 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      var SQL = 
@"SELECT DISTINCT mc.country_id, c.country, count(*) AS c 
FROM film_country c, film_movie_country mc 
  WHERE c.id = mc.country_id
GROUP BY mc.country_id, country ORDER BY c DESC";          
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
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
                Paragraph country = new Paragraph();
                // the name of the country will be a destination
                Anchor dest = new Anchor(
                  r["country"].ToString(), FilmFonts.BOLD
                );
                dest.Name = r["country_id"].ToString();
                country.Add(dest);
                country.Add(string.Format(": {0} movies", r["c"].ToString()));
                document.Add(country);
                // loop over the movies
                foreach (Movie movie in 
                    PojoFactory.GetMovies(r["country_id"].ToString()))
                {
                // the movie title will be an external link
                  Anchor imdb = new Anchor(movie.MovieTitle);
                  imdb.Reference = string.Format(
                    "http://www.imdb.com/title/tt{0}/", movie.Imdb
                  );
                  document.Add(imdb);
                  document.Add(Chunk.NEWLINE);
                }
                document.NewPage();
              }
              // Create an internal link to the first page
              Anchor toUS = new Anchor("Go back to the first page.");
              toUS.Reference = "#US";
              document.Add(toUS);           
            }
          }
        }
      }
    }
// ===========================================================================
  }
}