/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class EventsAndActions : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "events_and_actions.pdf";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        EventsAndActions e = new EventsAndActions();
        zip.AddEntry(RESULT, e.ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }           
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result (localhost)
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
    // Create the reader
      PdfReader reader = new PdfReader(src);
      int n = reader.NumberOfPages;
      using (MemoryStream ms = new MemoryStream()) {
        // Create the stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Get the writer (to add actions and annotations)
          PdfWriter writer = stamper.Writer;
          PdfAction action = PdfAction.GotoLocalPage(2,
            new PdfDestination(PdfDestination.FIT), writer
          );
          writer.SetOpenAction(action);
          action = PdfAction.JavaScript(
            "app.alert('Think before you print');", writer
          );
          writer.SetAdditionalAction(PdfWriter.WILL_PRINT, action);
          action = PdfAction.JavaScript(
            "app.alert('Think again next time!');", writer
          );
          writer.SetAdditionalAction(PdfWriter.DID_PRINT, action);
          action = PdfAction.JavaScript(
            "app.alert('We hope you enjoy the festival');", writer
          );
          writer.SetAdditionalAction(PdfWriter.DOCUMENT_CLOSE, action);
          action = PdfAction.JavaScript(
            "app.alert('This day is reserved for people with an accreditation "
            + "or an invitation.');",
            writer
          );
          stamper.SetPageAction(PdfWriter.PAGE_OPEN, action, 1);
          action = PdfAction.JavaScript(
            "app.alert('You can buy tickets for all the other days');", writer
          );
          stamper.SetPageAction(PdfWriter.PAGE_CLOSE, action, 1);      
        }
        return ms.ToArray();       
      }       
    }        
// ===========================================================================
  }
}