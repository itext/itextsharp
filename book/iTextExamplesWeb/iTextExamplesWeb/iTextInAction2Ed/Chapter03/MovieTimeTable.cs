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

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieTimeTable : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        DrawTimeTable(writer.DirectContentUnder);
        DrawTimeSlots(writer.DirectContent);
      }
    }
// ---------------------------------------------------------------------------    
    /** The number of locations on our time table. */
    public const int LOCATIONS = 9;
    /** The number of time slots on our time table. */
    public const int TIMESLOTS = 32;
    
    /** The offset to the left of our time table. */
    public const float OFFSET_LEFT = 76;
    /** The width of our time table. */
    public const float WIDTH = 740;
    /** The offset from the bottom of our time table. */
    public const float OFFSET_BOTTOM = 36;
    /** The height of our time table */
    public const float HEIGHT = 504;
    
    /** The offset of the location bar next to our time table. */
    public const float OFFSET_LOCATION = 26;
    /** The width of the location bar next to our time table. */
    public const float WIDTH_LOCATION = 48;
    
    /** The height of a bar showing the movies at one specific location. */
    public static float HEIGHT_LOCATION = HEIGHT / LOCATIONS;
    /** The width of a time slot. */
    public static float WIDTH_TIMESLOT = WIDTH / TIMESLOTS;
// ---------------------------------------------------------------------------    
    /**
     * Draws the time table for a day at the film festival.
     * @param directcontent a canvas to which the time table has to be drawn.
     */
    protected void DrawTimeTable(PdfContentByte directcontent) {        
        directcontent.SaveState();
        directcontent.SetLineWidth(1.2f);
        float llx, lly, urx, ury;
        
        llx = OFFSET_LEFT;
        lly = OFFSET_BOTTOM;
        urx = OFFSET_LEFT + WIDTH;
        ury = OFFSET_BOTTOM + HEIGHT;
        directcontent.MoveTo(llx, lly);
        directcontent.LineTo(urx, lly);
        directcontent.LineTo(urx, ury);
        directcontent.LineTo(llx, ury);
        directcontent.ClosePath();
        directcontent.Stroke();
        
        llx = OFFSET_LOCATION;
        lly = OFFSET_BOTTOM;
        urx = OFFSET_LOCATION + WIDTH_LOCATION;
        ury = OFFSET_BOTTOM + HEIGHT;
        directcontent.MoveTo(llx, lly);
        directcontent.LineTo(urx, lly);
        directcontent.LineTo(urx, ury);
        directcontent.LineTo(llx, ury);
        directcontent.ClosePathStroke();
        
        directcontent.SetLineWidth(1);
        directcontent.MoveTo(
          OFFSET_LOCATION + WIDTH_LOCATION / 2, OFFSET_BOTTOM
        );
        directcontent.LineTo(
          OFFSET_LOCATION + WIDTH_LOCATION / 2, OFFSET_BOTTOM + HEIGHT
        );
        float y;
        for (int i = 1; i < LOCATIONS; i++) {
          y = OFFSET_BOTTOM + (i * HEIGHT_LOCATION);
          if (i == 2 || i == 6) {
            directcontent.MoveTo(OFFSET_LOCATION, y);
            directcontent.LineTo(OFFSET_LOCATION + WIDTH_LOCATION, y);
          }
          else {
            directcontent.MoveTo(OFFSET_LOCATION + WIDTH_LOCATION / 2, y);
            directcontent.LineTo(OFFSET_LOCATION + WIDTH_LOCATION, y);
          }
          directcontent.MoveTo(OFFSET_LEFT, y);
          directcontent.LineTo(OFFSET_LEFT + WIDTH, y);
        }
        directcontent.Stroke();
        directcontent.RestoreState();
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws the time slots for a day at the film festival.
     * @param directcontent the canvas to which the time table has to be drawn.
     */
    protected void DrawTimeSlots(PdfContentByte directcontent) {
      directcontent.SaveState();
      float x;
      for (int i = 1; i < TIMESLOTS; i++) {
        x = OFFSET_LEFT + (i * WIDTH_TIMESLOT);
        directcontent.MoveTo(x, OFFSET_BOTTOM);
        directcontent.LineTo(x, OFFSET_BOTTOM + HEIGHT);
      }
      directcontent.SetLineWidth(0.3f);
      directcontent.SetColorStroke(BaseColor.GRAY);
      directcontent.SetLineDash(3, 1);
      directcontent.Stroke();
      directcontent.RestoreState();
    }    
// ===========================================================================
  }
}