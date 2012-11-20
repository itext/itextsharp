/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class MoviePosters2 : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "movie_posters_2.pdf";
    /** A pattern for an info string. */
    public const string INFO = "Movie produced in {0}; run length: {1}";
    /** A JavaScript snippet */
    public const string JS1 =
      "var t = this.getAnnot(this.pageNum, 'IMDB{0}'); t.popupOpen = true; "
    + "var w = this.getField('b{0}'); w.Focus();";
    /** A JavaScript snippet */
    public const string JS2 =
      "var t = this.getAnnot(this.pageNum, 'IMDB{0}'); t.popupOpen = false;";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MoviePosters1 m = new MoviePosters1(); 
        byte[] pdf = Utility.PdfBytes(m);
        MoviePosters2 m2 = new MoviePosters2();
        zip.AddEntry(RESULT, m2.ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      // Create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {           
          Image img;
          float x = 11.5f;
          float y = 769.7f;
          float llx, lly, urx, ury;
          string RESOURCE = Utility.ResourcePosters;
          // Loop over all the movies to add a popup annotation
          foreach (Movie movie in PojoFactory.GetMovies()) {
            img = Image.GetInstance(Path.Combine(RESOURCE, movie.Imdb + ".jpg"));
            img.ScaleToFit(1000, 60);
            llx = x + (45 - img.ScaledWidth) / 2;
            lly = y;
            urx = x + img.ScaledWidth;
            ury = y + img.ScaledHeight;
            AddPopup(stamper, new Rectangle(llx, lly, urx, ury),
              movie.MovieTitle, 
              string.Format(INFO, movie.Year, movie.Duration), 
              movie.Imdb
            );
            x += 48;
            if (x > 578) {
              x = 11.5f;
              y -= 84.2f;
            }
          }
        }
        return ms.ToArray();
      }
    }  
// ---------------------------------------------------------------------------
    /**
     * Adds a popup.
     * @param stamper the PdfStamper to which the annotation needs to be added
     * @param rect the position of the annotation
     * @param title the annotation title
     * @param contents the annotation content
     * @param imdb the IMDB number of the movie used as name of the annotation
     */
    public void AddPopup(PdfStamper stamper, Rectangle rect,
      String title, String contents, String imdb)
    {
  // Create the text annotation
      PdfAnnotation text = PdfAnnotation.CreateText(
        stamper.Writer,
        rect, title, contents, false, "Comment"
      );
      text.Name = string.Format("IMDB{0}", imdb);
      text.Flags = PdfAnnotation.FLAGS_READONLY | PdfAnnotation.FLAGS_NOVIEW;
      // Create the popup annotation
      PdfAnnotation popup = PdfAnnotation.CreatePopup(
        stamper.Writer,
        new Rectangle(
          rect.Left + 10, rect.Bottom + 10,
          rect.Left + 200, rect.Bottom + 100
        ), 
        null, false
      );
      // Add the text annotation to the popup
      popup.Put(PdfName.PARENT, text.IndirectReference);
      // Declare the popup annotation as popup for the text
      text.Put(PdfName.POPUP, popup.IndirectReference);
      // Add both annotations
      stamper.AddAnnotation(text, 1);
      stamper.AddAnnotation(popup, 1);
      // Create a button field
      PushbuttonField field = new PushbuttonField(
        stamper.Writer, rect,
        string.Format("b{0}", imdb)
      );
      PdfAnnotation widget = field.Field;
      // Show the popup onMouseEnter
      PdfAction enter = PdfAction.JavaScript(
        string.Format(JS1, imdb), stamper.Writer
      );
      widget.SetAdditionalActions(PdfName.E, enter);
      // Hide the popup onMouseExit
      PdfAction exit = PdfAction.JavaScript(
        string.Format(JS2, imdb), stamper.Writer
      );
      widget.SetAdditionalActions(PdfName.X, exit);
      // Add the button annotation
      stamper.AddAnnotation(widget, 1);
    }
// ===========================================================================
  }
}