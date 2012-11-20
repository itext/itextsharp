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
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class TimetableDestinations : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "timetable_destinations.pdf";
    /** The font that is used for the navigation links. */
    public static Font SYMBOL = new Font(Font.FontFamily.SYMBOL, 20);
    /** A list to cache all the possible actions */
    public List<PdfAction> actions;
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        zip.AddEntry(RESULT, new TimetableDestinations().ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
    // Create the reader
      PdfReader reader = new PdfReader(src);
      int n = reader.NumberOfPages;
      using (MemoryStream ms =  new MemoryStream()) {
        // Create the stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Make a list with all the possible actions
          actions = new List<PdfAction>();
          PdfDestination d;
          for (int i = 0; i < n; ) {
            d = new PdfDestination(PdfDestination.FIT);
            actions.Add(PdfAction.GotoLocalPage(++i, d, stamper.Writer));
          }
          // Add a navigation table to every page
          PdfContentByte canvas;
          for (int i = 0; i < n; ) {
            canvas = stamper.GetOverContent(++i);
            CreateNavigationTable(i, n).WriteSelectedRows(0, -1, 696, 36, canvas);
          }
        }
        return ms.ToArray();
      }   
    }    
// ---------------------------------------------------------------------------    
    /**
     * Create a table that can be used as a footer
     * @param pagenumber the page that will use the table as footer
     * @param total the total number of pages
     * @return a tabel
     */
    public PdfPTable CreateNavigationTable(int pagenumber, int total) {
      PdfPTable table = new PdfPTable(4);
      table.DefaultCell.Border = Rectangle.NO_BORDER;
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
      Chunk first = new Chunk(((char)220).ToString(), SYMBOL);
      first.SetAction(actions[0]);
      table.AddCell(new Phrase(first));
      Chunk previous = new Chunk(((char)172).ToString(), SYMBOL);
      previous.SetAction(actions[pagenumber - 2 < 0 ? 0 : pagenumber - 2]);
      table.AddCell(new Phrase(previous));
      Chunk next = new Chunk(((char)174).ToString(), SYMBOL);
      next.SetAction(actions[pagenumber >= total ? total - 1 : pagenumber]);
      table.AddCell(new Phrase(next));
      Chunk last = new Chunk(((char)222).ToString(), SYMBOL);
      last.SetAction(actions[total - 1]);
      table.AddCell(new Phrase(last));
      table.TotalWidth = 120;
      return table;
    }    
// ===========================================================================
  }
}