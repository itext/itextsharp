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
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter08;
/*
 * some of the chapter 9 examples need extra checks to allow
 * __NON__ web developers to build all the other chapter output files. 
 * again, this only runs on localhost!
 * 
 * this example creates a zip file; unpack the archive then click
 * on "subscribe.fdf" to see the result
 */
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class CreateFDF : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Subscribe s = new Subscribe();
        byte[] pdf = s.CreatePdf();
        string PdfName = Utility.ResultFileName(s.ToString() + ".pdf");
        zip.AddEntry(PdfName, pdf);

        FdfWriter fdf = new FdfWriter();
/*
 * we're hard-coding the FDF data, not receiving it from an
 * HTML page like the book example
 */
        fdf.SetFieldAsString("personal.name", "HARD-CODED name");
        fdf.SetFieldAsString("personal.loginname", "HARD-CODED loginname");
        fdf.SetFieldAsString("personal.password", "HARD-CODED password");
        fdf.SetFieldAsString("personal.reason", "HARD-CODED reason");
        fdf.File = PdfName;
        using (MemoryStream ms = new MemoryStream()) {
          fdf.WriteTo(ms);
          zip.AddEntry("subscribe.fdf", ms.ToArray());
        }
        zip.Save(stream);             
      }
    }
// ===========================================================================
  }
}