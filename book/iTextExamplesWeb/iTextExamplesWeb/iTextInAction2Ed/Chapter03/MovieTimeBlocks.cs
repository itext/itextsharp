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
  public class MovieTimeBlocks : MovieTimeTable {
// ===========================================================================
    /** A list containing all the locations. */
    protected List<String> locations;
// ---------------------------------------------------------------------------        
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.CompressionLevel = 0;
        // step 3
        document.Open();
        // step 4
        PdfContentByte over = writer.DirectContent;
        PdfContentByte under = writer.DirectContentUnder;
        locations = PojoFactory.GetLocations();
        List<string> days = PojoFactory.GetDays();
        List<Screening> screenings;
        foreach (string day in days) {
          DrawTimeTable(under);
          DrawTimeSlots(over);
          screenings = PojoFactory.GetScreenings(day);
          foreach (Screening screening in screenings) {
            DrawBlock(screening, under, over);
          }
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * Draws a colored block on the time table, corresponding with
     * the screening of a specific movie.
     * @param    screening    a screening POJO, contains a movie and a category
     * @param    under    the canvas to which the block is drawn
     */
    protected void DrawBlock(
      Screening screening, PdfContentByte under, PdfContentByte over
    ) {
      under.SaveState();
      BaseColor color = WebColors.GetRGBColor(
        "#" + screening.movie.entry.category.color
      );
      under.SetColorFill(color);
      Rectangle rect = GetPosition(screening);
      under.Rectangle(
        rect.Left, rect.Bottom, rect.Width, rect.Height
      );
      under.Fill();
      over.Rectangle(
        rect.Left, rect.Bottom, rect.Width, rect.Height
      );
      over.Stroke();
      under.RestoreState();
    }
// ---------------------------------------------------------------------------        
    /** The "offset time" for our calendar sheets. */
    // public static long TIME930 = 30600000L;
    public const long TIME930 = 34200000L;
    
    /** The width of one minute. */
    public readonly float MINUTE = WIDTH_TIMESLOT / 30f;
// ---------------------------------------------------------------------------        
    /**
     * Calculates the position of a rectangle corresponding with a screening.
     * @param    screening    a screening POJO, contains a movie
     * @return    a Rectangle
     */
    protected Rectangle GetPosition(Screening screening) {
      float llx, lly, urx, ury;
      // long minutesAfter930 = (screening.getTime().getTime() - TIME930) / 60000L;
      long minutesAfter930 = (
        Utility.GetMilliseconds(screening.Time) - TIME930
      ) / 60000L;
      llx = OFFSET_LEFT + (MINUTE * minutesAfter930);
      int location = locations.IndexOf(screening.Location) + 1;
      lly = OFFSET_BOTTOM + (LOCATIONS - location) * HEIGHT_LOCATION;
      urx = llx + MINUTE * screening.movie.Duration;
      ury = lly + HEIGHT_LOCATION;
      return new Rectangle(llx, lly, urx, ury);
    }
// ===========================================================================
  }
}