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
  public class MovieTemplates : MovieCalendar {
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
        PdfTemplate t_under = under.CreateTemplate(
          PageSize.A4.Height, PageSize.A4.Width
        );
        DrawTimeTable(t_under);
        PdfTemplate t_over = over.CreateTemplate(
          PageSize.A4.Height, PageSize.A4.Width
        );
        DrawTimeSlots(t_over);
        DrawInfo(t_over);        
        List<string> days = PojoFactory.GetDays();
        List<Screening> screenings;
        int d = 1;
        foreach (string day in days) {
          over.AddTemplate(t_over, 0, 0);
          under.AddTemplate(t_under, 0, 0);
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
    /**
     * Constructor MovieTemplates object.
     */
    public MovieTemplates() : base() {  }
// ===========================================================================
  }
}