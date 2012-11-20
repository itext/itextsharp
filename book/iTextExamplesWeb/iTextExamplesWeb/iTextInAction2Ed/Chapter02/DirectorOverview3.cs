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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class DirectorOverview3 : IWriter {
// ===========================================================================
    public const string SQL = 
@"SELECT DISTINCT d.id, d.name, d.given_name, count(*) AS c
FROM film_director d, film_movie_director md
  WHERE d.id = md.director_id
GROUP BY d.id, d.name, d.given_name ORDER BY c DESC";
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
        // creates line separators
        Chunk CONNECT = new Chunk(
          new LineSeparator(0.5f, 95, BaseColor.BLUE, Element.ALIGN_CENTER, 3.5f)
        );
        LineSeparator UNDERLINE = new LineSeparator(
          1, 100, null, Element.ALIGN_CENTER, -2
        );
        // creates tabs
        Chunk tab1 = new Chunk(new VerticalPositionMark(), 200, true);
        Chunk tab2 = new Chunk(new VerticalPositionMark(), 350, true);
        Chunk tab3 = new Chunk(new DottedLineSeparator(), 450, true);
        
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              // loops over the directors
              while (r.Read()) {
                // creates a paragraph with the director name
                director = PojoFactory.GetDirector(r);
                Paragraph p = new Paragraph(
                    PojoToElementFactory.GetDirectorPhrase(director));
                // adds a separator
                p.Add(CONNECT);
                // adds more info about the director
                p.Add(string.Format("movies: {0}", Convert.ToInt32(r["c"])));
                // adds a separator
                p.Add(UNDERLINE);
                // adds the paragraph to the document
                document.Add(p);
                // gets all the movies of the current director
                var director_id = Convert.ToInt32(r["id"]);
                // LINQ allows us to sort on any movie object property inline;
                // let's sort by Movie.Year, Movie.Title
                var by_year = from m in PojoFactory.GetMovies(director_id)
                    orderby m.Year, m.Title
                    select m;
                // loops over the movies
                foreach (Movie movie in by_year) {
                // create a Paragraph with the movie title
                  p = new Paragraph(movie.MovieTitle);
                  // insert a tab
                  p.Add(new Chunk(tab1));
                  // add the original title
                  var mt = movie.OriginalTitle;
                  if (mt != null) p.Add(new Chunk(mt));
                  // insert a tab
                  p.Add(new Chunk(tab2));
                  // add the run length of the movie
                  p.Add(new Chunk(string.Format("{0} minutes", movie.Duration)));
                  // insert a tab
                  p.Add(new Chunk(tab3));
                  // add the production year of the movie
                  p.Add(new Chunk(
                    string.Format("{0}", movie.Year.ToString())
                  ));
                  // add the paragraph to the document
                  document.Add(p);
                }
                document.Add(Chunk.NEWLINE);           
              }
            }
          }
        }
      }
    }
// ===========================================================================
  }
}