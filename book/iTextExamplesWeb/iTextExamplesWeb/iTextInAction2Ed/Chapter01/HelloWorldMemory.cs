/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloWorldMemory : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      HttpContext current = HttpContext.Current;
// running from: [1] web context or [2] command line?
      if (current != null) {
// [1]      
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("HelloWorldMemory"));      
          }
          HttpContext.Current.Response.BinaryWrite(ms.ToArray());
        }
      }
// [2]      
      else {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, stream);
          // step 3
          document.Open();
          // step 4
          document.Add(new Paragraph("HelloWorldMemory"));      
        }
      }
    }
// ===========================================================================
  }
}