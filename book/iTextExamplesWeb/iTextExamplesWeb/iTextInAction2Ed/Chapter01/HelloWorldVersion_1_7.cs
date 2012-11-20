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
 * Creates a Hello World in PDF version 1.7
 */
namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldVersion_1_7 : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        // Creating a PDF 1.7 document
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PdfVersion = PdfWriter.VERSION_1_7;        
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("HelloWorldVersion_1_7"));
      }
    }
// ===========================================================================
  }
}