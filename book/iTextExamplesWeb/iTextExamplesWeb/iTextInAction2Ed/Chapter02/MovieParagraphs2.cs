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

/**
 * Writes a list of countries to a PDF file.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MovieParagraphs2 : MovieParagraphs1 {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
        // Create a paragraph with the title
          Paragraph title = new Paragraph(
            PojoToElementFactory.GetMovieTitlePhrase(movie)
          );
          title.Alignment = Element.ALIGN_LEFT;
          document.Add(title);
          // Add the original title next to it using a dirty hack
          if ( !string.IsNullOrEmpty(movie.OriginalTitle) ) {
            Paragraph dummy = new Paragraph("\u00a0", FilmFonts.NORMAL);
            dummy.Leading = -18;
            document.Add(dummy);
            Paragraph originalTitle = new Paragraph(
              PojoToElementFactory.GetOriginalTitlePhrase(movie)
            );
            originalTitle.Alignment = Element.ALIGN_RIGHT;
            document.Add(originalTitle);
          }
          // Info about the director
          float indent = 20;
          // Loop over the directors
          foreach (Director pojo in movie.Directors) {
            Paragraph director = new Paragraph(
              PojoToElementFactory.GetDirectorPhrase(pojo)
            );
            director.IndentationLeft = indent;
            document.Add(director);
            indent += 20;
          }
          // Info about the country
          indent = 20;
          // Loop over the countries
          foreach (Country pojo in movie.Countries) {
            Paragraph country = new Paragraph(
              PojoToElementFactory.GetCountryPhrase(pojo)
            );
            country.Alignment = Element.ALIGN_RIGHT;
            country.IndentationRight = indent;
            document.Add(country);
            indent += 20;
          }
          // Extra info about the movie
          Paragraph info = CreateYearAndDuration(movie);
          info.Alignment = Element.ALIGN_CENTER;
          info.SpacingAfter = 36;
          document.Add(info);
        }      
      }
    }
// ===========================================================================
  }
}