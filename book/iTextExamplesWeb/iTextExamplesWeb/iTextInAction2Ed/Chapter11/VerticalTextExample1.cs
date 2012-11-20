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
  public class VerticalTextExample1 : IWriter {
// ===========================================================================
    /** A Japanese String. */
    public const string MOVIE = "\u4e03\u4eba\u306e\u4f8d";
    /** The facts. */
    public const string TEXT1
    = "You embarrass me. You're overestimating me. "
    + "Listen, I'm not a man with any special skill, "
    + "but I've had plenty of experience in battles; losing battles, all of them.";
    /** The conclusion. */
    public const string TEXT2
    = "In short, that's all I am. Drop such an idea for your own good.";
// ---------------------------------------------------------------------------
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document(new Rectangle(420, 600))) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        BaseFont bf = BaseFont.CreateFont(
          "KozMinPro-Regular", "UniJIS-UCS2-V", BaseFont.NOT_EMBEDDED
        );
        Font font = new Font(bf, 20);
        VerticalText vt = new VerticalText(writer.DirectContent);
        vt.SetVerticalLayout(390, 570, 540, 12, 30);
        vt.AddText(new Chunk(MOVIE, font));
        vt.Go();
        vt.AddText(new Phrase(TEXT1, font));
        vt.Go();
        vt.Alignment = Element.ALIGN_RIGHT;
        vt.AddText(new Phrase(TEXT2, font));
        vt.Go();
      }
    }
// ===========================================================================
  }
}