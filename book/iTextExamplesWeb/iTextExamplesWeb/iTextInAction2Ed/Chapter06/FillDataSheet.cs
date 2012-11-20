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
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;
/*
 * only run on localhost; requires write permissions
 * to the specified directory on a real web server
*/
namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class FillDataSheet : IWriter {
// ===========================================================================
/** The original PDF file. */
    public const string DATASHEET = "datasheet.pdf";
/** Format for resulting PDF files. */
    public const string RESULT = "imdb{0}.pdf";    
// ---------------------------------------------------------------------------    
    public virtual void Write(Stream stream) {  
      using (ZipFile zip = new ZipFile()) { 
        // Get the movies
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        string datasheet = Path.Combine(Utility.ResourcePdf, DATASHEET); 
        string className = this.ToString();            
        // Fill out the data sheet form with data
        foreach (Movie movie in movies) {
          if (movie.Year < 2007) continue;
          PdfReader reader = new PdfReader(datasheet);          
          string dest = string.Format(RESULT, movie.Imdb);
          using (MemoryStream ms = new MemoryStream()) {
            using (PdfStamper stamper = new PdfStamper(reader, ms)) {           
              Fill(stamper.AcroFields, movie);
              if (movie.Year == 2007) stamper.FormFlattening = true;
            }
            zip.AddEntry(dest, ms.ToArray());
          }         
        }
        zip.AddFile(datasheet, "");
        zip.Save(stream);
      }              
    }
// ---------------------------------------------------------------------------    
    /**
     * Fill out the fields using info from a Movie object.
     * @param form The form object
     * @param movie A movie POJO
     */
    public static void Fill(AcroFields form, Movie movie) {
      form.SetField("title", movie.MovieTitle);
      form.SetField("director", GetDirectors(movie));
      form.SetField("year", movie.Year.ToString());
      form.SetField("duration", movie.Duration.ToString());
      form.SetField("category", movie.entry.category.Keyword);
      foreach (Screening screening in movie.entry.Screenings) {
        form.SetField(screening.Location.Replace('.', '_'), "Yes");
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Gets the directors from a Movie object,
     * and concatenates them in a String.
     * @param movie a Movie object
     * @return a String containing director names
     */
    public static String GetDirectors(Movie movie) {
      List<Director> directors = movie.Directors;
      StringBuilder buf = new StringBuilder();
      foreach (Director director in directors) {
        buf.Append(director.GivenName);
        buf.Append(' ');
        buf.Append(director.Name);
        buf.Append(',');
        buf.Append(' ');
      }
      int i = buf.Length;
      if (i > 0) buf.Length = i - 2;
      return buf.ToString();
    }    
// ===========================================================================
  }
}