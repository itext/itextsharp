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
  public class ImageMask : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT1 = "hardmask.pdf";
    /** The resulting PDF file. */
    public const String RESULT2 = "softmask.pdf";
    /** One of the resources. */
    public static string RESOURCE = Path.Combine(Utility.ResourceImage, "bruno.jpg");
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {    
      using (ZipFile zip = new ZipFile()) {
        byte[] circledata = { 
          (byte) 0x3c, (byte) 0x7e, (byte) 0xff, (byte) 0xff, 
          (byte) 0xff, (byte) 0xff, (byte) 0x7e, (byte) 0x3c 
        };
        Image mask = Image.GetInstance(8, 8, 1, 1, circledata);
        mask.MakeMask();
        mask.Inverted = true;
        
        ImageMask im = new ImageMask();
        zip.AddEntry(RESULT1, im.CreatePdf(mask));
        
        byte[] gradient = new byte[256];
        for (int i = 0; i < 256; i++) {
          gradient[i] = (byte) i;
        }
        mask = Image.GetInstance(256, 1, 1, 8, gradient);
        mask.MakeMask();
        im = new ImageMask();
        zip.AddEntry(RESULT2, im.CreatePdf(mask));

        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates a PDF document.
     */ 
    public byte[] CreatePdf(Image mask) {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          Image img = Image.GetInstance(RESOURCE);
          img.ImageMask = mask;
          document.Add(img);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}