/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class NewPage : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("This page will NOT be followed by a blank page!"));
        document.NewPage();
        // we don't add anything to this page: newPage() will be ignored
        document.NewPage();
        document.Add(new Paragraph("This page will be followed by a blank page!"));
        document.NewPage();
        writer.PageEmpty = false;
        document.NewPage();
        document.Add(new Paragraph("The previous page was a blank page!"));
      }
    }
// ===========================================================================
  }
}