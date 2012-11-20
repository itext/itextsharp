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
using iTextSharp.text.pdf.draw;

namespace kuujinbo.iTextInAction2Ed.Chapter11 {
  public class FontTypes : IWriter {
// ===========================================================================
    /** Some text. */
    public const string TEXT
        = "quick brown fox jumps over the lazy dog\nQUICK BROWN FOX JUMPS OVER THE LAZY DOG";
    
/** Paths to and encodings of fonts we're going to use in this example;
 * you may have to comment out some of the fonts to run example 
*/
    public readonly string[][] FONTS = {
      new string[] {BaseFont.HELVETICA, BaseFont.WINANSI},
      new string[] {Path.Combine(Utility.ResourceFonts, "cmr10.afm"), BaseFont.WINANSI},
      new string[] {Path.Combine(Utility.ResourceFonts, "cmr10.pfm"), BaseFont.WINANSI},
      //new string[] {"c:/windows/fonts/ARBLI__.TTF", BaseFont.WINANSI},
      new string[] {"c:/windows/fonts/arial.ttf", BaseFont.WINANSI},
      new string[] {"c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H},
      new string[] {Path.Combine(Utility.ResourceFonts, "Puritan2.otf"), BaseFont.WINANSI},
      new string[] {"c:/windows/fonts/msgothic.ttc,0", BaseFont.IDENTITY_H},
      new string[] {"KozMinPro-Regular", "UniJIS-UCS2-H"}
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
        for (int i = 0; i < FONTS.Length; i++) {
          bf = BaseFont.CreateFont(FONTS[i][0], FONTS[i][1], BaseFont.EMBEDDED);
          document.Add(new Paragraph(String.Format("Font file: {0} with encoding {1}", 
            FONTS[i][0], FONTS[i][1]
          )));
          document.Add(new Paragraph(
            String.Format("iText class: {0}", bf.GetType().ToString()
          )));
          font = new Font(bf, 12);
          document.Add(new Paragraph(TEXT, font));
          document.Add(new LineSeparator(0.5f, 100, null, 0, -5));
        }
      }
    }
// ===========================================================================
  }
}