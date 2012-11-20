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
using Ionic.Zip;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class Burst : IWriter {
// ===========================================================================
/** Format of the resulting PDF files. */
    public const string RESULT = "timetable_p{0}.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // use one of the previous examples to create a PDF
      MovieTemplates mt = new MovieTemplates();
      // Create a reader
      byte[] pdf = Utility.PdfBytes(mt);
      PdfReader reader = new PdfReader(pdf);
      // loop over all the pages in the original PDF
      int n = reader.NumberOfPages;      
      using (ZipFile zip = new ZipFile()) {
        for (int i = 0; i < n; ) {
          string dest = string.Format(RESULT, ++i);
          using (MemoryStream ms = new MemoryStream()) {
// We'll create as many new PDFs as there are pages
            // step 1
            using (Document document = new Document()) {
              // step 2
              using (PdfCopy copy = new PdfCopy(document, ms)) {
                // step 3
                document.Open();
                // step 4
                copy.AddPage(copy.GetImportedPage(reader, i));
              }
            }
            zip.AddEntry(dest, ms.ToArray());
          }
        }
        zip.AddEntry(Utility.ResultFileName(mt.ToString() + ".pdf"), pdf);
        zip.Save(stream);       
      }
    }
// ===========================================================================
  }
}