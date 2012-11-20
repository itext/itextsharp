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
  public class RotationAndColors : IWriter {
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
        table.SetWidths(new int[]{ 1, 3, 3, 3 });
        table.WidthPercentage = 100;
        PdfPCell cell;
        // row 1, cell 1
        cell = new PdfPCell(new Phrase("COLOR"));
        cell.Rotation = 90;
        cell.VerticalAlignment = Element.ALIGN_TOP;
        table.AddCell(cell);
        // row 1, cell 2
        cell = new PdfPCell(new Phrase("red / no borders"));
        cell.Border = Rectangle.NO_BORDER;
        cell.BackgroundColor = BaseColor.RED;
        table.AddCell(cell);
        // row 1, cell 3
        cell = new PdfPCell(new Phrase("green / black bottom border"));
        cell.Border = Rectangle.BOTTOM_BORDER;
        cell.BorderColorBottom = BaseColor.BLACK;
        cell.BorderWidthBottom = 10f;
        cell.BackgroundColor = BaseColor.GREEN;
        table.AddCell(cell);
        // row 1, cell 4
        cell = new PdfPCell(new Phrase(
          "cyan / blue top border + padding"
        ));
        cell.Border  = Rectangle.TOP_BORDER;
        cell.UseBorderPadding = true;
        cell.BorderWidthTop = 5f;
        cell.BorderColorTop = BaseColor.BLUE;
        cell.BackgroundColor = BaseColor.CYAN;
        table.AddCell(cell);
        // row 2, cell 1
        cell = new PdfPCell(new Phrase("GRAY"));
        cell.Rotation = 90;
        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        table.AddCell(cell);
        // row 2, cell 2
        cell = new PdfPCell(new Phrase("0.6"));
        cell.Border = Rectangle.NO_BORDER;
        cell.GrayFill = 0.6f;
        table.AddCell(cell);
        // row 2, cell 3
        cell = new PdfPCell(new Phrase("0.75"));
        cell.Border = Rectangle.NO_BORDER;
        cell.GrayFill = 0.75f;
        table.AddCell(cell);
        // row 2, cell 4
        cell = new PdfPCell(new Phrase("0.9"));
        cell.Border = Rectangle.NO_BORDER;
        cell.GrayFill = 0.9f;
        table.AddCell(cell);
        // row 3, cell 1
        cell = new PdfPCell(new Phrase("BORDERS"));
        cell.Rotation = 90;
        cell.VerticalAlignment = Element.ALIGN_BOTTOM;
        table.AddCell(cell);
        // row 3, cell 2
        cell = new PdfPCell(new Phrase("different borders"));
        cell.BorderWidthLeft = 16f;
        cell.BorderWidthBottom = 12f;
        cell.BorderWidthRight = 8f;
        cell.BorderWidthTop = 4f;
        cell.BorderColorLeft = BaseColor.RED;
        cell.BorderColorBottom = BaseColor.ORANGE;
        cell.BorderColorRight = BaseColor.YELLOW;
        cell.BorderColorTop = BaseColor.GREEN;
        table.AddCell(cell);
        // row 3, cell 3
        cell = new PdfPCell(new Phrase("with correct padding"));
        cell.UseBorderPadding = true;
        cell.BorderWidthLeft = 16f;
        cell.BorderWidthBottom = 12f;
        cell.BorderWidthRight = 8f;
        cell.BorderWidthTop = 4f;
        cell.BorderColorLeft = BaseColor.RED;
        cell.BorderColorBottom = BaseColor.ORANGE;
        cell.BorderColorRight = BaseColor.YELLOW;
        cell.BorderColorTop = BaseColor.GREEN;
        table.AddCell(cell);
        // row 3, cell 4
        cell = new PdfPCell(new Phrase("red border"));
        cell.BorderWidth = 8f;
        cell.BorderColor = BaseColor.RED;
        table.AddCell(cell);
        document.Add(table);
      }
    }
// ===========================================================================
  }
}