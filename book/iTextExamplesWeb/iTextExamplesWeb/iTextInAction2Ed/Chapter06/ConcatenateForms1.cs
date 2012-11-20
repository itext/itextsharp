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

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class ConcatenateForms1 : IWriter {
// ===========================================================================
    public const string RESULT = "concatenated_forms1.pdf";
    /** The original PDF file. */
    public const string copyName = "datasheet.pdf";
    public readonly string DATASHEET = Path.Combine(
      Utility.ResourcePdf, copyName
    );
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        using (MemoryStream ms = new MemoryStream()) {
          // Create a PdfCopyFields object
          PdfCopyFields copy = new PdfCopyFields(ms);
          // add a document
          copy.AddDocument(new PdfReader(DATASHEET));
          // add a document
          copy.AddDocument(new PdfReader(DATASHEET));
          // close the PdfCopyFields object
          copy.Close();
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.AddFile(DATASHEET, "");
        zip.Save(stream);
      }
    }
// ===========================================================================
  }
}