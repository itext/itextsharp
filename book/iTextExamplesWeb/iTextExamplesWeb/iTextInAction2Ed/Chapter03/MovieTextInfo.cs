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

/**
 * Draws a time table to the direct content using lines and simple shapes,
 * adding blocks representing a movies.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieTextInfo : MovieTimeBlocks {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte over = writer.DirectContent;
        PdfContentByte under = writer.DirectContentUnder;
        locations = PojoFactory.GetLocations();
        List<string> days = PojoFactory.GetDays();
        List<Screening> screenings;
        int d = 1;
        foreach (string day in days) {
          DrawTimeTable(under);
          DrawTimeSlots(over);
          DrawInfo(over);
          DrawDateInfo(day, d++, over);
          screenings = PojoFactory.GetScreenings(day);
          foreach (Screening screening in screenings) {
            DrawBlock(screening, under, over);
            DrawMovieInfo(screening, over);
          }
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------        
    /** The base font that will be used to write info on the calendar sheet. */
    protected BaseFont bf;
    
    /** A phrase containing a white letter "P" (for Press) */
    protected Phrase press;

    /** The different time slots. */
    public readonly String[] TIME = { 
      "09:30", "10:00", "10:30", "11:00", "11:30", "12:00",
      "00:30", "01:00", "01:30", "02:00", "02:30", "03:00",
      "03:30", "04:00", "04:30", "05:00", "05:30", "06:00",
      "06:30", "07:00", "07:30", "08:00", "08:30", "09:00",
      "09:30", "10:00", "10:30", "11:00", "11:30", "12:00",
      "00:30", "01:00" 
    };
// ---------------------------------------------------------------------------           
    /**
     * Draws some text on every calendar sheet.
     * 
     */
    protected void DrawInfo(PdfContentByte directcontent) {
      directcontent.BeginText();
      directcontent.SetFontAndSize(bf, 18);
      float x, y;
      x = (OFFSET_LEFT + WIDTH + OFFSET_LOCATION) / 2;
      y = OFFSET_BOTTOM + HEIGHT + 24;
      directcontent.ShowTextAligned(
        Element.ALIGN_CENTER,
        "FOOBAR FILM FESTIVAL", x, y, 0
      );
      x = OFFSET_LOCATION + WIDTH_LOCATION / 2f - 6;
      y = OFFSET_BOTTOM + HEIGHT_LOCATION;
      directcontent.ShowTextAligned(
        Element.ALIGN_CENTER,
        "The Majestic", x, y, 90
      );
      y = OFFSET_BOTTOM + HEIGHT_LOCATION * 4f;
      directcontent.ShowTextAligned(
        Element.ALIGN_CENTER,
        "Googolplex", x, y, 90
      );
      y = OFFSET_BOTTOM + HEIGHT_LOCATION * 7.5f;
      directcontent.ShowTextAligned(
        Element.ALIGN_CENTER,
        "Cinema Paradiso", x, y, 90
      );
      directcontent.SetFontAndSize(bf, 12);
      x = OFFSET_LOCATION + WIDTH_LOCATION - 6;
      for (int i = 0; i < LOCATIONS; i++) {
        y = OFFSET_BOTTOM + ((8.5f - i) * HEIGHT_LOCATION);
        directcontent.ShowTextAligned(
          Element.ALIGN_CENTER,
          locations[i], x, y, 90
        );
      }
      directcontent.SetFontAndSize(bf, 6);
      y = OFFSET_BOTTOM + HEIGHT + 1;
      for (int i = 0; i < TIMESLOTS; i++) {
        x = OFFSET_LEFT + (i * WIDTH_TIMESLOT);
        directcontent.ShowTextAligned(
          Element.ALIGN_LEFT,
          TIME[i], x, y, 45
        );
      }
      directcontent.EndText();
    }
// ---------------------------------------------------------------------------            
    /**
     * Draws some text on every calendar sheet.
     * 
     */
    protected void DrawDateInfo(string day, int d, PdfContentByte directcontent) {
      directcontent.BeginText();
      directcontent.SetFontAndSize(bf, 18);
      float x, y;
      x = OFFSET_LOCATION;
      y = OFFSET_BOTTOM + HEIGHT + 24;
      directcontent.ShowTextAligned(
        Element.ALIGN_LEFT,
        "Day " + d, x, y, 0
      );
      x = OFFSET_LEFT + WIDTH;
      directcontent.ShowTextAligned(
        Element.ALIGN_RIGHT,
        day, x, y, 0
      );
      directcontent.EndText();
    }
// ---------------------------------------------------------------------------            
    /**
     * Draws the info about the movie.
     * @throws DocumentException 
     */
    protected virtual void DrawMovieInfo(
      Screening screening, PdfContentByte directcontent
    ) {
      // if (true) {
      if (screening.Press) {
        Rectangle rect = GetPosition(screening);
        ColumnText.ShowTextAligned(
          directcontent, 
          Element.ALIGN_CENTER,
          press, (rect.Left + rect.Right) / 2,
          rect.Bottom + rect.Height / 4, 0
        );
      }
    }
// ---------------------------------------------------------------------------            
    /**
     * Constructor for the MovieCalendar class; initializes the base font object.
     * @throws IOException 
     * @throws DocumentException 
     */
    public MovieTextInfo() {
      bf = BaseFont.CreateFont();
      Font f = new Font(bf, HEIGHT_LOCATION / 2);
      f.Color = BaseColor.WHITE;
      press = new Phrase("P", f);
    }
// ===========================================================================
  }
}