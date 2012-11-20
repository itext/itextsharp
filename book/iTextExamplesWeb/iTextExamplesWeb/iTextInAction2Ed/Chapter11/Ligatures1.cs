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
  public class Ligatures1 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf = BaseFont.CreateFont(
          "c:/windows/fonts/arial.ttf", BaseFont.CP1252, BaseFont.EMBEDDED
        );
        Font font = new Font(bf, 12);
        document.Add(new Paragraph(
          "Movie title: Love at First Hiccough (Denmark)", font
        ));
        document.Add(new Paragraph("directed by Tomas Villum Jensen", font));
        document.Add(new Paragraph("K\u00e6rlighed ved f\u00f8rste hik", font));
        document.Add(new Paragraph(
          Ligaturize("Kaerlighed ved f/orste hik"), font
        ));
      }
    }
// ---------------------------------------------------------------------------
  /**
   * Method that makes the ligatures for the combinations 'a' and 'e'
   * and for '/' and 'o'.
   * @param s a String that may have the combinations ae or /o
   * @return a String where the combinations are replaced by a unicode character
   */
    public String Ligaturize(string s) {
      int pos;
      //while ((pos = s.IndexOf("ae")) > -1) {
      //  s = s.Substring(0, pos) + '\u00e6' + s.Substring(pos + 2);
      //}
      while ((pos = s.IndexOf("/o")) > -1) {
        s = s.Substring(0, pos) + "\u00f8" + s.Substring(pos + 2);
      }
      return s;
    }    
// ===========================================================================
  }
}