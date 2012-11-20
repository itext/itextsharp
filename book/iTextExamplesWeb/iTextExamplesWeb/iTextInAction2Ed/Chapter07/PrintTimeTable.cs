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
  public class PrintTimeTable : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "print_timetable.pdf";
    /** Path to a resource. */
    public const string RESOURCE = "print_page.js";
    protected string jsString;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        PrintTimeTable p = new PrintTimeTable();
        zip.AddEntry(RESULT, p.ManipulatePdf(pdf));
        zip.AddEntry(RESOURCE, p.jsString);
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
          // Add JavaScript
          jsString = File.ReadAllText(
            Path.Combine(Utility.ResourceJavaScript, RESOURCE)
          );
          stamper.JavaScript = jsString; 
          // Create a Chunk with a chained action
          PdfContentByte canvas;
          Chunk chunk = new Chunk("print this page");
          PdfAction action = PdfAction.JavaScript(
            "app.alert('Think before you print!');", 
            stamper.Writer
          );
          action.Next(PdfAction.JavaScript(
            "printCurrentPage(this.pageNum);", 
            stamper.Writer
          ));
          action.Next(new PdfAction("http://www.panda.org/savepaper/"));
          chunk.SetAction(action);
          Phrase phrase = new Phrase(chunk);
          // Add this Chunk to every page
          for (int i = 0; i < n; ) {
            canvas = stamper.GetOverContent(++i);
            ColumnText.ShowTextAligned(
              canvas, Element.ALIGN_RIGHT, phrase, 816, 18, 0
            );
          }
        } 
        return ms.ToArray();     
      }      
    }        
// ===========================================================================
  }
}