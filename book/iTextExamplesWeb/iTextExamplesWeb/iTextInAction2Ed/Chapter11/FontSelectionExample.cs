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
  public class FontSelectionExample : IWriter {
// ===========================================================================
    /** Some text */
    public readonly string TEXT =
    "These are the protagonists in 'Hero', a movie by Zhang Yimou:\n"
    + "\u7121\u540d (Nameless), \u6b98\u528d (Broken Sword), "
    + "\u98db\u96ea (Flying Snow), \u5982\u6708 (Moon), "
    + "\u79e6\u738b (the King), and \u9577\u7a7a (Sky).";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4)) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        FontSelector selector = new FontSelector();
        selector.AddFont(FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12));
        selector.AddFont(FontFactory.GetFont(
          "MSung-Light", "UniCNS-UCS2-H", BaseFont.NOT_EMBEDDED
        ));
        Phrase ph = selector.Process(TEXT);
        document.Add(new Paragraph(ph));
      }
    }
// ===========================================================================
  }
}