/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ReadOutLoud : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.SetTagged();
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;
        BaseFont bf = BaseFont.CreateFont(
          BaseFont.HELVETICA,
          BaseFont.CP1252, BaseFont.NOT_EMBEDDED
        );
        BaseFont bf2 = BaseFont.CreateFont(
          "c:/windows/fonts/msgothic.ttc,1",
          BaseFont.IDENTITY_H, BaseFont.EMBEDDED
        );

        PdfStructureTreeRoot root = writer.StructureTreeRoot;
        PdfStructureElement div = new PdfStructureElement(
          root, new PdfName("Div")
        );
        PdfDictionary dict;

        cb.BeginMarkedContentSequence(div);

        cb.BeginText();
        cb.MoveText(36, 788);
        cb.SetFontAndSize(bf, 12);
        cb.SetLeading(18);
        cb.ShowText("These are some famous movies by Stanley Kubrick: ");
        dict = new PdfDictionary();
        dict.Put(PdfName.E, new PdfString("Doctor"));
        cb.BeginMarkedContentSequence(new PdfName("Span"), dict, true);
        cb.NewlineShowText("Dr.");
        cb.EndMarkedContentSequence();
        cb.ShowText(
          " Strangelove or: How I Learned to Stop Worrying and Love the Bomb."
        );
        dict = new PdfDictionary();
        dict.Put(PdfName.E, new PdfString("Eyes Wide Shut."));
        cb.BeginMarkedContentSequence(new PdfName("Span"), dict, true);
        cb.NewlineShowText("EWS");
        cb.EndMarkedContentSequence();
        cb.EndText();
        dict = new PdfDictionary();
        dict.Put(PdfName.LANG, new PdfString("en-us"));
        dict.Put(new PdfName("Alt"), new PdfString("2001: A Space Odyssey."));
        cb.BeginMarkedContentSequence(new PdfName("Span"), dict, true);
        Image img = Image.GetInstance(Path.Combine(
          Utility.ResourcePosters, "0062622.jpg"
        ));
        img.ScaleToFit(1000, 100);
        img.SetAbsolutePosition(36, 640);
        cb.AddImage(img);
        cb.EndMarkedContentSequence();

        cb.BeginText();
        cb.MoveText(36, 620);
        cb.SetFontAndSize(bf, 12);
        cb.ShowText("This is a movie by Akira Kurosawa: ");
        dict = new PdfDictionary();
        dict.Put(PdfName.ACTUALTEXT, new PdfString("Seven Samurai."));
        cb.BeginMarkedContentSequence(new PdfName("Span"), dict, true);
        cb.SetFontAndSize(bf2, 12);
        cb.ShowText("\u4e03\u4eba\u306e\u4f8d");
        cb.EndMarkedContentSequence();
        cb.EndText();
        
        cb.EndMarkedContentSequence();

      }
    }
// ===========================================================================
  }
}