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
  public class TransparentImage : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "transparant_image.pdf";
    /** One of the resources. */
    public static string RESOURCE1 = Path.Combine(Utility.ResourceImage, "bruno.jpg");
    /** One of the resources. */
    public static string RESOURCE2 = Path.Combine(Utility.ResourceImage, "info.png");
    /** One of the resources. */
    public static string RESOURCE3 = Path.Combine(Utility.ResourceImage, "1t3xt.gif");
    /** One of the resources. */
    public static string RESOURCE4 = Path.Combine(Utility.ResourceImage, "logo.gif");
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TransparentImage t = new TransparentImage();
        zip.AddEntry(RESULT, t.CreatePdf());
        zip.AddFile(RESOURCE1, "");
        zip.AddFile(RESOURCE2, "");
        zip.AddFile(RESOURCE3, "");
        zip.AddFile(RESOURCE4, "");
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        Image img1 = Image.GetInstance(
          Path.Combine(Utility.ResourceImage, RESOURCE1)
        );
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          img1.SetAbsolutePosition(0, 0);
          document.Add(img1);
          Image img2 = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, RESOURCE2)
          );
          img2.SetAbsolutePosition(0, 260);
          document.Add(img2);
          Image img3 = Image.GetInstance(
            Path.Combine(Utility.ResourceImage,RESOURCE3)
          );
          img3.Transparency = new int[]{ 0x00, 0x10 };
          img3.SetAbsolutePosition(0, 0);
          document.Add(img3);
          Image img4 = Image.GetInstance(
            Path.Combine(Utility.ResourceImage,RESOURCE4)
          );
          img4.Transparency = new int[]{ 0xF0, 0xFF };
          img4.SetAbsolutePosition(50, 50);
          document.Add(img4);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}