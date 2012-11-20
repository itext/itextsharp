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
 * Creates a PDF with the biggest possible page size.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldMaximum : IWriter {
// ===========================================================================
    /**
     * Creates a PDF file: hello_maximum.pdf
     * Important notice: the PDF is valid (in conformance with
     * ISO-32000), but some PDF viewers won't be able to render
     * the PDF correctly due to their own limitations.
     */
    public void Write(Stream stream) {
      // step 1
      // Specifying the page size
      using (Document document = new Document(new Rectangle(14400, 14400))) {
        // step 2        
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // changes the user unit
        writer.Userunit = 75000f;        
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("HelloWorldMaximum"));
      }
    }
// ===========================================================================
  }
}