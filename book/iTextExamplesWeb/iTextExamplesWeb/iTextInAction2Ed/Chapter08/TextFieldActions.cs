/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class TextFieldActions : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        TextField date = new TextField(
          writer, new Rectangle(36, 806, 126, 780), "date"
        );
        date.BorderColor = new GrayColor(0.2f);
        PdfFormField datefield = date.GetTextField();
        // enter something that resembles a date for this to work;
        // if PDF reader doesn't recognize as a date, the field is cleared 
        datefield.SetAdditionalActions(
          PdfName.V, 
          PdfAction.JavaScript("AFDate_FormatEx( 'dd-mm-yyyy' );", writer)
        );
        writer.AddAnnotation(datefield);
        TextField name = new TextField(
          writer, new Rectangle(130, 806, 256, 780), "name"
        );
        name.BorderColor = new GrayColor(0.2f);
        PdfFormField namefield = name.GetTextField();
        namefield.SetAdditionalActions(
          PdfName.FO, 
          PdfAction.JavaScript(
            "app.alert('name field got the focus');", writer
          )
        );
        namefield.SetAdditionalActions(
          PdfName.BL, 
          PdfAction.JavaScript("app.alert('name lost the focus');", writer)
        );
        namefield.SetAdditionalActions(
          PdfName.K, 
          PdfAction.JavaScript(
            "event.change = event.change.toUpperCase();", writer
          )
        );
        writer.AddAnnotation(namefield);
      }
    }
// ===========================================================================
  }
}