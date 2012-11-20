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
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class ImportingPages1 : IWriter {
// ===========================================================================
    public const string RESULT = "time_table_imported1.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // Use old example to create PDF
      MovieTemplates mt = new MovieTemplates();
      byte[] pdf = Utility.PdfBytes(mt);
      using (ZipFile zip = new ZipFile()) { 
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            PdfPTable table = new PdfPTable(2);
            PdfReader reader = new PdfReader(pdf);
            int n = reader.NumberOfPages;
            PdfImportedPage page;
            for (int i = 1; i <= n; i++) {
              page = writer.GetImportedPage(reader, i);
              table.AddCell(Image.GetInstance(page));
            }
            document.Add(table);
          }
          zip.AddEntry(RESULT, ms.ToArray());           
        }
        zip.AddEntry(Utility.ResultFileName(mt.ToString() + ".pdf"), pdf);
        zip.Save(stream);
      }
   }
// ===========================================================================
  }
}