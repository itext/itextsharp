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
  public class DirectorOverview2 : IWriter {
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
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                // create a paragraph for the director
                director = PojoFactory.GetDirector(r);
                Paragraph p = new Paragraph(
                    PojoToElementFactory.GetDirectorPhrase(director));
                // add a dotted line separator
                p.Add(new Chunk(new DottedLineSeparator()));
                // adds the number of movies of this director
                p.Add(string.Format("movies: {0}", Convert.ToInt32(r["c"])));
                document.Add(p);
                // Creates a list
                List list = new List(List.ORDERED);
                list.IndentationLeft = 36;
                list.IndentationRight = 36;
                // Gets the movies of the current director
                var director_id = Convert.ToInt32(r["id"]);
                ListItem movieitem;
                // LINQ allows us to on sort any movie object property inline;
                // let's sort by Movie.Year, Movie.Title
                var by_year = from m in PojoFactory.GetMovies(director_id)
                    orderby m.Year, m.Title
                    select m;
                // loops over the movies
                foreach (Movie movie in by_year) {
                // creates a list item with a movie title
                  movieitem = new ListItem(movie.MovieTitle);
                  // adds a vertical position mark as a separator
                  movieitem.Add(new Chunk(new VerticalPositionMark()));
                  var yr = movie.Year;
                  // adds the year the movie was produced
                  movieitem.Add(new Chunk( yr.ToString() ));
                  // add an arrow to the right if the movie dates from 2000 or later
                  if (yr > 1999) {
                    movieitem.Add(PositionedArrow.RIGHT);
                  }
                  // add the list item to the list
                  list.Add(movieitem);
                }
                // add the list to the document
                document.Add(list);           
              }
            }
          }
        }
      }
    }
// ===========================================================================
  }
}