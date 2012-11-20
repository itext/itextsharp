/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class MovieAnnotation : IWriter {
// ===========================================================================
    /** One of the resources. */
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceImage, "foxdog.mpg"
    );
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
          writer, RESOURCE, "foxdog.mpg", null
        );
        writer.AddAnnotation(PdfAnnotation.CreateScreen(
          writer,
          new Rectangle(200f, 700f, 400f, 800f), 
          "Fox and Dog", fs,
          "video/mpeg", true
        ));        
      }
    }
// ===========================================================================
	}
}