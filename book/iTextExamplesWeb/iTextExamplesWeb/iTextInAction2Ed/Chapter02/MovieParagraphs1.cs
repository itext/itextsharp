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
  public class MovieParagraphs1 : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
          Paragraph p = CreateMovieInformation(movie);
          p.Alignment = Element.ALIGN_JUSTIFIED;
          p.IndentationLeft = 18;
          p.FirstLineIndent = -18;
          document.Add(p);
        }
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a Paragraph containing information about a movie.
     * @param    movie    the movie for which you want to create a Paragraph
     */
    public Paragraph CreateMovieInformation(Movie movie) {
      Paragraph p = new Paragraph();
      p.Font = FilmFonts.NORMAL;
      p.Add(new Phrase("Title: ", FilmFonts.BOLDITALIC));
      p.Add(PojoToElementFactory.GetMovieTitlePhrase(movie));
      p.Add(" ");
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        p.Add(new Phrase("Original title: ", FilmFonts.BOLDITALIC));
        p.Add(PojoToElementFactory.GetOriginalTitlePhrase(movie));
        p.Add(" ");
      }
      p.Add(new Phrase("Country: ", FilmFonts.BOLDITALIC));
      foreach (Country country in movie.Countries) {
        p.Add(PojoToElementFactory.GetCountryPhrase(country));
        p.Add(" ");
      }
      p.Add(new Phrase("Director: ", FilmFonts.BOLDITALIC));
      foreach (Director director in movie.Directors) {
        p.Add(PojoToElementFactory.GetDirectorPhrase(director));
        p.Add(" ");
      }
      p.Add(CreateYearAndDuration(movie));
      return p;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a Paragraph containing information about the year
     * and the duration of a movie.
     * @param    movie    the movie for which you want to create a Paragraph
     */
    public Paragraph CreateYearAndDuration(Movie movie) {
      Paragraph info = new Paragraph();
      info.Font = FilmFonts.NORMAL;
      info.Add(new Chunk("Year: ", FilmFonts.BOLDITALIC));
      info.Add(new Chunk(movie.Year.ToString(), FilmFonts.NORMAL));
      info.Add(new Chunk(" Duration: ", FilmFonts.BOLDITALIC));
      info.Add(new Chunk(movie.Duration.ToString(), FilmFonts.NORMAL));
      info.Add(new Chunk(" minutes", FilmFonts.NORMAL));
      return info;
    }    
// ===========================================================================
  }
}