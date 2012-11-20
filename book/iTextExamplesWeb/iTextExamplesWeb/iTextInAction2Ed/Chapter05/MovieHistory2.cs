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
  public class MovieHistory2 : IWriter {
// ===========================================================================
    /** Inner class to add a header and a footer. */
    class HeaderFooter : PdfPageEventHelper {
      /** Alternating phrase for the header. */
      Phrase[] header = new Phrase[2];
      /** Current page number (will be reset for every chapter). */
      int pagenumber;
      
      /**
       * Initialize one of the headers.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onOpenDocument(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnOpenDocument(PdfWriter writer, Document document) {
        header[0] = new Phrase("Movie history");
      }
      
      /**
       * Initialize one of the headers, based on the chapter title;
       * reset the page number.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onChapter(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document, float,
       *      com.itextpdf.text.Paragraph)
       */
      public override void OnChapter(
        PdfWriter writer, Document document,
        float paragraphPosition, Paragraph title) 
      {
        header[1] = new Phrase(title.Content);
        pagenumber = 1;
      }

      /**
       * Increase the page number.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onStartPage(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnStartPage(PdfWriter writer, Document document) {
        pagenumber++;
      }
        
      /**
       * Adds the header and the footer.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onEndPage(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnEndPage(PdfWriter writer, Document document) {
        Rectangle rect = writer.GetBoxSize("art");
        switch(writer.PageNumber % 2) {
        case 0:
          ColumnText.ShowTextAligned(writer.DirectContent,
            Element.ALIGN_RIGHT, 
            header[0],
            rect.Right, rect.Top, 0
          );
          break;
        case 1:
          ColumnText.ShowTextAligned(
            writer.DirectContent,
            Element.ALIGN_LEFT,
            header[1],
            rect.Left, rect.Top, 0
          );
          break;
        }
        ColumnText.ShowTextAligned(
          writer.DirectContent,
          Element.ALIGN_CENTER, 
          new Phrase(String.Format("page {0}", pagenumber)),
          (rect.Left + rect.Right) / 2, 
          rect.Bottom - 18, 0
        );
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
    
    public MovieHistory2() {
      FONT[0] = new Font(Font.FontFamily.HELVETICA, 24);
      FONT[1] = new Font(Font.FontFamily.HELVETICA, 18);
      FONT[2] = new Font(Font.FontFamily.HELVETICA, 14);
      FONT[3] = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD);
      FONT[4] = new Font(Font.FontFamily.HELVETICA, 10);
    }
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4, 36, 36, 54, 54)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        HeaderFooter tevent = new HeaderFooter();
        writer.SetBoxSize("art", new Rectangle(36, 54, 559, 788));
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
      }
    }
// ===========================================================================
  }
}