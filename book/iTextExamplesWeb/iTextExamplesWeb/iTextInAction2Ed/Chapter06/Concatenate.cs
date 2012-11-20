/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ionic.Zip;
using kuujinbo.iTextInAction2Ed.Chapter02;
using kuujinbo.iTextInAction2Ed.Chapter05;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class Concatenate : IWriter {
// ===========================================================================
    public const string RESULT = "concatenated.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      MovieLinks1 ml = new MovieLinks1();
      MovieHistory mh = new MovieHistory();
      List<byte[]> pdf = new List<byte[]>() {
        Utility.PdfBytes(ml),
        Utility.PdfBytes(mh)
      };
      string[] names = {ml.ToString(), mh.ToString()};
      using (ZipFile zip = new ZipFile()) { 
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (Document document = new Document()) {
            // step 2
            using (PdfCopy copy = new PdfCopy(document, ms)) {
              // step 3
              document.Open();
              // step 4
              for (int i = 0; i < pdf.Count; ++i) {
                zip.AddEntry(Utility.ResultFileName(names[i] + ".pdf"), pdf[i]);
                PdfReader reader = new PdfReader(pdf[i]);
                // loop over the pages in that document
                int n = reader.NumberOfPages;
                for (int page = 0; page < n; ) {
                  copy.AddPage(copy.GetImportedPage(reader, ++page));
                }
              }
            }
          }
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.Save(stream);    
      }
    }
// ===========================================================================
  }
}