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
  public class MovieAnnotations2 : IWriter {
// ===========================================================================
    /** Pattern for an info String. */
    public const string INFO = "Movie produced in {0}; run length: {1}";
// ---------------------------------------------------------------------------          
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Phrase phrase;
        Chunk chunk;        
        foreach (Movie movie in PojoFactory.GetMovies()) {
          phrase = new Phrase(movie.MovieTitle);
          chunk = new Chunk("\u00a0");
          chunk.SetAnnotation( PdfAnnotation.CreateText(
            writer, null, movie.MovieTitle,
            string.Format(INFO, movie.Year, movie.Duration),
            false, "Comment"
          ));
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