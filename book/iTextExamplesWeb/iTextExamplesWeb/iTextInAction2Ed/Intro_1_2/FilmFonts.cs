/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using iTextSharp.text;
using iTextSharp.text.pdf;

/**
 * Contains a series of static Font objects that are used throughout the book.
 */
namespace kuujinbo.iTextInAction2Ed.Intro_1_2 {
  public class FilmFonts {
// =========================================================================== 
    /** A font used in our PDF file */
    public static Font NORMAL = new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL);
    /** A font used in our PDF file */
    public static Font BOLD = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
    /** A font used in our PDF file */
    public static Font ITALIC = new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC);
    /** A font used in our PDF file */
    public static Font BOLDITALIC = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLDITALIC);
// =========================================================================== 
  }
}