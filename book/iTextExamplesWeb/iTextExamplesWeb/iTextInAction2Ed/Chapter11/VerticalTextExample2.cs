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
  public class VerticalTextExample2 : VerticalTextExample1 {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(new Rectangle(420, 600))) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf = BaseFont.CreateFont(
          "KozMinPro-Regular", "Identity-V", BaseFont.NOT_EMBEDDED
        );
        Font font = new Font(bf, 20);
        VerticalText vt = new VerticalText(writer.DirectContent);
        vt.SetVerticalLayout(390, 570, 540, 12, 30);
        font = new Font(bf, 20);
        vt.AddText(new Phrase(ConvertCIDs(TEXT1), font));
        vt.Go();
        vt.Alignment = Element.ALIGN_RIGHT;
        vt.AddText(new Phrase(ConvertCIDs(TEXT2), font));
        vt.Go();
      }
    }
// ---------------------------------------------------------------------------
/**
 * Converts the CIDs of the horizontal characters of a String
 * into a String with vertical characters.
 * @param text The String with the horizontal characters
 * @return A String with vertical characters
 */
    public string ConvertCIDs(string text) {
      char[] cid = text.ToCharArray();
      for (int k = 0; k < cid.Length; ++k) {
        char c = cid[k];
        cid[k] = c == '\n' ? '\uff00' : (char) (c - ' ' + 8720);
      }
      return new String(cid);
    }
// ===========================================================================
  }
}