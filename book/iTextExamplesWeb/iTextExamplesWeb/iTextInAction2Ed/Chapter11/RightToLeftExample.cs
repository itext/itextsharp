/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter11 {
  public class RightToLeftExample : IWriter {
// ===========================================================================
    /** A movie title. */
    public const string MOVIE =
    "\u05d4\u05d0\u05e1\u05d5\u05e0\u05d5\u05ea \u05e9\u05dc \u05e0\u05d9\u05e0\u05d4";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf = BaseFont.CreateFont(
          "c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, true
        );
        Font font = new Font(bf, 14);
        document.Add(new Paragraph("Movie title: Nina's Tragedies"));
        document.Add(new Paragraph("directed by Savi Gabizon"));
        ColumnText column = new ColumnText(writer.DirectContent);
        column.SetSimpleColumn(36, 770, 569, 36);
        column.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
        column.AddElement(new Paragraph(MOVIE, font));
        column.Go();
      }
    }
// ===========================================================================
  }
}