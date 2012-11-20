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
  public class CellHeights : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfPTable table = new PdfPTable(2);
        // a long phrase
        Phrase p = new Phrase(
          "Dr. iText or: How I Learned to Stop Worrying and Love PDF."
        );
        PdfPCell cell = new PdfPCell(p);
        // the prhase is wrapped
        table.AddCell("wrap");
        cell.NoWrap = false;
        table.AddCell(cell);
        // the phrase isn't wrapped
        table.AddCell("no wrap");
        cell.NoWrap = true;
        table.AddCell(cell);
        // a long phrase with newlines
        p = new Phrase(
            "Dr. iText or:\nHow I Learned to Stop Worrying\nand Love PDF.");
        cell = new PdfPCell(p);
        // the phrase fits the fixed height
        table.AddCell("fixed height (more than sufficient)");
        cell.FixedHeight = 72f;
        table.AddCell(cell);
        // the phrase doesn't fit the fixed height
        table.AddCell("fixed height (not sufficient)");
        cell.FixedHeight = 36f;
        table.AddCell(cell);
        // The minimum height is exceeded
        table.AddCell("minimum height");
        cell = new PdfPCell(new Phrase("Dr. iText"));
        cell.MinimumHeight = 36f;
        table.AddCell(cell);
        // The last row is extended
        table.ExtendLastRow = true;
        table.AddCell("extend last row");
        table.AddCell(cell);
        document.Add(table);
      }
    }
// ===========================================================================
  }
}