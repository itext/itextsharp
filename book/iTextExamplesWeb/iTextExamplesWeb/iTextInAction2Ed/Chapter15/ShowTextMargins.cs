/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ShowTextMargins : IWriter {
// ===========================================================================
    /** The original PDF that will be parsed. */
    public readonly string PREFACE = Path.Combine(
      Utility.ResourcePdf, "preface.pdf"
    );    
    /** The resulting text file. */
    public const String RESULT = "margins.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(PREFACE, "");
        PdfReader reader = new PdfReader(PREFACE);
        PdfReaderContentParser parser = new PdfReaderContentParser(reader);
        using (MemoryStream ms = new MemoryStream()) {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
            TextMarginFinder finder;
            for (int i = 1; i <= reader.NumberOfPages; i++) {
              finder = parser.ProcessContent(i, new TextMarginFinder());
              PdfContentByte cb = stamper.GetOverContent(i);
              cb.Rectangle(
                finder.GetLlx(), finder.GetLly(),
                finder.GetWidth(), finder.GetHeight()
              );
              cb.Stroke();
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