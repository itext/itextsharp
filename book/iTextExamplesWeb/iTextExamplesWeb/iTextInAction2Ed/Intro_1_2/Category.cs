/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */

/**
 * POJO for an object that corresponds with a record
 * in the table festival_category.
 */
namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class Category {
// =========================================================================== 
    /** The name of the category. */
    public string Name { get; set; }
    /** A short keyword for the category. */
    public string Keyword { get; set; }
    /** The color code of the category. */
    public string color { get; set; }
    /** The parent category (if any). */
    public Category Parent { get; set; }    
// =========================================================================== 
  }
} 