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
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MoviePosters3 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      string RESOURCE = Utility.ResourcePosters;
      // step 1
      using (Document document = new Document()) {
        // step 2
        var writer = PdfWriter.GetInstance(document, stream);
        writer.StrictImageSequence = true;
        writer.InitialLeading = 18;
        // step 3
        document.Open();
        Rectangle rect = new Rectangle(0, 806, 36, 842);
        rect.BackgroundColor = BaseColor.RED;
        document.Add(rect);
        // step 4
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
        // Create an image
          Image img = Image.GetInstance(
            Path.Combine(RESOURCE, movie.Imdb + ".jpg")
          );
          img.Alignment = Image.ALIGN_LEFT | Image.TEXTWRAP;
          img.Border = Image.BOX;
          img.BorderWidth = 10;
          img.BorderColor = BaseColor.WHITE;
          img.ScaleToFit(1000, 72);
          document.Add(img);
          // Create text elements
          document.Add(new Paragraph(movie.MovieTitle, FilmFonts.BOLD));
          document.Add(PojoToElementFactory.GetCountryList(movie));
          document.Add(new Paragraph(
            String.Format("Year: {0}", movie.Year)
          ));
          document.Add(new Paragraph(
              String.Format("Duration: {0} minutes", movie.Duration))
          );
          document.Add(new Paragraph("Directors:"));
          document.Add(PojoToElementFactory.GetDirectorList(movie));
          document.Add(Chunk.NEWLINE);
        }
      }
    }
// ===========================================================================
  }
}