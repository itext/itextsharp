/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class TimetableAnnotations1 : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "timetable_help.pdf";
    /** A pattern for an info string. */
    public const string INFO = "Movie produced in {0}; run length: {1}";
    /** A list containing all the locations. */
    protected List<String> locations;
    
    /** The number of locations on our time table. */
    public const int LOCATIONS = 9;
    /** The number of time slots on our time table. */
    public const int TIMESLOTS = 32;
    /** The "offset time" for our calendar sheets. */
    // public static long TIME930 = 30600000L;
    public const long TIME930 = 34200000L;
    /** The offset to the left of our time table. */
    public const float OFFSET_LEFT = 76;
    /** The width of our time table. */
    public const float WIDTH = 740;
    /** The width of a time slot. */
    public static float WIDTH_TIMESLOT = WIDTH / TIMESLOTS;
    /** The width of one minute. */
    public static float MINUTE = WIDTH_TIMESLOT / 30f;
    /** The offset from the bottom of our time table. */
    public const float OFFSET_BOTTOM = 36;
    /** The height of our time table */
    public const float HEIGHT = 504;
    /** The height of a bar showing the movies at one specific location. */
    public static float HEIGHT_LOCATION = HEIGHT / LOCATIONS;
// ---------------------------------------------------------------------------
    public virtual void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        TimetableAnnotations1 t = new TimetableAnnotations1();
        zip.AddEntry(RESULT, t.ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }
    }     
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public virtual byte[] ManipulatePdf(byte[] src) {
      locations = PojoFactory.GetLocations();
      // Create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Add the annotations
          int page = 1;
          Rectangle rect;
          PdfAnnotation annotation;
          Movie movie;
          foreach (string day in PojoFactory.GetDays()) {
            foreach (Screening screening in PojoFactory.GetScreenings(day)) {
              movie = screening.movie;
              rect = GetPosition(screening);
              annotation = PdfAnnotation.CreateText(
                stamper.Writer, rect, movie.MovieTitle,
                string.Format(INFO, movie.Year, movie.Duration),
                false, "Help"
              );
              annotation.Color = WebColors.GetRGBColor(
                "#" + movie.entry.category.color
              );
              stamper.AddAnnotation(annotation, page);
            }
            page++;
          }
        }
        return ms.ToArray();
      }
    }  
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
      Rectangle rect = new Rectangle(llx, lly, urx, ury);
      return rect;
    }
// ===========================================================================
  }
}