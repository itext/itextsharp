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
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter02;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class ConcatenateBookmarks : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "concatenated_bookmarks.pdf";
// ---------------------------------------------------------------------------      
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates();
        byte[] pdf = Utility.PdfBytes(m);
        BookmarkedTimeTable b = new BookmarkedTimeTable();
        byte[] bttBytes = b.ManipulatePdf(pdf);      
        MovieHistory mh = new MovieHistory(); 
        byte[] mhBytes = Utility.PdfBytes(mh);
        List<byte[]> src = new List<byte[]>() {bttBytes, mhBytes};
        ConcatenateBookmarks cb = new ConcatenateBookmarks();
        zip.AddEntry(RESULT, cb.ManipulatePdf(src));
        zip.AddEntry(Utility.ResultFileName(b.ToString() + ".pdf"), bttBytes);
        zip.AddEntry(Utility.ResultFileName(mh.ToString() + ".pdf"), mhBytes);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------  
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(List<byte[]> src) {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          using (PdfCopy copy = new PdfCopy(document, ms)) {
            // step 3
            document.Open();
            // step 4
            int page_offset = 0;
            // Create a list for the bookmarks
            List<Dictionary<String, Object>> bookmarks = 
                new List<Dictionary<String, Object>>();
                
            for (int i  = 0; i < src.Count; i++) {
              PdfReader reader = new PdfReader(src[i]);
              // merge the bookmarks
              IList<Dictionary<String, Object>> tmp = 
                  SimpleBookmark.GetBookmark(reader);
              SimpleBookmark.ShiftPageNumbers(tmp, page_offset, null);
              foreach (var d in tmp) bookmarks.Add(d);
              
              // add the pages
              int n = reader.NumberOfPages;
              page_offset += n;
              for (int page = 0; page < n; ) {
                copy.AddPage(copy.GetImportedPage(reader, ++page));
              }
            }
            // Add the merged bookmarks
            copy.Outlines = bookmarks;
          }
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}