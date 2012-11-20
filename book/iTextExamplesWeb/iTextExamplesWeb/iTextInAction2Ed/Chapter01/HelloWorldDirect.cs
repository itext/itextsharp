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
  public class HelloWorldDirect : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        var writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContentUnder;
        writer.CompressionLevel =0;
        canvas.SaveState();                               // q
        canvas.BeginText();                               // BT
        canvas.MoveText(36, 788);                         // 36 788 Td
        canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
        canvas.ShowText("HelloWorldDirect");              // (Hello World)Tj
        canvas.EndText();                                 // ET
        canvas.RestoreState();                            // Q
      }
    }
// ===========================================================================
  }
}