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
  public class EncodingExample : IWriter {
// ===========================================================================
    /** The path to the font. */
    public const string FONT = "c:/windows/fonts/arialbd.ttf";
    /** Movie information. */
    public readonly string[][] MOVIES = {
      new string[] {
        "Cp1252",
        "A Very long Engagement (France)",
        "directed by Jean-Pierre Jeunet",
        "Un long dimanche de fian\u00e7ailles"
      },
      new string[] {
        "Cp1250",
        "No Man's Land (Bosnia-Herzegovina)",
        "Directed by Danis Tanovic",
        "Nikogar\u0161nja zemlja"
      },
      new string[] {
        "Cp1251",
        "You I Love (Russia)",
        "directed by Olga Stolpovskaja and Dmitry Troitsky",
        "\u042f \u043b\u044e\u0431\u043b\u044e \u0442\u0435\u0431\u044f"
      },
      new string[] {
        "Cp1253",
        "Brides (Greece)",
        "directed by Pantelis Voulgaris",
        "\u039d\u03cd\u03c6\u03b5\u03c2"
      }
    };
// ---------------------------------------------------------------------------
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf;
        for (int i = 0; i < 4; i++) {
          bf = BaseFont.CreateFont(FONT, MOVIES[i][0], BaseFont.EMBEDDED);
          document.Add(new Paragraph(
            "Font: " + bf.PostscriptFontName
            + " with encoding: " + bf.Encoding
          ));
          document.Add(new Paragraph(MOVIES[i][1]));
          document.Add(new Paragraph(MOVIES[i][2]));
          document.Add(new Paragraph(MOVIES[i][3], new Font(bf, 12)));
          document.Add(Chunk.NEWLINE);
        } 
      }
    }
// ===========================================================================
  }
}