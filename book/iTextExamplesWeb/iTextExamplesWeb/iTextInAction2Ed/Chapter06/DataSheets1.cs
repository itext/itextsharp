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
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class DataSheets1 : FillDataSheet {
// ===========================================================================
    public new const string RESULT = "datasheets1.pdf";
    public readonly string DATASHEET_PATH = Path.Combine(Utility.ResourcePdf, DATASHEET);
// ---------------------------------------------------------------------------
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) { 
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            using (PdfCopy copy = new PdfCopy(document, ms)) {
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
// ---------------------------------------------------------------------------
    /**
     * Fills out a data sheet, flattens it, and adds it to a combined PDF.
     * @param copy the PdfCopy instance (can also be PdfSmartCopy)
     */
    public void AddDataSheets(PdfCopy copy) {
      IEnumerable<Movie> movies = PojoFactory.GetMovies();
      // Loop over all the movies and fill out the data sheet
      foreach (Movie movie in movies) {
        PdfReader reader = new PdfReader(DATASHEET_PATH);
        using (var ms = new MemoryStream()) {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
            Fill(stamper.AcroFields, movie);
            stamper.FormFlattening = true;
          }
          reader = new PdfReader(ms.ToArray());
          copy.AddPage(copy.GetImportedPage(reader, 1));
        }
      }
    }    
// ===========================================================================
  }
}