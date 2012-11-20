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

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MovieHistory : IWriter {
// ===========================================================================
    /** The different epochs. */
    public static readonly string[] EPOCH = { 
      "Forties", "Fifties", "Sixties", "Seventies", "Eighties",
      "Nineties", "Twenty-first Century" 
    };
    /** The fonts for the title. */
    public static Font[] FONT = new Font[4];
    static MovieHistory() {
      FONT[0] = new Font(Font.FontFamily.HELVETICA, 24);
      FONT[1] = new Font(Font.FontFamily.HELVETICA, 18);
      FONT[2] = new Font(Font.FontFamily.HELVETICA, 14);
      FONT[3] = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
    }
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        int epoch = -1;
        int currentYear = 0;
        Paragraph title = null;
        Chapter chapter = null;
        Section section = null;
        Section subsection = null;        
        IEnumerable<Movie> movies = PojoFactory.GetMovies(true);
        // loop over the movies
        foreach (Movie movie in movies) {
          int yr = movie.Year;
        // add the chapter if we're in a new epoch
          if (epoch < (yr - 1940) / 10) {
            epoch = (yr - 1940) / 10;
            if (chapter != null) {
              document.Add(chapter);
            }
            title = new Paragraph(EPOCH[epoch], FONT[0]);
            chapter = new Chapter(title, epoch + 1);
          }
          // switch to a new year
          if (currentYear < yr) {
              currentYear = yr;
              title = new Paragraph(
                string.Format("The year {0}", yr), FONT[1]
              );
              section = chapter.AddSection(title);
              section.BookmarkTitle = yr.ToString();
              section.Indentation = 30;
              section.BookmarkOpen = false;
              section.NumberStyle = Section.NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT;
              section.Add(new Paragraph(
                string.Format("Movies from the year {0}:", yr)
              ));
          }
          title = new Paragraph(movie.Title, FONT[2]);
          subsection = section.AddSection(title);
          subsection.IndentationLeft = 20;
          subsection.NumberDepth = 1;
          subsection.Add(new Paragraph("Duration: " + movie.Duration.ToString(), FONT[3]));
          subsection.Add(new Paragraph("Director(s):", FONT[3]));
          subsection.Add(PojoToElementFactory.GetDirectorList(movie));
          subsection.Add(new Paragraph("Countries:", FONT[3]));
          subsection.Add(PojoToElementFactory.GetCountryList(movie));
        }
        document.Add(chapter);
      }
    }
// ===========================================================================
  }
}