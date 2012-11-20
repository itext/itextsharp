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
  public class XMen : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        string RESOURCE = Utility.ResourcePosters;
        // we'll use 4 images in this example
        Image[] img = {
          Image.GetInstance(Path.Combine(RESOURCE, "0120903.jpg")),
          Image.GetInstance(Path.Combine(RESOURCE, "0290334.jpg")),
          Image.GetInstance(Path.Combine(RESOURCE, "0376994.jpg")),
          Image.GetInstance(Path.Combine(RESOURCE, "0348150.jpg"))
        };
        // Creates a table with 6 columns
        PdfPTable table = new PdfPTable(6);
        table.WidthPercentage = 100;
        // first movie
        table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
        table.DefaultCell.VerticalAlignment = Element.ALIGN_TOP;
        table.AddCell("X-Men");
        // we wrap he image in a PdfPCell
        PdfPCell cell = new PdfPCell(img[0]);
        table.AddCell(cell);
        // second movie
        table.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
        table.AddCell("X2");
        // we wrap the image in a PdfPCell and let iText scale it
        cell = new PdfPCell(img[1], true);
        table.AddCell(cell);
        // third movie
        table.DefaultCell.VerticalAlignment = Element.ALIGN_BOTTOM;
        table.AddCell("X-Men: The Last Stand");
        // we add the image with addCell()
        table.AddCell(img[2]);
        // fourth movie
        table.AddCell("Superman Returns");
        cell = new PdfPCell();
        // we add it with addElement(); it can only take 50% of the width.
        img[3].WidthPercentage = 50;
        cell.AddElement(img[3]);
        table.AddCell(cell);
        // we complete the table (otherwise the last row won't be rendered)
        table.CompleteRow();
        document.Add(table);
      }
    }
// ===========================================================================
  }
}