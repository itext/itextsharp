/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;

/**
 * POJO for an object that corresponds with a record
 * in the table festival_screening.
 */
namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class Screening {
// =========================================================================== 
    /** The date of the screening. */
    public string Date { get; set; }
    /** The time of the screening. */
    public string Time { get; set; }
    /** The location of the screening. */
    public String Location { get; set; }
    /** Is this a screening for the press only? */
    public bool Press { get; set; }
    /** The movie that will be screened. */
    public Movie movie { get; set; }
// =========================================================================== 
  }
}