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
  public class ColumnWidths : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfPTable table = CreateTable1();
        document.Add(table);
        table = CreateTable2();
        table.SpacingBefore = 5;
        table.SpacingAfter = 5;
        document.Add(table);
        table = CreateTable3();
        document.Add(table);
        table = CreateTable4();
        table.SpacingBefore = 5;
        table.SpacingAfter = 5;
        document.Add(table);
        table = CreateTable5();
        document.Add(table);
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table; widths are set with setWidths().
     * @return a PdfPTable
     * @throws DocumentException
     */
    public static PdfPTable CreateTable1() {
      PdfPTable table = new PdfPTable(3);
      table.WidthPercentage  =288 / 5.23f;
      table.SetWidths(new int[]{2, 1, 1});
      PdfPCell cell;
      cell = new PdfPCell(new Phrase("Table 1"));
      cell.Colspan = 3;
      table.AddCell(cell);
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table; widths are set with setWidths().
     * @return a PdfPTable
     * @throws DocumentException
     */
    public static PdfPTable CreateTable2() {
      PdfPTable table = new PdfPTable(3);
      table.TotalWidth = 288;
      table.LockedWidth = true;
      table.SetWidths(new float[]{2, 1, 1});
      PdfPCell cell;
      cell = new PdfPCell(new Phrase("Table 2"));
      cell.Colspan = 3;
      table.AddCell(cell);
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table; widths are set in the constructor.
     * @return a PdfPTable
     * @throws DocumentException
     */
    public static PdfPTable CreateTable3() {
      PdfPTable table = new PdfPTable(new float[]{ 2, 1, 1 });
      table.WidthPercentage = 55.067f;
      PdfPCell cell;
      cell = new PdfPCell(new Phrase("Table 3"));
      cell.Colspan = 3;
      table.AddCell(cell);
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table; widths are set with special setWidthPercentage() method.
     * @return a PdfPTable
     */
    public static PdfPTable CreateTable4() {
      PdfPTable table = new PdfPTable(3);
      Rectangle rect = new Rectangle(523, 770);
      table.SetWidthPercentage(new float[]{ 144, 72, 72 }, rect);
      PdfPCell cell;
      cell = new PdfPCell(new Phrase("Table 4"));
      cell.Colspan =3;
      table.AddCell(cell);
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a table; widths are set with setTotalWidth().
     * @return a PdfPTable
     */
    public static PdfPTable CreateTable5() {
      PdfPTable table = new PdfPTable(3);
      table.SetTotalWidth(new float[]{ 144, 72, 72 });
      table.LockedWidth = true;
      PdfPCell cell;
      cell = new PdfPCell(new Phrase("Table 5"));
      cell.Colspan = 3 ;
      table.AddCell(cell);
      cell = new PdfPCell(new Phrase("Cell with rowspan 2"));
      cell.Rowspan = 2;
      table.AddCell(cell);
      table.AddCell("row 1; cell 1");
      table.AddCell("row 1; cell 2");
      table.AddCell("row 2; cell 1");
      table.AddCell("row 2; cell 2");
      return table;
    }    
// ===========================================================================
  }
}