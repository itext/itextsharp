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
  public class MovieSlideShow : IWriter {
// ===========================================================================
    /**
     * Page event to set the transition and duration for every page.
     */
    class TransitionDuration : PdfPageEventHelper {
      public override void OnStartPage(PdfWriter writer, Document document) {
        writer.Transition = new PdfTransition(PdfTransition.DISSOLVE, 3);
        writer.Duration = 5;
      }
    }
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A5.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PdfVersion = PdfWriter.VERSION_1_5;
        writer.ViewerPreferences = PdfWriter.PageModeFullScreen;
        writer.PageEvent = new TransitionDuration();        
        // step 3
        document.Open();
        // step 4
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        Image img;
        PdfPCell cell;
        PdfPTable table = new PdfPTable(6);
        string RESOURCE = Utility.ResourcePosters;
        foreach (Movie movie in movies) {
          img = Image.GetInstance(Path.Combine(RESOURCE, movie.Imdb + ".jpg"));
          cell = new PdfPCell(img, true);
          cell.Border = PdfPCell.NO_BORDER;
          table.AddCell(cell);
        }
        document.Add(table);
      }
    }
// ===========================================================================
  }
}