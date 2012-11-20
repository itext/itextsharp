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
  public class Diacritics2 : IWriter {
// ===========================================================================
    /** A movie title. */
    public const string MOVIE = "Tomten \u00a8ar far till alla barnen";
    /** Fonts */
    public readonly string[] FONTS = {
      "c:/windows/fonts/arial.ttf",
      "c:/windows/fonts/cour.ttf"
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
        document.Add(new Paragraph("Movie title: In Bed With Santa (Sweden)"));
        document.Add(new Paragraph("directed by Kjell Sundvall"));
        BaseFont bf = BaseFont.CreateFont(
          FONTS[0], BaseFont.CP1252, BaseFont.EMBEDDED
        );
        Font font = new Font(bf, 12);
        bf.SetCharAdvance('\u00a8', -100);
        document.Add(new Paragraph(MOVIE, font));
        bf = BaseFont.CreateFont(FONTS[1], BaseFont.CP1252, BaseFont.EMBEDDED);
        bf.SetCharAdvance('\u00a8', 0);
        font = new Font(bf, 12);
        document.Add(new Paragraph(MOVIE, font));
      }
    }
// ===========================================================================
  }
}