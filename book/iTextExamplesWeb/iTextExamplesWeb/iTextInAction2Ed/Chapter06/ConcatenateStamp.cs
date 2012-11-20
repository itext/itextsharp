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
using kuujinbo.iTextInAction2Ed.Chapter02;
using kuujinbo.iTextInAction2Ed.Chapter05;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class ConcatenateStamp : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT = "concatenated_stamped.pdf";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {   
      using (ZipFile zip = new ZipFile()) { 
        MovieLinks1 ml = new MovieLinks1();
        byte[] r1 = Utility.PdfBytes(ml);
        MovieHistory mh = new MovieHistory();
        byte[] r2 = Utility.PdfBytes(mh);
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            using (PdfCopy copy = new PdfCopy(document, ms)) {
              // step 3
              document.Open();
              // step 4
              // reader for document 1
              PdfReader reader1 = new PdfReader(r1);
              int n1 = reader1.NumberOfPages;
              // reader for document 2
              PdfReader reader2 = new PdfReader(r2);
              int n2 = reader2.NumberOfPages;
              // initializations
              PdfImportedPage page;
              PdfCopy.PageStamp stamp;
              // Loop over the pages of document 1
              for (int i = 0; i < n1; ) {
                page = copy.GetImportedPage(reader1, ++i);
                stamp = copy.CreatePageStamp(page);
                // add page numbers
                ColumnText.ShowTextAligned(
                  stamp.GetUnderContent(), Element.ALIGN_CENTER,
                  new Phrase(string.Format("page {0} of {1}", i, n1 + n2)),
                  297.5f, 28, 0
                );
                stamp.AlterContents();
                copy.AddPage(page);
              }

              // Loop over the pages of document 2
              for (int i = 0; i < n2; ) {
                page = copy.GetImportedPage(reader2, ++i);
                stamp = copy.CreatePageStamp(page);
                // add page numbers
                ColumnText.ShowTextAligned(
                  stamp.GetUnderContent(), Element.ALIGN_CENTER,
                  new Phrase(string.Format("page {0} of {1}", n1 + i, n1 + n2)),
                  297.5f, 28, 0
                );
                stamp.AlterContents();
                copy.AddPage(page);
              }   
            }   
          }
          zip.AddEntry(RESULT, ms.ToArray());          
          zip.AddEntry(Utility.ResultFileName(ml.ToString() + ".pdf"), r1);       
          zip.AddEntry(Utility.ResultFileName(mh.ToString()+ ".pdf"), r2); 
        }
        zip.Save(stream);
      }       
    }
// ===========================================================================
  }
}