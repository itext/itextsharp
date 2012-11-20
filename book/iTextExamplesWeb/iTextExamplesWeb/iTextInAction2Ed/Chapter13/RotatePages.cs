/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class RotatePages : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "timetable_rotated.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates();
        byte[] pdf = Utility.PdfBytes(m);
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf); 
        RotatePages r = new RotatePages();      
        zip.AddEntry(RESULT, r.ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Manipulates a PDF file src
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      int n = reader.NumberOfPages;
      int rot;
      PdfDictionary pageDict;
      for (int i = 1; i <= n; i++) {
        rot = reader.GetPageRotation(i);
        pageDict = reader.GetPageN(i);
        pageDict.Put(PdfName.ROTATE, new PdfNumber(rot + 90));        
      }        
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}