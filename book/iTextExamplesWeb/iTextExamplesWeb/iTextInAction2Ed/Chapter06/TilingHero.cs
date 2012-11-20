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

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class TilingHero : IWriter {
// ===========================================================================
    public const String RESOURCE = "Hero.pdf";
    public const String RESULT = "superman.pdf";
// ---------------------------------------------------------------------------         
    public void Write(Stream stream) {
      // Creating a reader
      string resource = Path.Combine(Utility.ResourcePdf, RESOURCE);
      PdfReader reader = new PdfReader(resource);
      Rectangle pagesize = reader.GetPageSizeWithRotation(1); 
      using (ZipFile zip = new ZipFile()) {
        // step 1
        using (MemoryStream ms = new MemoryStream()) {
          using (Document document = new Document(pagesize)) {
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            PdfContentByte content = writer.DirectContent;
            PdfImportedPage page = writer.GetImportedPage(reader, 1);
            // adding the same page 16 times with a different offset
            float x, y;
            for (int i = 0; i < 16; i++) {
              x = -pagesize.Width * (i % 4);
              y = pagesize.Height * (i / 4 - 3);
              content.AddTemplate(page, 4, 0, 0, 4, x, y);
              document.NewPage();
            }
          }
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.AddFile(resource, "");
        zip.Save(stream);
      }
    }
// ===========================================================================
  }
}