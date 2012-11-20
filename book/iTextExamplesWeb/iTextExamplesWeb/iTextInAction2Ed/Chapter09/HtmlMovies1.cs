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
using System.Web;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class HtmlMovies1 : IWriter {
// ===========================================================================
    /** The resulting HTML file. */
    public const string HTML = "movies_1.html";
    /** The resulting PDF file. */
    public const string PDF = "html_movies_1.pdf";
    protected StringBuilder Html = new StringBuilder();
    
    /** The StyleSheet. */
    protected StyleSheet styles = null;
    /** Extra properties. */
    protected Dictionary<string, object> providers = null;
// ---------------------------------------------------------------------------    
    public virtual void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        HtmlMovies1 h = new HtmlMovies1();
        byte[] pdf = h.CreateHtmlAndPdf(stream);
        zip.AddEntry(HTML, h.Html.ToString());
        zip.AddEntry(PDF, pdf);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * Creates a list with movies in HTML and PDF simultaneously.
     * @param pdf a path to the resulting PDF file
     * @param html a path to the resulting HTML file
     */
    public byte[] CreateHtmlAndPdf(Stream stream) {
      // store the HTML in a string and dump on separate page
      Html.Append("<html>\n<body>");
      Html.Append(Environment.NewLine);
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          String snippet;
          foreach (Movie movie in PojoFactory.GetMovies()) {
            // create the snippet
            snippet = CreateHtmlSnippet(movie);
            // use the snippet for the HTML page
            Html.Append(snippet);
            // use the snippet for the PDF document
            List<IElement> objects = HTMLWorker.ParseToList(
              new StringReader(snippet), styles, providers
            );
            foreach (IElement element in objects) document.Add(element);
          }
        }
        Html.Append("</body>\n</html>");
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * Sets the styles for the HTML to PDF conversion
     * @param styles a StyleSheet object
     */
    public void SetStyles(StyleSheet styles) {
      this.styles = styles;
    }
// ---------------------------------------------------------------------------    
    /**
     * Set some extra properties.
     * @param providers the properties map
     */
    public void SetProviders(Dictionary<String, Object> providers) {
      this.providers = providers;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates an HTML snippet with info about a movie.
     * @param movie the movie for which we want to create HTML
     * @return a String with HTML code
     */
    public virtual String CreateHtmlSnippet(Movie movie) {
      StringBuilder buf = new StringBuilder("\t<span class='title'>");
      buf.Append(movie.MovieTitle);
      buf.Append("</span><br />\n");
      buf.Append("\t<ul>\n");
      foreach (Country country in movie.Countries) {
        buf.Append("\t\t<li class='country'>");
        buf.Append(country.Name);
        buf.Append("</li>\n");
      }
      buf.Append("\t</ul>\n");
      buf.Append("\tYear: <i>");
      buf.Append(movie.Year.ToString());
      buf.Append(" minutes</i><br />\n");
      buf.Append("\tDuration: <i>");
      buf.Append(movie.Duration.ToString());
      buf.Append(" minutes</i><br />\n");
      buf.Append("\t<ul>\n");
      foreach (Director director in movie.Directors) {
        buf.Append("\t\t<li><span class='director'>");
        buf.Append(director.Name);
        buf.Append(", ");
        buf.Append(director.GivenName);
        buf.Append("</span></li>\n");
      }
      buf.Append("\t</ul>\n");
      return buf.ToString();
    }   
// ===========================================================================
  }
}