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
  public class MovieAnnotations1 : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "movie_annotations_1.pdf";
    /** Pattern for an info String. */
    public const string INFO = "Movie produced in {0}; run length: {1}";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        foreach (Movie movie in PojoFactory.GetMovies()) {
          document.Add(new Phrase(movie.MovieTitle));
          document.Add(new Annotation(
            movie.Title,
            string.Format(INFO, movie.Year, movie.Duration)
          ));
          document.Add(PojoToElementFactory.GetDirectorList(movie));
          document.Add(PojoToElementFactory.GetCountryList(movie));
        }
      }
    }
// ===========================================================================
  }
}