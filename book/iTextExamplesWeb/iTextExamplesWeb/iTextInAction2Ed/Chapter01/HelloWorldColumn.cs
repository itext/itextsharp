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

namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldColumn : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        var writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // we set the compression to 0 so that we can read the PDF syntax
        writer.CompressionLevel = 0;
        // writes something to the direct content using a convenience method
        Phrase hello = new Phrase("HelloWorldColumn");
        PdfContentByte canvas = writer.DirectContentUnder;
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT,
          hello, 36, 788, 0
        );
      }
    }
// ===========================================================================
  }
}