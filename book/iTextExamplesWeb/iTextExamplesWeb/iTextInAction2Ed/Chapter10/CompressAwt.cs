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
  public class CompressAwt : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "hitchcock100.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "hitchcock20.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "hitchcock10.pdf";
    /** One of the resources. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceImage, "hitchcock.png"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        CompressAwt c = new CompressAwt();
        zip.AddEntry(RESULT1, c.CreatePdf(100L));
        zip.AddEntry(RESULT2, c.CreatePdf(20L));
        zip.AddEntry(RESULT3, c.CreatePdf(10L));
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
/*
 * you DO NOT want to use classes within the System.Drawing namespace to
 * manipulate image files in ASP.NET applications, see the warning here:
 * http://msdn.microsoft.com/en-us/library/system.drawing.aspx
*/
    public byte[] CreatePdf(long quality) {
      using (MemoryStream msDoc = new MemoryStream()) {
        Image img = null;
        using (System.Drawing.Bitmap dotnetImg = 
            new System.Drawing.Bitmap(RESOURCE)) 
        {
          // set codec to jpeg type => jpeg index codec is "1"
          System.Drawing.Imaging.ImageCodecInfo codec = 
          System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()[1];
          // set parameters for image quality
          System.Drawing.Imaging.EncoderParameters eParams = 
            new System.Drawing.Imaging.EncoderParameters(1);
          eParams.Param[0] = 
            new System.Drawing.Imaging.EncoderParameter(
              System.Drawing.Imaging.Encoder.Quality, quality
          );
          using (MemoryStream msImg = new MemoryStream()) {
            dotnetImg.Save(msImg, codec, eParams);
            msImg.Position = 0;
            img = Image.GetInstance(msImg);
            img.SetAbsolutePosition(15, 15);
            // step 1
            using (Document document = new Document()) {
              // step 2
              PdfWriter.GetInstance(document, msDoc);
              // step 3
              document.Open();
              // step 4
              document.Add(img);
            }
          }
        }
        return msDoc.ToArray();
      }
    }
// ===========================================================================
  }
}