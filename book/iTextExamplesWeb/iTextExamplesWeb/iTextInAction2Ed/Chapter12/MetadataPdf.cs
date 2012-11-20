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

namespace kuujinbo.iTextInAction2Ed.Chapter12 {
  public class MetadataPdf : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "pdf_metadata.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "pdf_metadata_changed.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MetadataPdf metadata = new MetadataPdf();
        byte[] pdf = metadata.CreatePdf();
        zip.AddEntry(RESULT1, pdf);
        zip.AddEntry(RESULT2, metadata.ManipulatePdf(pdf));
        zip.Save(stream);
      }
    }
// ---------------------------------------------------------------------------
/**
 * Creates a PDF document.
 */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.AddTitle("Hello World example");
          document.AddAuthor("Bruno Lowagie");
          document.AddSubject("This example shows how to add metadata");
          document.AddKeywords("Metadata, iText, PDF");
          document.AddCreator("My program using iText");
          document.Open();
          // step 4
          document.Add(new Paragraph("Hello World"));
        }
        return ms.ToArray();    
      }
    }    
// ---------------------------------------------------------------------------
/**
 * Manipulates a PDF file src with the file dest as result
 * @param src the original PDF
 */
    public byte[] ManipulatePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          Dictionary<String, String> info = reader.Info;
          info["Title"] = "Hello World stamped";
          info["Subject"] = "Hello World with changed metadata";
          info["Keywords"] = "iText in Action, PdfStamper";
          info["Creator"] = "Silly standalone example";
          info["Author"] = "Also Bruno Lowagie";
          stamper.MoreInfo = info;
        }
        return ms.ToArray();
      }
    }  
// ===========================================================================
  }
}