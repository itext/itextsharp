/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter02;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class ConcatenateNamedDestinations : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "concatenated_links_1.pdf";  
    /** The resulting PDF file. */
    public const string RESULT2 = "concatenated_links_2.pdf";    
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        // Use previous examples to create PDF files
        MovieLinks1 m = new MovieLinks1(); 
        byte[] pdfM = Utility.PdfBytes(m);
        LinkActions l = new LinkActions();
        byte[] pdfL = l.CreatePdf();
        // Create readers.
        PdfReader[] readers = {
          new PdfReader(pdfL),
          new PdfReader(pdfM)
        };
        
        // step 1
        //Document document = new Document();
        // step 2
        using (var ms = new MemoryStream()) { 
          // step 1
          using (Document document = new Document()) {
            using (PdfCopy copy = new PdfCopy(document, ms)) {
              // step 3
              document.Open();
              // step 4
              int n;
              // copy the pages of the different PDFs into one document
              for (int i = 0; i < readers.Length; i++) {
                readers[i].ConsolidateNamedDestinations();
                n = readers[i].NumberOfPages;
                for (int page = 0; page < n; ) {
                  copy.AddPage(copy.GetImportedPage(readers[i], ++page));
                }
              }
              // Add named destination  
              copy.AddNamedDestinations(
                // from the second document
                SimpleNamedDestination.GetNamedDestination(readers[1], false),
                // using the page count of the first document as offset
                readers[0].NumberOfPages
              );
            }
            zip.AddEntry(RESULT1, ms.ToArray());
          }
          
          // Create a reader
          PdfReader reader = new PdfReader(ms.ToArray());
          // Convert the remote destinations into local destinations
          reader.MakeRemoteNamedDestinationsLocal();
          using (MemoryStream ms2 = new MemoryStream()) {
            // Create a new PDF containing the local destinations
            using (PdfStamper stamper = new PdfStamper(reader, ms2)) {
            }
            zip.AddEntry(RESULT2, ms2.ToArray());
          }
          
        }
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdfM);
        zip.AddEntry(Utility.ResultFileName(l.ToString() + ".pdf"), pdfL);
        zip.Save(stream);             
      }   
   }
// ===========================================================================
  }
}