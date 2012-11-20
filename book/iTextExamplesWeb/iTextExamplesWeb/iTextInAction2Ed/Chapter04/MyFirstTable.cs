/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class MyFirstTable : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(CreateFirstTable());
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates our first table
     * @return our first table
     */
    public static PdfPTable CreateFirstTable() {
    // a table with three columns
      PdfPTable table = new PdfPTable(3);
      // the cell object
      PdfPCell cell;
      // we add a cell with colspan 3
      cell = new PdfPCell(new Phrase("Cell with colspan 3"));
      cell.Colspan = 3;
      table.AddCell(cell);
      // now we add a cell with rowspan 2
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      // we add the four remaining cells with addCell()
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }
// ===========================================================================
  }
}