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
  public class CJKExample : IWriter {
// ===========================================================================
    /** Movies, their director and original title */
    public readonly string[][] MOVIES = {
      new string[] {
        "STSong-Light", "UniGB-UCS2-H",
        "Movie title: House of The Flying Daggers (China)",
        "directed by Zhang Yimou",
        "\u5341\u950a\u57cb\u4f0f"
      },
      new string[] {
        "KozMinPro-Regular", "UniJIS-UCS2-H",
        "Movie title: Nobody Knows (Japan)",
        "directed by Hirokazu Koreeda",
        "\u8ab0\u3082\u77e5\u3089\u306a\u3044"
      },
      new string[] {
        "HYGoThic-Medium", "UniKS-UCS2-H",
        "Movie title: '3-Iron' aka 'Bin-jip' (South-Korea)",
        "directed by Kim Ki-Duk",
        "\ube48\uc9d1"
      }
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
        for (int i = 0; i < 3; i++) {
          bf = BaseFont.CreateFont(
            MOVIES[i][0], MOVIES[i][1], BaseFont.NOT_EMBEDDED
          );
          font = new Font(bf, 12);
          document.Add(new Paragraph(bf.PostscriptFontName, font));
          for (int j = 2; j < 5; j++) {
            document.Add(new Paragraph(MOVIES[i][j], font));
          }
          document.Add(Chunk.NEWLINE);
        }        
      }
    }
// ===========================================================================
  }
}