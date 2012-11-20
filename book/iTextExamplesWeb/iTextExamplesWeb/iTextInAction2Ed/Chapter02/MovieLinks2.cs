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

/**
 * We'll test our SQLite database with this example
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MovieLinks2 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // Create a local destination at the top of the page
        Paragraph p = new Paragraph();
        Chunk top = new Chunk("Country List", FilmFonts.BOLD);
        top.SetLocalDestination("top");
        p.Add(top);
        document.Add(p);
        // create an external link
        Chunk imdb = new Chunk("Internet Movie Database", FilmFonts.ITALIC);
        imdb.SetAnchor(new Uri("http://www.imdb.com/"));
        p = new Paragraph(
          "Click on a country, and you'll get a list of movies, containing links to the "
        );
        p.Add(imdb);
        p.Add(".");
        document.Add(p);
        // Create a remote goto
        p = new Paragraph("This list can be found in a ");
        Chunk page1 = new Chunk("separate document");
        page1.SetRemoteGoto("movie_links_1.pdf", 1);
        p.Add(page1);
        p.Add(".");
        document.Add(p);
        document.Add(Chunk.NEWLINE);
        
        var SQL =
@"SELECT DISTINCT mc.country_id, c.country, count(*) AS c 
FROM film_country c, film_movie_country mc
WHERE c.id = mc.country_id 
GROUP BY mc.country_id, country ORDER BY c DESC";        
        // Create a database connection and statement
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;        
            c.Open();            
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
              // add country with remote goto
                Paragraph country = new Paragraph(r["country"].ToString());
                country.Add(": ");
                Chunk link = new Chunk(string.Format(
                  "{0} movies", Convert.ToInt32(r["c"])
                ));
                link.SetRemoteGoto(
                  "movie_links_1.pdf", r["country_id"].ToString()
                );
                country.Add(link);
                document.Add(country);
              }
            }
          }
        }      
        document.Add(Chunk.NEWLINE);
        // Create local goto to top
        p = new Paragraph("Go to ");
        top = new Chunk("top");
        top.SetLocalGoto("top");
        p.Add(top);
        p.Add(".");
        document.Add(p);        
      }
    }
// ===========================================================================
  }
}