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
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter12 {
  public class HelloWorldCompression : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "compression_not_at_all.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "compression_zero.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "compression_normal.pdf";
    /** The resulting PDF file. */
    public const string RESULT4 = "compression_high.pdf";
    /** The resulting PDF file. */
    public const string RESULT5 = "compression_full.pdf";
    /** The resulting PDF file. */
    public const string RESULT6 = "compression_full_too.pdf";
    public const string RESULT7 = "compression_removed.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        HelloWorldCompression hello = new HelloWorldCompression();
        zip.AddEntry(RESULT1, hello.CreatePdf(-1));
        byte[] compress0 = hello.CreatePdf(0);
        zip.AddEntry(RESULT2, compress0);
        zip.AddEntry(RESULT3, hello.CreatePdf(1));
        zip.AddEntry(RESULT4, hello.CreatePdf(2));
        zip.AddEntry(RESULT5, hello.CreatePdf(3));
        byte[] compress6 = hello.CompressPdf(compress0);
        zip.AddEntry(RESULT6, compress6);
        zip.AddEntry(RESULT7, hello.DecompressPdf(compress6));
        zip.Save(stream);
      }    
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF with information about the movies
     * @param    filename the name of the PDF file that will be created.
     */
    public byte[] CreatePdf(int compression) {
      using (MemoryStream ms = new MemoryStream()) {
      // step 1
        using (Document document = new Document()) {
        // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          switch(compression) {
            case -1:
              Document.Compress = false;
              break;
            case 0:
              writer.CompressionLevel = 0;
              break;
            case 2:
              writer.CompressionLevel = 9;
              break;
            case 3:
              writer.SetFullCompression();
              break;
          }
          // step 3
          document.Open();
        // step 4
        // Create database connection and statement
          var SQL = 
@"SELECT DISTINCT mc.country_id, c.country, count(*) AS c 
FROM film_country c, film_movie_country mc 
  WHERE c.id = mc.country_id
GROUP BY mc.country_id, country ORDER BY c DESC";
        // Create a new list
          List list = new List(List.ORDERED);
          DbProviderFactory dbp = AdoDB.Provider;
          using (var c = dbp.CreateConnection()) {
            c.ConnectionString = AdoDB.CS;
            using (DbCommand cmd = c.CreateCommand()) {
              cmd.CommandText = SQL;
              c.Open();            
              using (var r = cmd.ExecuteReader()) {
                while (r.Read()) {
                // create a list item for the country
                  ListItem item = new ListItem(
                    String.Format("{0}: {1} movies", r["country"], r["c"]),
                    FilmFonts.BOLDITALIC
                  );
                  // create a movie list for each country
                  List movielist = new List(List.ORDERED, List.ALPHABETICAL);
                  movielist.Lowercase = List.LOWERCASE;
                  foreach (Movie movie in 
                      PojoFactory.GetMovies(r["country_id"].ToString())) 
                  {
                    ListItem movieitem = new ListItem(movie.MovieTitle);
                    List directorlist = new List(List.UNORDERED);
                    foreach (Director director in movie.Directors) {
                      directorlist.Add(String.Format(
                        "{0}, {1}", director.Name, director.GivenName
                      ));
                    }
                    movieitem.Add(directorlist);
                    movielist.Add(movieitem);
                  }
                  item.Add(movielist);
                  list.Add(item);
                }
              }
            }
          }
          document.Add(list);
        }
        Document.Compress = true; 
        return ms.ToArray();
      }     
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] CompressPdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = 
            new PdfStamper(reader, ms, PdfWriter.VERSION_1_5))
        {
          stamper.Writer.CompressionLevel = 9;
          int total = reader.NumberOfPages + 1;
          for (int i = 1; i < total; i++) {
            reader.SetPageContent(i, reader.GetPageContent(i));
          }
          stamper.SetFullCompression();
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] DecompressPdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          Document.Compress = false;
          int total = reader.NumberOfPages + 1;
          for (int i = 1; i < total; i++) {
            reader.SetPageContent(i, reader.GetPageContent(i));
          }
        }
        Document.Compress = true;
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}