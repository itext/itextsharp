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
 * Creates a PDF file: hello_narrow.pdf
 */
namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldNarrow : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      // Using a custom page size
      Rectangle pagesize = new Rectangle(216f, 720f);
      using (Document document = new Document(pagesize, 36f, 72f, 108f, 180f)) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph(
          "Hello World! Hello People! " +
          "Hello Sky! Hello Sun! Hello Moon! Hello Stars!")
        );
      }
    }
// ===========================================================================
  }
}