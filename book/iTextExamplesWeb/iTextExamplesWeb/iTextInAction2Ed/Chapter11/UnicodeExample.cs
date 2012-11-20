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
  public class UnicodeExample : EncodingExample {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf;
        for (int i = 0; i < 4; i++) {
          bf = BaseFont.CreateFont(FONT, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
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