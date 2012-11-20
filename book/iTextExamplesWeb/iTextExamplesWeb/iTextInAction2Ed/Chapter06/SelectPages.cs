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
  public class SelectPages : IWriter {
// ===========================================================================
	/** A resulting PDF file. */
    public const String RESULT1 = "timetable_stamper.pdf";
	/** A resulting PDF file. */
    public const String RESULT2 = "timetable_copy.pdf"; 
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      MovieTemplates mt = new MovieTemplates();
      byte[] pdf = Utility.PdfBytes(mt);
      PdfReader reader = new PdfReader(pdf);
      using (ZipFile zip = new ZipFile()) {
        reader.SelectPages("4-8");
        zip.AddEntry(RESULT1, ManipulateWithStamper(reader));
/*
 * can't figure out __WHY__, but if i don't reset the reader the example
 * will __NOT__ work!
 */
        reader = new PdfReader(pdf);
        reader.SelectPages("4-8"); 
        zip.AddEntry(RESULT2, ManipulateWithCopy(reader));       
        zip.AddEntry(Utility.ResultFileName(mt.ToString() + ".pdf"), pdf);
        zip.Save(stream);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a new PDF based on the one in the reader
     * @param reader a reader with a PDF file
     */
    private byte[] ManipulateWithStamper(PdfReader reader) {
      using (MemoryStream ms = new MemoryStream()) {
        using ( PdfStamper stamper = new PdfStamper(reader, ms) ) {
        }
        return ms.ToArray();
      }    
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a new PDF based on the one in the reader
     * @param reader a reader with a PDF file
     */
    private byte[] ManipulateWithCopy(PdfReader reader) {
      using (MemoryStream ms = new MemoryStream()) {
        int n = reader.NumberOfPages;
        using (Document document = new Document()) {
          using (PdfCopy copy = new PdfCopy(document, ms)) {
            document.Open();
            for (int i = 0; i < n;) {
              copy.AddPage(copy.GetImportedPage(reader, ++i));
            }
          }
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}