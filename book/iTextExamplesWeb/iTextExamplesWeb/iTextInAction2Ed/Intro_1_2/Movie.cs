/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System.Collections.Generic;

/**
 * POJO for an object that corresponds with a record
 * in the table film_movietitle.
 */
 namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class Movie {
// =========================================================================== 
    /** The title of the movie. */
    public string Title { get; set; }
    /** The original title (if different from title). */
    public string OriginalTitle { get; set; }
    /** Code used by IMDB */
    public string Imdb { get; set; }
    /** The year the movie was released. */
    public int Year { get; set; }
    /** The duration of the movie in minutes. */
    public int Duration { get; set; }
    /** The list of directors. */
    private List<Director> _directors = new List<Director>();
    public List<Director> Directors { 
      get { return _directors; }
    }
    /** The list of countries. */
    private List<Country> _countries = new List<Country>();
    public List<Country> Countries { 
      get { return _countries; }
    }
// ---------------------------------------------------------------------------
    /** The filmfestival entry info. */
    private Entry _entry;
    public Entry entry { 
      get { return _entry; }
      set {
        _entry = value;
        if (_entry.movie == null) _entry.movie = this;
      } 
    }
// ---------------------------------------------------------------------------
    /**
     * Adds a director.
     * @param director one of the directors of the movie
     */
    public void AddDirector(Director director) {
      _directors.Add(director);
    }
// ---------------------------------------------------------------------------
    /**
     * Adds a country.
     * @param country  one of the countries the movie was made by.
     */
    public void AddCountry(Country country) {
      _countries.Add(country);    
    }
// ---------------------------------------------------------------------------
    /**
     * Returns the title in the correct form.
     * @return a title
     */
    public string MovieTitle {
      get {
        var title = Title;
        if (!string.IsNullOrEmpty(title)) {
          if (title.EndsWith(", A"))
              return "A " + title.Substring(0, title.Length - 3);
          if (title.EndsWith(", The"))
              return "The " + title.Substring(0, title.Length - 5);
          return title;
        }
        return null;
      }
    } 
// ---------------------------------------------------------------------------    
    /**
     * Returns the title in the correct form.
     * @return a title
     */
    public string GetMovieTitle(bool prefix) {
      var title = Title;
      if (title.EndsWith(", A")) {
      return prefix ? "A " : title.Substring(0, title.Length - 3);
      }
      
      if (title.EndsWith(", The")) {
        return prefix ? "The " : title.Substring(0, title.Length - 5);
      }
      
      return prefix ? null : title;
    }    
// =========================================================================== 
  }
}