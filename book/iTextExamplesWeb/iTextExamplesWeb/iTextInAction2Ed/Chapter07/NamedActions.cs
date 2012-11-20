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
  public class NamedActions : IWriter {
// ===========================================================================
    public const String RESULT = "named_actions.pdf";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) { 
        MovieTemplates m = new MovieTemplates();
        byte[] pdf = Utility.PdfBytes(m); 
        zip.AddEntry(RESULT, new NamedActions().ManipulatePdf(pdf)); 
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
    // Create a table with named actions
      Font symbol = new Font(Font.FontFamily.SYMBOL, 20);
      PdfPTable table = new PdfPTable(4);
      table.DefaultCell.Border = Rectangle.NO_BORDER;
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
      Chunk first = new Chunk( ((char)220).ToString() , symbol);
      first.SetAction(new PdfAction(PdfAction.FIRSTPAGE));
      table.AddCell(new Phrase(first));
      Chunk previous = new Chunk( ((char)172).ToString(), symbol);
      previous.SetAction(new PdfAction(PdfAction.PREVPAGE));
      table.AddCell(new Phrase(previous));
      Chunk next = new Chunk( ((char)174).ToString(), symbol);
      next.SetAction(new PdfAction(PdfAction.NEXTPAGE));
      table.AddCell(new Phrase(next));
      Chunk last = new Chunk( ((char)222).ToString(), symbol);
      last.SetAction(new PdfAction(PdfAction.LASTPAGE));
      table.AddCell(new Phrase(last));
      table.TotalWidth = 120;
      
      // Create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Add the table to each page
          PdfContentByte canvas;
          for (int i = 0; i < reader.NumberOfPages; ) {
            canvas = stamper.GetOverContent(++i);
            table.WriteSelectedRows(0, -1, 696, 36, canvas);
          }
        }
        return ms.ToArray();
      }    
    }    
// ===========================================================================
  }
}