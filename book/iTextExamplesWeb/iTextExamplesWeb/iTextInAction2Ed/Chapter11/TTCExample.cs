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
  public class TTCExample : IWriter {
// ===========================================================================
  /** The path to the font. */
    public const string FONT = "c:/windows/fonts/msgothic.ttc";
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
        String[] names = BaseFont.EnumerateTTCNames(FONT);
        for (int i = 0; i < names.Length; i++) {
          bf = BaseFont.CreateFont(
            String.Format("{0}, {1}", FONT, i),
            BaseFont.IDENTITY_H, BaseFont.EMBEDDED
          );
          font = new Font(bf, 12);
          document.Add(new Paragraph("font " + i + ": " + names[i], font));
          document.Add(new Paragraph("Rash\u00f4mon", font));
          document.Add(new Paragraph("Directed by Akira Kurosawa", font));
          document.Add(new Paragraph("\u7f85\u751f\u9580", font));
          document.Add(Chunk.NEWLINE);
        }
      }
    }
// ===========================================================================
  }
}