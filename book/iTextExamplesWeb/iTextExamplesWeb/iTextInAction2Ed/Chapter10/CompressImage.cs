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

namespace kuujinbo.iTextInAction2Ed.Chapter10 {
  public class CompressImage : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "uncompressed.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "compressed.pdf";
    /** One of the resources. */
    public const string RESOURCE = "butterfly.bmp";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        CompressImage c = new CompressImage();
        zip.AddEntry(RESULT1, c.CreatePdf(false));
        zip.AddEntry(RESULT2, c.CreatePdf(true));
        zip.AddFile(Path.Combine(Utility.ResourceImage, RESOURCE), "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     * @param filename the path to the new PDF document
     */    
    public byte[] CreatePdf(bool compress)  {
      using (MemoryStream ms = new MemoryStream()) {
      // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          Image img = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, RESOURCE)
          );
          if (compress) img.CompressionLevel = 9;
          document.Add(img);
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}