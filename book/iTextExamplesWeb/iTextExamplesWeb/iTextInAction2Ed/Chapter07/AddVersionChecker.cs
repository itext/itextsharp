/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter01;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class AddVersionChecker : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "version_checker.pdf";
    /** Path to a resource. */
    public const string RESOURCE = "viewer_version.js";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        HelloWorld h = new HelloWorld(); 
        byte[] pdf = Utility.PdfBytes(h);
        // Create a reader
        PdfReader reader = new PdfReader(pdf);
        string js = File.ReadAllText(
          Path.Combine(Utility.ResourceJavaScript, RESOURCE)
        );        
        using (MemoryStream ms = new MemoryStream()) {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
            // Add some javascript
            stamper.JavaScript = js;
          }
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.AddEntry(RESOURCE, js);
        zip.AddEntry(Utility.ResultFileName(h.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }
    }
// ===========================================================================
  }
}