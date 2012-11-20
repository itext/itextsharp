/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO; 
using System.Collections.Generic; 
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
/*
 * this creates a bit of overhead, so we only run on localhost;
*/
namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class DataSheets2 : DataSheets1 {
// ===========================================================================
    public new const string RESULT = "datasheets2.pdf";
// ---------------------------------------------------------------------------
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) { 
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            using (PdfSmartCopy copy = new PdfSmartCopy(document, ms)) {
              // step 3
              document.Open();
              // step 4
              AddDataSheets(copy);
            }
          }
          zip.AddEntry(RESULT, ms.ToArray());        
        }
        zip.AddFile(DATASHEET_PATH, "");
        zip.Save(stream);
      }    
    } 
// ===========================================================================
  }
}