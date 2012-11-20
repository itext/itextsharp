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
  public class TableAlignment : MyFirstTable {
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
        table.WidthPercentage = 50;
        table.HorizontalAlignment = Element.ALIGN_LEFT;
        document.Add(table);
        table.HorizontalAlignment = Element.ALIGN_CENTER;
        document.Add(table);
        table.HorizontalAlignment = Element.ALIGN_RIGHT;
        document.Add(table);
      }
    }
// ===========================================================================
  }
}