/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.Com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class NUpTool : IWriter {
// ===========================================================================
    public const String RESULT = "result{0}up.pdf";
// ---------------------------------------------------------------------------     
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        // previous example
        Stationery s = new Stationery();
        byte[] stationary  = s.CreatePdf(s.CreateStationary());
        // reader for the src file
        PdfReader reader = new PdfReader(stationary);
        // initializations
        int pow = 1;
               
        do {
          Rectangle pageSize = reader.GetPageSize(1); 
          Rectangle newSize = (pow % 2) == 0 
            ? new Rectangle(pageSize.Width, pageSize.Height)
            : new Rectangle(pageSize.Height, pageSize.Width)
          ;
          Rectangle unitSize = new Rectangle(pageSize.Width, pageSize.Height);
          for (int i = 0; i < pow; i++) {
            unitSize = new Rectangle(unitSize.Height / 2, unitSize.Width);
          }
          int n = (int)Math.Pow(2, pow);
          int r = (int)Math.Pow(2, pow / 2);
          int c = n / r;           
          
          using (MemoryStream ms = new MemoryStream()) {
            // step 1
            using (Document document = new Document(newSize, 0, 0, 0, 0)) {
              // step 2
              PdfWriter writer = PdfWriter.GetInstance(document, ms);
              // step 3
              document.Open();
              // step 4
              PdfContentByte cb = writer.DirectContent;
              PdfImportedPage page;
              Rectangle currentSize;
              float offsetX, offsetY, factor;
              int total = reader.NumberOfPages;
              
              for (int i = 0; i < total; ) {
                if (i % n == 0) {
                  document.NewPage();
                }
                currentSize = reader.GetPageSize(++i);
                factor = Math.Min(
                    unitSize.Width / currentSize.Width,
                    unitSize.Height / currentSize.Height
                );
                offsetX = unitSize.Width * ((i % n) % c)
                  + (unitSize.Width - (currentSize.Width * factor)) / 2f
                ;
                offsetY = newSize.Height
                  - (unitSize.Height * (((i % n) / c) + 1))
                  + (unitSize.Height - (currentSize.Height * factor)) / 2f
                ;
                page = writer.GetImportedPage(reader, i);
                cb.AddTemplate(page, factor, 0, 0, factor, offsetX, offsetY);
              }
            }
            zip.AddEntry(string.Format(RESULT, n), ms.ToArray());
            ++pow;
          }
        } while (pow < 5); 
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), stationary);
        zip.Save(stream);
      }
    }
// ===========================================================================
  }
}