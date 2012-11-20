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
  public class EncodingNames : IWriter {
// ===========================================================================
/** The path to the font. 
 *  you may have to comment out some of the fonts to run example 
*/
    public readonly string[] FONT = {
      //"c:/windows/fonts/ARBLI__.TTF",
      Path.Combine(Utility.ResourceFonts, "Puritan2.otf"),
      "c:/windows/fonts/arialbd.ttf"
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
        for (int i = 0; i < FONT.Length; ++i) {
          BaseFont bf = BaseFont.CreateFont(FONT[i],  BaseFont.WINANSI, BaseFont.EMBEDDED);
          document.Add(new Paragraph("PostScript name: " + bf.PostscriptFontName));
          document.Add(new Paragraph("Available code pages:"));
          string[] encoding = bf.CodePagesSupported;
          for (int j = 0; j < encoding.Length; j++) {
            document.Add(new Paragraph("encoding[" + j + "] = " + encoding[j]));
          }
          document.Add(Chunk.NEWLINE);        
        }
      }
    }
// ===========================================================================
  }
}