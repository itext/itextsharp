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

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class StampText : IWriter {
// ===========================================================================
  /** A resulting PDF file. */
    public const string RESULT1 = "hello1.pdf";
  /** A resulting PDF file. */
    public const string RESULT2 = "hello2.pdf";
  /** A resulting PDF file. */
    public const string RESULT3 = "hello3.pdf";
// ---------------------------------------------------------------------------        
    public void Write(Stream stream) {
      HelloWorldLandscape1 h1 = new HelloWorldLandscape1();
      byte[] h1b = Utility.PdfBytes(h1);
      HelloWorldLandscape2 h2 = new HelloWorldLandscape2();
      byte[] h2b = Utility.PdfBytes(h2);

      using (ZipFile zip = new ZipFile()) {
        zip.AddEntry(RESULT1, Stamp(h1b));
        zip.AddEntry(RESULT2, StampIgnoreRotation(h1b));
        zip.AddEntry(RESULT3, Stamp(h2b));

        zip.AddEntry(Utility.ResultFileName(h1.ToString() + ".pdf"), h1b);
        zip.AddEntry(Utility.ResultFileName(h2.ToString() + ".pdf"), h2b);
        zip.Save(stream);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param resource the original PDF
     */
    public static byte[] Stamp(byte[] resource) {
      PdfReader reader = new PdfReader(resource);
      using (var ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          PdfContentByte canvas = stamper.GetOverContent(1);
          ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_LEFT, 
            new Phrase("Hello people!"), 
            36, 540, 0
          );
        }
        return ms.ToArray();
      }
    }

    /**
     * Manipulates a PDF file
     * @param resource the original PDF
     */
    public static byte[] StampIgnoreRotation(byte[] resource) {
      PdfReader reader = new PdfReader(resource);
      using (var ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          stamper.RotateContents = false;
          PdfContentByte canvas = stamper.GetOverContent(1);
          ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_LEFT, 
            new Phrase("Hello people!"), 
            36, 540, 0
          );
        }
        return ms.ToArray();
      }    
    }    
// ===========================================================================
  }
}