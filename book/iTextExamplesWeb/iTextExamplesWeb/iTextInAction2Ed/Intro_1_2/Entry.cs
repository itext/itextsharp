/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
/**
 * POJO for an object that corresponds with a record
 * in the table festival_entry.
 */
namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class Entry {
// =========================================================================== 
    /** The festival year. */
    public int Year { get; set; }
    /** The category. */
    public Category category { get; set; }
// ---------------------------------------------------------------------------
    /** The movie. */
    private Movie _movie;
    public Movie movie { 
      get { return _movie; }
      set {
        _movie = value;
        if (_movie.entry == null) _movie.entry = this;
      } 
    }    
// ---------------------------------------------------------------------------
    /** The screenings. */
    private List<Screening> _screenings = new List<Screening>();
    public List<Screening> Screenings { 
      get { return _screenings; }
    }
// ---------------------------------------------------------------------------
    /**
     * Adds a screening to this entry.
     */
    public void AddScreening(Screening screening) {
      _screenings.Add(screening);    
    }
// =========================================================================== 
  }
}