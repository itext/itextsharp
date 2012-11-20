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
  public class Diacritics1 : IWriter {
// ===========================================================================
    /** A movie title. */
    public const string MOVIE = 
    "\u0e1f\u0e49\u0e32\u0e17\u0e30\u0e25\u0e32\u0e22\u0e42\u0e08\u0e23";
    /** Movie poster */
    public static string POSTER = Path.Combine(
      Utility.ResourcePosters, "0269217.jpg"
    );
    /** Fonts */
    public readonly string[] FONTS = {
      "c:/windows/fonts/angsa.ttf",
      "c:/windows/fonts/arialuni.ttf"
    };
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf;
        Font font;
        Image img = Image.GetInstance(POSTER);
        img.ScalePercent(50);
        img.BorderWidth = 18f;
        img.Border = Image.BOX;
        img.BorderColor = GrayColor.GRAYWHITE;
        img.Alignment = Element.ALIGN_LEFT | Image.TEXTWRAP;
        document.Add(img);
        document.Add(new Paragraph(
          "Movie title: Tears of the Black Tiger (Thailand)"
        ));
        document.Add(new Paragraph("directed by Wisit Sasanatieng"));
        for (int i = 0; i < 2; i++) {
          bf = BaseFont.CreateFont(
            FONTS[i], BaseFont.IDENTITY_H, BaseFont.EMBEDDED
          );
          document.Add(new Paragraph("Font: " + bf.PostscriptFontName));
          font = new Font(bf, 20);
          document.Add(new Paragraph(MOVIE, font));
        }
      }
    }
// ===========================================================================
  }
}