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
  public class TimetableAnnotations2 : TimetableAnnotations1 {
// ===========================================================================
    /** The resulting PDF. */
    public new const String RESULT = "timetable_links.pdf";
    /** Path to IMDB. */
    public const string IMDB = "http://imdb.com/title/tt{0}/";
// --------------------------------------------------------------------------- 
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        TimetableAnnotations2 t = new TimetableAnnotations2();
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
          // Add annotations for every screening
          int page = 1;
          Rectangle rect;
          PdfAnnotation annotation;
          foreach (string day in PojoFactory.GetDays()) {
            foreach (Screening screening in PojoFactory.GetScreenings(day)) {
              rect = GetPosition(screening);
              annotation = PdfAnnotation.CreateLink(
                stamper.Writer, rect, PdfAnnotation.HIGHLIGHT_INVERT,
                new PdfAction(string.Format(IMDB, screening.movie.Imdb))
              );
              stamper.AddAnnotation(annotation, page);
            }
            page++;
          }
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}