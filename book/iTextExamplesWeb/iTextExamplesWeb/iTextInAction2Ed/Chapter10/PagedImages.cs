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
using iTextSharp.text.pdf.codec;

namespace kuujinbo.iTextInAction2Ed.Chapter10 {
  public class PagedImages : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "tiff_jbig2_gif.pdf";
    /** One of the resources. */
    public static string RESOURCE1 = Path.Combine(Utility.ResourceImage, "marbles.tif");
    /** One of the resources. */
    public static string RESOURCE2 = Path.Combine(Utility.ResourceImage, "amb.jb2");
    /** One of the resources. */
    public static string RESOURCE3 = Path.Combine(Utility.ResourceImage, "animated_fox_dog.gif");
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        PagedImages p = new PagedImages();
        zip.AddEntry(RESULT, p.CreatePdf());
        zip.AddFile(RESOURCE1, "");
        zip.AddFile(RESOURCE2, "");
        zip.AddFile(RESOURCE3, "");
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
          document.Open();
          // step 4
          AddTif(document, RESOURCE1);
          document.NewPage();
          AddJBIG2(document, RESOURCE2);
          document.NewPage();
          AddGif(document, RESOURCE3);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    public void AddTif(Document document, String path) {
      RandomAccessFileOrArray ra = new RandomAccessFileOrArray(path);
      int n = TiffImage.GetNumberOfPages(ra);
      Image img;
      for (int i = 1; i <= n; i++) {
        img = TiffImage.GetTiffImage(ra, i);
        img.ScaleToFit(523, 350);
        document.Add(img);
      }
    }
// ---------------------------------------------------------------------------    
    public void AddJBIG2(Document document, String path) {
      RandomAccessFileOrArray ra = new RandomAccessFileOrArray(path);
      int n = JBIG2Image.GetNumberOfPages(ra);
      Image img;
      for (int i = 1; i <= n; i++) {
        img = JBIG2Image.GetJbig2Image(ra, i);
        img.ScaleToFit(523, 350);
        document.Add(img);
      }
    }
// ---------------------------------------------------------------------------    
    public void AddGif(Document document, String path) {
      GifImage img = new GifImage(path);
      int n = img.GetFrameCount();
      for (int i = 1; i <= n; i++) {
        document.Add(img.GetImage(i));
      }
    }    
// ===========================================================================
  }
}