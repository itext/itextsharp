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

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class PdfXPdfA : IWriter {
// ===========================================================================
    public const string RESULT1 = "x.pdf";
    public const string RESULT2 = "a.pdf";
    public const string FONT = "c:/windows/fonts/arial.ttf";
    public static string PROFILE = Path.Combine(
      Utility.ResourceImage, "sRGB.profile"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        PdfXPdfA example = new PdfXPdfA();
        zip.AddEntry(RESULT1, example.CreatePdfX());       
        zip.AddEntry(RESULT2, example.CreatePdfA());       
        zip.Save(stream);             
      }
    }    
// ---------------------------------------------------------------------------
  /**
   * Creates a PDF document.
   */      
    public byte[] CreatePdfX() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
        // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.PDFXConformance = PdfWriter.PDFX1A2001;
          // step 3
          document.Open();
          // step 4
          Font font = FontFactory.GetFont(
            FONT, BaseFont.CP1252, BaseFont.EMBEDDED, Font.UNDEFINED,
            Font.UNDEFINED, new CMYKColor(255, 255, 0, 0)
          );
          document.Add(new Paragraph("Hello World", font));
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdfA() {
      using (MemoryStream ms = new MemoryStream()) {
      // step 1
        using (Document document = new Document()) {
          // step 2
          PdfAWriter writer = PdfAWriter.GetInstance(document, ms, PdfAConformanceLevel.PDF_A_1B);
          writer.CreateXmpMetadata();    
          // step 3
          document.Open();
          // step 4
          Font font = FontFactory.GetFont(FONT, BaseFont.CP1252, BaseFont.EMBEDDED);
          document.Add(new Paragraph("Hello World", font));
          using (FileStream fs2 = new FileStream(
              PROFILE, FileMode.Open, FileAccess.Read, FileShare.Read
          )) 
          {
            ICC_Profile icc = ICC_Profile.GetInstance(fs2);
            writer.SetOutputIntents(
              "Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc
            );
          }
        } 
        return ms.ToArray();  
      }   
    }
// ===========================================================================
  }
}