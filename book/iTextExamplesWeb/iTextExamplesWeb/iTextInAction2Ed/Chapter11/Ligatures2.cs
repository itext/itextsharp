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
  public class Ligatures2 : IWriter {
// ===========================================================================
    /** Correct movie title. */
    public const string MOVIE = 
    "\u0644\u0648\u0631\u0627\u0646\u0633 \u0627\u0644\u0639\u0631\u0628";
    /** Correct movie title. */
    public const string MOVIE_WITH_SPACES = 
    "\u0644 \u0648 \u0631 \u0627 \u0646 \u0633   \u0627 \u0644 \u0639 \u0631 \u0628";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf = BaseFont.CreateFont(
          "c:/windows/fonts/arialuni.ttf", 
          BaseFont.IDENTITY_H, BaseFont.EMBEDDED
        );
        Font font = new Font(bf, 20);
        document.Add(new Paragraph("Movie title: Lawrence of Arabia (UK)"));
        document.Add(new Paragraph("directed by David Lean"));
        document.Add(new Paragraph("Wrong: " + MOVIE, font));
        ColumnText column = new ColumnText(writer.DirectContent);
        column.SetSimpleColumn(36, 730, 569, 36);
        column.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
        column.AddElement(new Paragraph("Wrong: " + MOVIE_WITH_SPACES, font));
        column.AddElement(new Paragraph(MOVIE, font));
        column.Go();
      }
    }
// ===========================================================================
  }
}