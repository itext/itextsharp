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

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class MovieHistory1 : IWriter {
// ===========================================================================
    /**
     * Inner class to keep track of the TOC
     * and to draw lines under ever chapter and section.
     */
    class ChapterSectionTOC : PdfPageEventHelper {
      private MovieHistory1 mh1;
      public ChapterSectionTOC(MovieHistory1 mh1) {
        this.mh1 = mh1;
      }
    
      /** List with the titles. */
      public List<Paragraph> titles = new List<Paragraph>();
      
      public override void OnChapter(PdfWriter writer, Document document,
              float position, Paragraph title) 
      {
        titles.Add(new Paragraph(title.Content, mh1.FONT[4]));
      }

      public override void OnChapterEnd(
        PdfWriter writer, Document document,
        float position) 
      {
        DrawLine(
          writer.DirectContent, document.Left, document.Right, position - 5
        );
      }

      public override void OnSection(
        PdfWriter writer, Document document,
        float position, int depth, Paragraph title) 
      {
          title = new Paragraph(title.Content, mh1.FONT[4]);
          title.IndentationLeft = 18 * depth;
          titles.Add(title);
      }

      public override void OnSectionEnd(
        PdfWriter writer, Document document, float position
        ) 
      {
        DrawLine(
          writer.DirectContent, document.Left, document.Right, position - 3
        );
      }
      
      public void DrawLine(
        PdfContentByte cb, float x1, float x2, float y
        ) 
      {
        cb.MoveTo(x1, y);
        cb.LineTo(x2, y);
        cb.Stroke();
      }
    }
    // end inner class
// ---------------------------------------------------------------------------
    
    /** The different epochs. */
    public readonly string[] EPOCH = { 
      "Forties", "Fifties", "Sixties", "Seventies", "Eighties",
    "Nineties", "Twenty-first Century" 
    };
    /** The fonts for the title. */
    public Font[] FONT = new Font[5];
    
    public MovieHistory1() {
      FONT[0] = new Font(Font.FontFamily.HELVETICA, 24);
      FONT[1] = new Font(Font.FontFamily.HELVETICA, 18);
      FONT[2] = new Font(Font.FontFamily.HELVETICA, 14);
      FONT[3] = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
      FONT[4] = new Font(Font.FontFamily.HELVETICA, 10);
    }
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // IMPORTANT: set linear page mode!
        writer.SetLinearPageMode();
        ChapterSectionTOC tevent = new ChapterSectionTOC(new MovieHistory1());
        writer.PageEvent = tevent;        
        // step 3
        document.Open();
        // step 4
        int epoch = -1;
        int currentYear = 0;
        Paragraph title = null;
        Chapter chapter = null;
        Section section = null;
        Section subsection = null;
        // add the chapters, sort by year
        foreach (Movie movie in PojoFactory.GetMovies(true)) {
          int year = movie.Year;
          if (epoch < (year - 1940) / 10) {
            epoch = (year - 1940) / 10;
            if (chapter != null) {
              document.Add(chapter);
            }
            title = new Paragraph(EPOCH[epoch], FONT[0]);
            chapter = new Chapter(title, epoch + 1);
          }
          if (currentYear < year) {
            currentYear = year;
            title = new Paragraph(
              String.Format("The year {0}", year), FONT[1]
            );
            section = chapter.AddSection(title);
            section.BookmarkTitle = year.ToString();
            section.Indentation = 30;
            section.BookmarkOpen = false;
            section.NumberStyle = Section.NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT;
            section.Add(new Paragraph(
              String.Format("Movies from the year {0}:", year))
            );
          }
          title = new Paragraph(movie.MovieTitle, FONT[2]);
          subsection = section.AddSection(title);
          subsection.IndentationLeft = 20;
          subsection.NumberDepth = 1;
          subsection.Add(new Paragraph(
            "Duration: " + movie.Duration.ToString(), FONT[3]
          ));
          subsection.Add(new Paragraph("Director(s):", FONT[3]));
          subsection.Add(PojoToElementFactory.GetDirectorList(movie));
          subsection.Add(new Paragraph("Countries:", FONT[3]));
          subsection.Add(PojoToElementFactory.GetCountryList(movie));
        }      
        document.Add(chapter);
        // add the TOC starting on the next page
        document.NewPage();
        int toc = writer.PageNumber;
        foreach (Paragraph p in tevent.titles) {
          document.Add(p);
        }
        // always go to a new page before reordering pages.
        document.NewPage();
        // get the total number of pages that needs to be reordered
        int total = writer.ReorderPages(null);
        // change the order
        int[] order = new int[total];
        for (int i = 0; i < total; i++) {
          order[i] = i + toc;
          if (order[i] > total) {
            order[i] -= total;
          }
        }
        // apply the new order
        writer.ReorderPages(order);        
      }
    }
// ===========================================================================
  }
}