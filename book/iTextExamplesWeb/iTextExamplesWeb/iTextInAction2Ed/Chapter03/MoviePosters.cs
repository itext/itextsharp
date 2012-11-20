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

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MoviePosters : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.CompressionLevel = 0;
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // Create the XObject
        PdfTemplate celluloid = canvas.CreateTemplate(595, 84.2f);
        celluloid.Rectangle(8, 8, 579, 68);
        for (float f = 8.25f; f < 581; f+= 6.5f) {
          celluloid.RoundRectangle(f, 8.5f, 6, 3, 1.5f);
          celluloid.RoundRectangle(f, 72.5f, 6, 3, 1.5f);
        }
        celluloid.SetGrayFill(0.1f);
        celluloid.EoFill();
        // Write the XObject to the OutputStream
        writer.ReleaseTemplate(celluloid);
        // Add the XObject 10 times
        for (int i = 0; i < 10; i++) {
          canvas.AddTemplate(celluloid, 0, i * 84.2f);
        }
        // Go to the next page
        document.NewPage();
        // Add the XObject 10 times
        for (int i = 0; i < 10; i++) {
          canvas.AddTemplate(celluloid, 0, i * 84.2f);
        }
        // Get the movies from the database
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        Image img;
        float x = 11.5f;
        float y = 769.7f;
        string RESOURCE = Utility.ResourcePosters;
        // Loop over the movies and add images
        foreach (Movie movie in movies) {
          img = Image.GetInstance(Path.Combine(RESOURCE, movie.Imdb + ".jpg"));
          img.ScaleToFit(1000, 60);
          img.SetAbsolutePosition(x + (45 - img.ScaledWidth) / 2, y);
          canvas.AddImage(img);
          x += 48;
          if (x > 578) {
            x = 11.5f;
            y -= 84.2f;
          }
        }
        // Go to the next page
        document.NewPage();
        // Add the template using a different CTM
        canvas.AddTemplate(celluloid, 0.8f, 0, 0.35f, 0.65f, 0, 600);
        // Wrap the XObject in an Image object
        Image tmpImage = Image.GetInstance(celluloid);
        tmpImage.SetAbsolutePosition(0, 480);
        document.Add(tmpImage);
        // Perform transformations on the image
        tmpImage.RotationDegrees = 30;
        tmpImage.ScalePercent(80);
        tmpImage.SetAbsolutePosition(30, 500);
        document.Add(tmpImage);
        // More transformations
        tmpImage.Rotation = (float)Math.PI / 2;
        tmpImage.SetAbsolutePosition(200, 300);
        document.Add(tmpImage);
      }
    }
// ===========================================================================
  }
}