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
  public class RawImage : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // Image in colorspace DeviceGray
        byte[] gradient = new byte[256];
        for (int i = 0; i < 256; i++) {
          gradient[i] = (byte) i;
        }
        Image img1 = Image.GetInstance(256, 1, 1, 8, gradient);
        img1.ScaleAbsolute(256, 50);
        document.Add(img1);
        // Image in colorspace RGB
        byte[] cgradient = new byte[256 * 3];
        for (int i = 0; i < 256; i++) {
          cgradient[i * 3] = (byte) (255 - i);
          cgradient[i * 3 + 1] = (byte) (255 - i);
          cgradient[i * 3 + 2] = (byte) i;
        }
        Image img2 = Image.GetInstance(256, 1, 3, 8, cgradient);
        img2.ScaleAbsolute(256, 50);
        document.Add(img2);
        Image img3 = Image.GetInstance(16, 16, 3, 8, cgradient);
        img3.ScaleAbsolute(64, 64);
        document.Add(img3);
      }
    }
// ===========================================================================
  }
}