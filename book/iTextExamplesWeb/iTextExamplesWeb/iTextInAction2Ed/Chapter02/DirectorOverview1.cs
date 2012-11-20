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
using System.Linq;
using System.util.collections;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class DirectorOverview1 : IWriter {
// ===========================================================================
    public const string SQL = 
@"SELECT DISTINCT d.id, d.name, d.given_name, count(*) AS c 
  FROM film_director d, film_movie_director md 
  WHERE d.id = md.director_id 
  GROUP BY d.id, d.name, d.given_name ORDER BY name";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Director director;
        // creating separators
        LineSeparator line
            = new LineSeparator(1, 100, null, Element.ALIGN_CENTER, -2);
        Paragraph stars = new Paragraph(20);
        stars.Add(new Chunk(StarSeparator.LINE));
        stars.SpacingAfter = 30;
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                // get the director object and use it in a Paragraph
                director = PojoFactory.GetDirector(r);
                Paragraph p = new Paragraph(
                    PojoToElementFactory.GetDirectorPhrase(director));
                // if there are more than 2 movies for this director
                // an arrow is added to the left
                var count = Convert.ToInt32(r["c"]);
                if (count > 2) {
                  p.Add(PositionedArrow.LEFT);
                }
                p.Add(line);
                // add the paragraph with the arrow to the document
                document.Add(p);
                // Get the movies of the director
                var director_id = Convert.ToInt32(r["id"]);
                // LINQ allows us to sort on any movie object property inline;
                // let's sort by Movie.Year, Movie.Title
                var by_year = from m in PojoFactory.GetMovies(director_id)
                    orderby m.Year, m.Title
                    select m;
                foreach (Movie movie in by_year) {
                  p = new Paragraph(movie.MovieTitle);
                  p.Add(": ");
                  p.Add(new Chunk(movie.Year.ToString()));
                  if (movie.Year > 1999) p.Add(PositionedArrow.RIGHT);
                  document.Add(p);
                }                
                // add a star separator after the director info is added
                document.Add(stars);           
              }
            }
          }
        }
      }
    }
// ===========================================================================
  }
}