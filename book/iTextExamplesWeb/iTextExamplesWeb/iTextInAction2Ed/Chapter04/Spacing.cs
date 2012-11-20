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
  public class Spacing : IWriter {
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
        table.WidthPercentage = 100;
        Phrase p = new Phrase(
          "Dr. iText or: How I Learned to Stop Worrying " +
          "and Love the Portable Document Format."
        );
        PdfPCell cell = new PdfPCell(p);
        table.AddCell("default leading / spacing");
        table.AddCell(cell);
        table.AddCell("absolute leading: 20");
        cell.SetLeading(20f, 0f);
        table.AddCell(cell);
        table.AddCell("absolute leading: 3; relative leading: 1.2");
        cell.SetLeading(3f, 1.2f);
        table.AddCell(cell);
        table.AddCell("absolute leading: 0; relative leading: 1.2");
        cell.SetLeading(0f, 1.2f);
        table.AddCell(cell);
        table.AddCell("no leading at all");
        cell.SetLeading(0f, 0f);
        table.AddCell(cell);
        cell = new PdfPCell(new Phrase(
            "Dr. iText or: How I Learned to Stop Worrying and Love PDF"));
        table.AddCell("padding 10");
        cell.Padding = 10;
        table.AddCell(cell);
        table.AddCell("padding 0");
        cell.Padding = 0;
        table.AddCell(cell);
        table.AddCell("different padding for left, right, top and bottom");
        cell.PaddingLeft = 20;
        cell.PaddingRight = 50;
        cell.PaddingTop = 0;
        cell.PaddingBottom = 5;
        table.AddCell(cell);
        p = new Phrase("iText in Action Second Edition");
        table.DefaultCell.Padding = 2;
        table.DefaultCell.UseAscender = false;
        table.DefaultCell.UseDescender = false;
        table.AddCell("padding 2; no ascender, no descender");
        table.AddCell(p);
        table.DefaultCell.UseAscender = true;
        table.DefaultCell.UseDescender = false;
        table.AddCell("padding 2; ascender, no descender");
        table.AddCell(p);
        table.DefaultCell.UseAscender = false;
        table.DefaultCell.UseDescender = true;
        table.AddCell("padding 2; descender, no ascender");
        table.AddCell(p);
        table.DefaultCell.UseAscender = true;
        table.DefaultCell.UseDescender = true;
        table.AddCell("padding 2; ascender and descender");
        cell.Padding = 2;
        cell.UseAscender = true;
        cell.UseDescender = true;
        table.AddCell(p);
        document.Add(table);
      }
    }
// ===========================================================================
  }
}