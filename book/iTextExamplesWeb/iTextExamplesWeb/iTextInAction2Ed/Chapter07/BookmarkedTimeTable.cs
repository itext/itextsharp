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
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class BookmarkedTimeTable : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "time_table_bookmarks.pdf";
// ---------------------------------------------------------------------------            
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates();
        byte[] pdf = Utility.PdfBytes(m);
        BookmarkedTimeTable b = new BookmarkedTimeTable();
        zip.AddEntry(RESULT, b.ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------  
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      // Create a list with bookmarks
      List<Dictionary<string,object>> outlines = 
          new List<Dictionary<string,object>>();
      Dictionary<string, object> map = new Dictionary<string, object>();
      outlines.Add(map);
      map.Add("Title", "Calendar");
      List<Dictionary<string,object>> kids = 
          new List<Dictionary<string,object>>();
      map.Add("Kids", kids);
      int page = 1;
      IEnumerable<string> days = PojoFactory.GetDays();
      foreach (string day in days) {
        Dictionary<string,object> kid = new Dictionary<string,object>();      
        kids.Add(kid);
        kid["Title"] = day;
        kid["Action"] = "GoTo";
        kid["Page"] = String.Format("{0} Fit", page++);
      }      
      // Create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          stamper.Outlines = outlines;
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}