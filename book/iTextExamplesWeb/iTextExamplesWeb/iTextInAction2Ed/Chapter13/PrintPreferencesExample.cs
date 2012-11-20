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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class PrintPreferencesExample : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PdfVersion = PdfWriter.VERSION_1_5;
        writer.AddViewerPreference(PdfName.PRINTSCALING, PdfName.NONE);
        writer.AddViewerPreference(PdfName.NUMCOPIES, new PdfNumber(3));
        writer.AddViewerPreference(PdfName.PICKTRAYBYPDFSIZE, PdfBoolean.PDFTRUE);        
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph("Hello World!")); 
      }
    }
// ===========================================================================
  }
}