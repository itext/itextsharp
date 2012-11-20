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
  public class TimetableAnnotations3 : TimetableAnnotations1 {
// ===========================================================================
    /** The resulting PDF file. */
    public new const String RESULT = "timetable_tickets.pdf";
    /** Path to IMDB. */
    public const string IMDB = "http://imdb.com/title/tt{0}/";
// --------------------------------------------------------------------------- 
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        TimetableAnnotations3 t = new TimetableAnnotations3();
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
    public override byte[] ManipulatePdf(byte[] src) {
      locations = PojoFactory.GetLocations();
      // Create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Loop over the days and screenings
          int page = 1;
          Rectangle rect;
          float top;
          PdfAnnotation annotation;
          Movie movie;
          foreach (string day in PojoFactory.GetDays()) {
            foreach (Screening screening in PojoFactory.GetScreenings(day)) {
              rect = GetPosition(screening);
              movie = screening.movie;
              // Annotation for press previews
              if (screening.Press) {
                annotation = PdfAnnotation.CreateStamp(
                  stamper.Writer, rect, "Press only", "NotForPublicRelease"
                );
                annotation.Color = BaseColor.BLACK;
                annotation.Flags = PdfAnnotation.FLAGS_PRINT;
              }
              // Annotation for screenings that are sold out
              else if (IsSoldOut(screening)) {
                top = reader.GetPageSizeWithRotation(page).Top;
                annotation = PdfAnnotation.CreateLine(
                    stamper.Writer, rect, "SOLD OUT",
                    top - rect.Top, rect.Right,
                    top - rect.Bottom, rect.Left
                );
                annotation.Title = movie.MovieTitle;
                annotation.Color = BaseColor.WHITE;
                annotation.Flags = PdfAnnotation.FLAGS_PRINT;
                annotation.BorderStyle = new PdfBorderDictionary(
                  5, PdfBorderDictionary.STYLE_SOLID
                );
              }
              // Annotation for screenings with tickets available
              else {
                annotation = PdfAnnotation.CreateSquareCircle(
                  stamper.Writer, rect, "Tickets available", true
                );
                annotation.Title = movie.MovieTitle;
                annotation.Color = BaseColor.BLUE;
                annotation.Flags = PdfAnnotation.FLAGS_PRINT;
                annotation.Border = new PdfBorderArray(
                  0, 0, 2, new PdfDashPattern()
                );
              }
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
     * Checks if the screening has been sold out.
     * @param screening a Screening POJO
     * @return true if the screening has been sold out.
     */
    public bool IsSoldOut(Screening screening) {
      return screening.movie.MovieTitle.StartsWith("L")
        ? true : false
      ;
    }    
// ===========================================================================
  }
}