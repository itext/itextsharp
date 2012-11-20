/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class MoviePosters1 : IWriter {
// ===========================================================================
    /** Path to IMDB. */
    public const string IMDB = "http://imdb.com/title/tt{0}/";
// ---------------------------------------------------------------------------      
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // Create a reusable XObject
        PdfTemplate celluloid = canvas.CreateTemplate(595, 84.2f);
        celluloid.Rectangle(8, 8, 579, 68);
        for (float f = 8.25f; f < 581; f+= 6.5f) {
          celluloid.RoundRectangle(f, 8.5f, 6, 3, 1.5f);
          celluloid.RoundRectangle(f, 72.5f, 6, 3, 1.5f);
        }
        celluloid.SetGrayFill(0.1f);
        celluloid.EoFill();
        writer.ReleaseTemplate(celluloid);
        // Add the XObject ten times
        for (int i = 0; i < 10; i++) {
          canvas.AddTemplate(celluloid, 0, i * 84.2f);
        }
        // Add the movie posters
        Image img;
        Annotation annotation;
        float x = 11.5f;
        float y = 769.7f;
        string RESOURCE = Utility.ResourcePosters;
        foreach (Movie movie in PojoFactory.GetMovies()) {
          img = Image.GetInstance(Path.Combine(RESOURCE, movie.Imdb + ".jpg"));
          img.ScaleToFit(1000, 60);
          img.SetAbsolutePosition(x + (45 - img.ScaledWidth) / 2, y);
          annotation = new Annotation(
            0, 0, 0, 0,
            string.Format(IMDB, movie.Imdb)
          );
          img.Annotation = annotation;
          canvas.AddImage(img);
          x += 48;
          if (x > 578) {
            x = 11.5f;
            y -= 84.2f;
          }
        }
      }
    }
// ===========================================================================
  }
}