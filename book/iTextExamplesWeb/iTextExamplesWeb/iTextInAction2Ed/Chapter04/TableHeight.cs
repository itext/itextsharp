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
  public class TableHeight : MyFirstTable {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfPTable table = CreateFirstTable();
        document.Add(new Paragraph(string.Format(
          "Table height before document.Add(): {0}",
          table.TotalHeight)
        ));
        document.Add(new Paragraph(
            string.Format("Height of the first row: {0}",
                table.GetRowHeight(0))
         ));
        document.Add(table);
        document.Add(new Paragraph(string.Format(
          "Table height after document.Add(): {0}",
          table.TotalHeight
        )));
        document.Add(new Paragraph(string.Format(
          "Height of the first row: {0}",
          table.GetRowHeight(0)
        )));
        table = CreateFirstTable();
        document.Add(new Paragraph( string.Format(
          "Table height before setTotalWidth(): {0}",
          table.TotalHeight
        )));
        document.Add(new Paragraph(string.Format(
          "Height of the first row: {0}",
          table.GetRowHeight(0)
        )));
        table.TotalWidth = 50;
        table.LockedWidth = true;
        document.Add( new Paragraph(string.Format(
          "Table height after setTotalWidth(): {0}",
          table.TotalHeight
        )));
        document.Add(new Paragraph(string.Format(
          "Height of the first row: {0}",
          table.GetRowHeight(0)
        )));
        document.Add(table);
      }
    }
// ===========================================================================
  }
}