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

/**
 * A possible way to create a document in the landscape format.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldLandscape1 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      // landscape format using the rotate() method
      using (Document document = new Document(PageSize.LETTER.Rotate())) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("HelloWorldLandscape1"));
      }
    }
// ===========================================================================
  }
}