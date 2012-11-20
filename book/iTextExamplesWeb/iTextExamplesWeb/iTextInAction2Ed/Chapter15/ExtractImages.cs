/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using kuujinbo.iTextInAction2Ed.Chapter10;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ExtractImages : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      ImageTypes it = new ImageTypes();
      using (ZipFile zip = new ZipFile()) {
        byte[] pdf = it.CreatePdf();
        zip.AddEntry(Utility.ResultFileName(it.ToString() + ".pdf"), pdf);
        PdfReader reader = new PdfReader(pdf);
        PdfReaderContentParser parser = new PdfReaderContentParser(reader);
        MyImageRenderListener listener = new MyImageRenderListener();
        for (int i = 1; i <= reader.NumberOfPages; i++) {
          parser.ProcessContent(i, listener);
        } 
        for (int i = 0; i < listener.MyImages.Count; ++i) {
          zip.AddEntry(
            listener.ImageNames[i],
            listener.MyImages[i]
          );
        }         
        zip.Save(stream);
      }
    }
// ===========================================================================
  }
}