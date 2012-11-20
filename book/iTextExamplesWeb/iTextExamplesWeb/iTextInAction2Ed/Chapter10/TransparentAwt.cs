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
  public class TransparentAwt : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "transparant_hitchcock.pdf";
    /** One of the resources. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceImage, "hitchcock.gif"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TransparentAwt t = new TransparentAwt();
        zip.AddEntry(RESULT, t.CreatePdf());
        zip.AddFile(RESOURCE, "");
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
        Rectangle r = new Rectangle(PageSize.A4);
        r.BackgroundColor = new GrayColor(0.8f);
        using (Document document = new Document(r)) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
/*
* you DO NOT want to use classes within the System.Drawing namespace to
* manipulate image files in ASP.NET applications, see the warning here:
* http://msdn.microsoft.com/en-us/library/system.drawing.aspx
*/
          using (System.Drawing.Image dotnet_img = 
            System.Drawing.Image.FromFile(
              Path.Combine(Utility.ResourceImage, RESOURCE)
            )) 
          {
            document.Add(new Paragraph("Hitchcock in Red."));
            Image img1 = Image.GetInstance(
              dotnet_img, System.Drawing.Imaging.ImageFormat.Gif
            );
            document.Add(img1);
            document.Add(new Paragraph("Hitchcock in Black and White."));
            Image img2 = Image.GetInstance(dotnet_img, null, true);
            document.Add(img2);
            document.NewPage();
            document.Add(new Paragraph("Hitchcock in Red and Yellow."));
            Image img3 = Image.GetInstance(
              dotnet_img, new BaseColor(0xFF, 0xFF, 0x00)
            );
            document.Add(img3);
            document.Add(new Paragraph("Hitchcock in Black and White."));
            Image img4 = Image.GetInstance(
              dotnet_img, new BaseColor(0xFF, 0xFF, 0x00), true
            );
            document.Add(img4);
          }
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}