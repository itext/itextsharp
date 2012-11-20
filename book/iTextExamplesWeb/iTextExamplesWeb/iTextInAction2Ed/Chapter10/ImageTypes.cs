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
  public class ImageTypes : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "image_types.pdf";
    /** Paths to images. */
    public string[] RESOURCES = {
      "bruno_ingeborg.jpg",
      "map.jp2",
      "info.png",
      "close.bmp",
      "movie.gif",
      "butterfly.wmf",
      "animated_fox_dog.gif",
      "marbles.tif",
      "amb.jb2"
    };
    /** Path to an image. */
    public const string RESOURCE = "hitchcock.png";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ImageTypes it = new ImageTypes();
        zip.AddEntry(RESULT, it.CreatePdf());
/* 
 * uncomment if you want to see the image files in the zip archive;
 * some of them are semi-large files
 * 
        for (int i = 0; i < RESOURCES.Length; i++) {
          zip.AddFile(Path.Combine(Utility.ResourceImage, RESOURCES[i]), "");
        }
        zip.AddFile(Path.Combine(Utility.ResourceImage, RESOURCE), "");
 */
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document() ) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          Image img = null;
          for (int i = 0; i < RESOURCES.Length; i++) {
            img = Image.GetInstance(Path.Combine(Utility.ResourceImage, RESOURCES[i]));
            if (img.ScaledWidth > 300 || img.ScaledHeight > 300) {
              img.ScaleToFit(300, 300);
            }
            document.Add(new Paragraph(String.Format(
              "{0} is an image of type {1}", RESOURCES[i], img.GetType())
            ));
            document.Add(img);
          }
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
            img = Image.GetInstance(
              dotnet_img, System.Drawing.Imaging.ImageFormat.Png
            );
            document.Add(new Paragraph(String.Format(
              "{0} is an image of type {1}", "System.Drawing.Image", img.GetType())
            ));
            document.Add(img);
          }

          BarcodeEAN codeEAN = new BarcodeEAN();
          codeEAN.CodeType = Barcode.EAN13;
          codeEAN.Code = "9781935182610";
          img = codeEAN.CreateImageWithBarcode(writer.DirectContent, null, null);
          document.Add(new Paragraph(String.Format(
            "{0} is an image of type {1}", "barcode", img.GetType())
          ));
          document.Add(img);

          BarcodePDF417 pdf417 = new BarcodePDF417();
          string text = "iText in Action, a book by Bruno Lowagie.";
          pdf417.SetText(text);
          img = pdf417.GetImage();
          document.Add(new Paragraph(String.Format(
            "{0} is an image of type {1}", "barcode", img.GetType())
          ));
          document.Add(img);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}