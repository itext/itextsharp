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
using iTextSharp.text.html.simpleparser;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class HtmlMovies2 : HtmlMovies1 {
// ===========================================================================
    public new const string HTML = "movies_2.html";
    /** The resulting PDF file. */
    public const string RESULT1 = "html_movies_2.pdf";
    /** Another resulting PDF file. */
    public const string RESULT2 = "html_movies_3.pdf";  
// ---------------------------------------------------------------------------    
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        HtmlMovies2 movies = new HtmlMovies2();
        // create a StyleSheet
        StyleSheet styles = new StyleSheet();
        styles.LoadTagStyle("ul", "indent", "10");
        styles.LoadTagStyle("li", "leading", "14");
        styles.LoadStyle("country", "i", "");
        styles.LoadStyle("country", "color", "#008080");
        styles.LoadStyle("director", "b", "");
        styles.LoadStyle("director", "color", "midnightblue");
        movies.SetStyles(styles);
        // create extra properties
        Dictionary<String,Object> map = new Dictionary<String, Object>();
        map.Add(HTMLWorker.FONT_PROVIDER, new MyFontFactory());
        map.Add(HTMLWorker.IMG_PROVIDER, new MyImageFactory());
        movies.SetProviders(map);
        // creates HTML and PDF (reusing a method from the super class)
        byte[] pdf = movies.CreateHtmlAndPdf(stream);
        zip.AddEntry(HTML, movies.Html.ToString());
        zip.AddEntry(RESULT1, pdf);
        zip.AddEntry(RESULT2, movies.CreatePdf());
        // add the images so the static html file works
        foreach (Movie movie in PojoFactory.GetMovies()) {
          zip.AddFile(
            Path.Combine(
              Utility.ResourcePosters, 
              string.Format("{0}.jpg", movie.Imdb)
            ), 
            ""
          );
        }
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Inner class implementing the ImageProvider class.
     * This is needed if you want to resolve the paths to images.
     */
    public class MyImageFactory : IImageProvider {
      public Image GetImage(String src, IDictionary<string,string> h, 
        ChainedProperties cprops, IDocListener doc)
      {
        return Image.GetInstance(Path.Combine(
          Utility.ResourcePosters,
          src.Substring(src.LastIndexOf("/") + 1)
        ));
      }    
    }    
// ---------------------------------------------------------------------------    
    /**
     * Inner class implementing the FontProvider class.
     * This is needed if you want to select the correct fonts.
     */
    public class MyFontFactory : IFontProvider {
      public Font GetFont(String fontname, String encoding, bool embedded, 
        float size, int style, BaseColor color) 
      {
        return new Font(Font.FontFamily.TIMES_ROMAN, size, style, color);
      }

      public bool IsRegistered(String fontname) {
        return false;
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          List<IElement> objects = HTMLWorker.ParseToList(
            new StringReader(this.Html.ToString()),
            null, providers
          );
          foreach (IElement element in objects) {
            document.Add(element);
          }
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates an HTML snippet with info about a movie.
     * @param movie the movie for which we want to create HTML
     * @return a String with HTML code
     */
    public override String CreateHtmlSnippet(Movie movie) {
      StringBuilder buf = new StringBuilder("<table width='500'>\n<tr>\n");
      buf.Append("\t<td><img src='");
      buf.Append(movie.Imdb);
      buf.Append(".jpg' /></td>\t<td>\n");
      buf.Append(base.CreateHtmlSnippet(movie));
      buf.Append("\t</ul>\n\t</td>\n</tr>\n</table>");
      return buf.ToString();
    }    
// ===========================================================================
  }
}