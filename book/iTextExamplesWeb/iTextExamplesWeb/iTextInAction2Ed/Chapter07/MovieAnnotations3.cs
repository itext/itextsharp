/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class MovieAnnotations3 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      MovieAnnotations3 example = new MovieAnnotations3();
      example.CreatePdf(stream);        
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     * @param stream Stream for the new PDF document
     */
    public void CreatePdf(Stream stream) {
      string RESOURCE = Utility.ResourcePosters;
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Phrase phrase;
        Chunk chunk;
        PdfAnnotation annotation;
        foreach (Movie movie in PojoFactory.GetMovies()) {
          phrase = new Phrase(movie.MovieTitle);
          chunk = new Chunk("\u00a0\u00a0");
          annotation = PdfAnnotation.CreateFileAttachment(
            writer, null,
            movie.MovieTitle, null,
            Path.Combine(RESOURCE, movie.Imdb + ".jpg"),
            string.Format("img_{0}.jpg", movie.Imdb)
          );
          annotation.Put(PdfName.NAME, new PdfString("Paperclip"));
          chunk.SetAnnotation(annotation);
          phrase.Add(chunk);
          document.Add(phrase);
          document.Add(PojoToElementFactory.GetDirectorList(movie));
          document.Add(PojoToElementFactory.GetCountryList(movie));
        }
      }
    }
// ===========================================================================
  }
}