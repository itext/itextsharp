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
  public class NestedTable : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfPTable table = new PdfPTable(4);
        PdfPTable nested1 = new PdfPTable(2);
        nested1.AddCell("1.1");
        nested1.AddCell("1.2");
        PdfPTable nested2 = new PdfPTable(1);
        nested2.AddCell("12.1");
        nested2.AddCell("12.2");
        for (int k = 0; k < 16; ++k) {
          if (k == 1) {
            table.AddCell(nested1);
          } else if (k == 12) {
            table.AddCell(new PdfPCell(nested2));
          } else {
            table.AddCell("cell " + k);
          }
        }
        document.Add(table);
      }
    }
// ===========================================================================
  }
}