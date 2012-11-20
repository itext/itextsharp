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
  public class FontFactoryExample : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Font font = FontFactory.GetFont("Times-Roman");
        document.Add(new Paragraph("Times-Roman", font));
        Font fontbold = FontFactory.GetFont("Times-Roman", 12, Font.BOLD);
        document.Add(new Paragraph("Times-Roman, Bold", fontbold));
        document.Add(Chunk.NEWLINE);
        FontFactory.Register("c:/windows/fonts/garabd.ttf", "my_bold_font");
        Font myBoldFont = FontFactory.GetFont("my_bold_font");
        BaseFont bf = myBoldFont.BaseFont;
        document.Add(new Paragraph(bf.PostscriptFontName, myBoldFont));
        String[][] name = bf.FullFontName;
        for (int i = 0; i < name.Length; i++) {
          document.Add(new Paragraph(
            name[i][3] + " (" + name[i][0]
            + "; " + name[i][1] + "; " + name[i][2] + ")"
          ));
        }
        Font myBoldFont2 = FontFactory.GetFont("Garamond vet");
        document.Add(new Paragraph("Garamond Vet", myBoldFont2));
        document.Add(Chunk.NEWLINE);
        document.Add(new Paragraph("Registered fonts:"));
        FontFactory.RegisterDirectory(Utility.ResourceFonts); 
/*               
        string fontDirectory = Utility.ResourceFonts;
        FontFactory.RegisterDirectory(
          fontDirectory.Substring(0, fontDirectory.Length - 3
        ));
 */
        foreach (String f in FontFactory.RegisteredFonts) {
          document.Add(new Paragraph(
            f, FontFactory.GetFont(f, "", BaseFont.EMBEDDED
          )));
        }
        document.Add(Chunk.NEWLINE);
        Font cmr10 = FontFactory.GetFont("cmr10");
        cmr10.BaseFont.PostscriptFontName = "Computer Modern Regular";
        Font computerModern = FontFactory.GetFont(
          "Computer Modern Regular", "", BaseFont.EMBEDDED
        );
        document.Add(new Paragraph("Computer Modern", computerModern));
        document.Add(Chunk.NEWLINE);
        FontFactory.RegisterDirectories();
        foreach (String f in FontFactory.RegisteredFamilies) {
          document.Add(new Paragraph(f));
        }
        document.Add(Chunk.NEWLINE);
        Font garamond = FontFactory.GetFont(
          "garamond", BaseFont.WINANSI, BaseFont.EMBEDDED
        );
        document.Add(new Paragraph("Garamond", garamond));
        Font garamondItalic = FontFactory.GetFont(
          "Garamond", BaseFont.WINANSI, BaseFont.EMBEDDED, 12, Font.ITALIC
        );
        document.Add(new Paragraph("Garamond-Italic", garamondItalic));
      }
    }
// ===========================================================================
  }
}