/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class PojoToElementFactory {
// =========================================================================== 
    /**
     * Creates a Phrase containing the title of a Movie.
     * @param movie a Movie object
     * @return a Phrase object
     */
    public static Phrase GetMovieTitlePhrase(Movie movie) {
      return new Phrase(movie.MovieTitle, FilmFonts.NORMAL);
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a Phrase containing the original title of a Movie.
     * @param movie a Movie object
     * @return a Phrase object
     */
    public static Phrase GetOriginalTitlePhrase(Movie movie) {
      return  string.IsNullOrEmpty(movie.OriginalTitle)
        ? new Phrase("", FilmFonts.NORMAL)
        : new Phrase(movie.OriginalTitle, FilmFonts.ITALIC)
        ;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a Phrase containing the name of a Director.
     * @param director a Director object
     * @return a Phrase object
     */
    public static Phrase GetDirectorPhrase(Director director) {
      Phrase phrase = new Phrase();
      phrase.Add(new Chunk(director.Name, FilmFonts.BOLD));
      phrase.Add(new Chunk(", ", FilmFonts.BOLD));
      phrase.Add(new Chunk(director.GivenName, FilmFonts.NORMAL));
      return phrase;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a Phrase containing the name of a Country.
     * @param country a Country object
     * @return a Phrase object
     */
    public static Phrase GetCountryPhrase(Country country) {
      return new Phrase(country.Name, FilmFonts.NORMAL);
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a list with directors.
     * @param movie a Movie object
     * @return a List object
     */
    public static List GetDirectorList(Movie movie) {
      var list = new List();
      foreach (Director director in movie.Directors ) {
        list.Add(string.Format(
          "{0}, {1}", director.Name, director.GivenName
        ));
      }
      return list;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a list with countries.
     * @param movie a Movie object
     * @return a List object
     */
    public static List GetCountryList(Movie movie) {
      var list = new List();
      foreach (Country country in movie.Countries) {
        list.Add(country.Name);
      }
      return list;
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a phrase with the production year of a movie.
     * @param movie a Movie object
     * @return a Phrase object
     */
    // public static Element GetYearPhrase(Movie movie) {
    public static Phrase GetYearPhrase(Movie movie) {
      Phrase p = new Phrase();
      p.Add(new Chunk("Year: ", FilmFonts.BOLD));
      p.Add(new Chunk(movie.Year.ToString(), FilmFonts.NORMAL));
      return p;
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a phrase with the run length of a movie.
     * @param movie a Movie object
     * @return a Phrase object
     */
    // public static Element getDurationPhrase(Movie movie) {
    public static Phrase GetDurationPhrase(Movie movie) {
      Phrase p = new Phrase();
      p.Add(new Chunk("Duration: ", FilmFonts.BOLD));
      p.Add(new Chunk(movie.Duration.ToString(), FilmFonts.NORMAL));
      return p;
    }
// =========================================================================== 
  }
}