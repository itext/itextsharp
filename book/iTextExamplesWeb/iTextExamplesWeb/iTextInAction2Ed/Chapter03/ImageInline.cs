/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class ImageInline : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.POSTCARD, 30, 30, 30, 30)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.CompressionLevel = 0;
        // step 3
        document.Open();
        // step 4
        Image img = Image.GetInstance(Path.Combine(
          Utility.ResourceImage, "loa.jpg"
        ));
        img.SetAbsolutePosition(
          (PageSize.POSTCARD.Width - img.ScaledWidth) / 2,
          (PageSize.POSTCARD.Height - img.ScaledHeight) / 2
        );
        writer.DirectContent.AddImage(img, true);
      }
    }
// ===========================================================================
  }
}