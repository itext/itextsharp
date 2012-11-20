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

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class LaunchAction : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Paragraph p = new Paragraph(
          new Chunk( "Click to open test.txt in Notepad.")
          .SetAction(
            new PdfAction(
              "c:/windows/notepad.exe",
              "test.txt", "open",
              Path.Combine(Utility.ResourceText, "")
            )
        ));
        document.Add(p);
      }
    }
// ===========================================================================
  }
}