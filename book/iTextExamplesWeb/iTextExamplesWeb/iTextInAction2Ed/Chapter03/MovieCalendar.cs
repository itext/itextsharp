/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieCalendar : MovieTextInfo {
// ===========================================================================
    /**
     * Draws the info about the movie.
     * @throws DocumentException 
     */
    protected override void DrawMovieInfo(
      Screening screening, PdfContentByte directcontent
    ) {
      base.DrawMovieInfo(screening, directcontent);
      Rectangle rect = GetPosition(screening);
      ColumnText column = new ColumnText(directcontent);
      column.SetSimpleColumn(
        new Phrase(screening.movie.Title),
        rect.Left, rect.Bottom,
        rect.Right, rect.Top, 18, Element.ALIGN_CENTER
      );
      column.Go();
    }
// ---------------------------------------------------------------------------            
    /**
     * Constructor for the MovieCalendar class; initializes the base font object.
     * @throws IOException 
     * @throws DocumentException 
     */
    public MovieCalendar() : base() {  }
// ===========================================================================
  }
}