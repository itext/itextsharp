/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class RadioButtons : IWriter {
// ===========================================================================
    public readonly string[] LANGUAGES = {
      "English", "German", "French", "Spanish", "Dutch"
    };
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte cb = writer.DirectContent;
        BaseFont bf = BaseFont.CreateFont(
          BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED
        );
        
        PdfFormField radiogroup = PdfFormField.CreateRadioButton(
          writer, true
        );
        radiogroup.FieldName = "language";
        Rectangle rect = new Rectangle(40, 806, 60, 788);
        RadioCheckField radio;
        PdfFormField radiofield;
        for (int page = 0; page < LANGUAGES.Length; ) {
          radio = new RadioCheckField(writer, rect, null, LANGUAGES[page]);
          radio.BackgroundColor = new GrayColor(0.8f);
          radiofield = radio.RadioField;
          radiofield.PlaceInPage = ++page;
          radiogroup.AddKid(radiofield);
        }
        writer.AddAnnotation(radiogroup);
        for (int i = 0; i < LANGUAGES.Length; i++) {
          cb.BeginText();
          cb.SetFontAndSize(bf, 18);
          cb.ShowTextAligned(Element.ALIGN_LEFT, LANGUAGES[i], 70, 790, 0);
          cb.EndText();
          document.NewPage();
        }
      }
    }
// ===========================================================================
  }
}