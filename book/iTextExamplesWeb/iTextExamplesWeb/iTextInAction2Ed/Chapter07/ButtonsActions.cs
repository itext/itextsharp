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
using System.Data;
using System.Data.Common;
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class ButtonsActions : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "save_mail_timetable.pdf";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates(); 
        byte[] pdf = Utility.PdfBytes(m);
        ButtonsActions b = new ButtonsActions();
        zip.AddEntry(RESULT, b.ManipulatePdf(pdf));
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), pdf);        
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      // Create a reader
      PdfReader reader = new PdfReader(src);
      int n = reader.NumberOfPages;
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Create pushbutton 1
          PushbuttonField saveAs = new PushbuttonField(
            stamper.Writer,
            new Rectangle(636, 10, 716, 30), 
            "Save"
          );
          saveAs.BorderColor = BaseColor.BLACK;
          saveAs.Text = "Save";
          saveAs.TextColor = BaseColor.RED;
          saveAs.Layout = PushbuttonField.LAYOUT_LABEL_ONLY;
          saveAs.Rotation = 90;
          PdfAnnotation saveAsButton = saveAs.Field;
          saveAsButton.Action = PdfAction.JavaScript(
            "app.execMenuItem('SaveAs')", stamper.Writer
          );
          // Create pushbutton 2
          PushbuttonField mail = new PushbuttonField(
            stamper.Writer,
            new Rectangle(736, 10, 816, 30),
            "Mail"
          );
          mail.BorderColor = BaseColor.BLACK;
          mail.Text = "Mail";
          mail.TextColor = BaseColor.RED;
          mail.Layout = PushbuttonField.LAYOUT_LABEL_ONLY;
          mail.Rotation = 90;
          PdfAnnotation mailButton = mail.Field;
          mailButton.Action = PdfAction.JavaScript(
            "app.execMenuItem('AcroSendMail:SendMail')", 
            stamper.Writer
          );
          // Add the annotations to every page of the document
          for (int page = 1; page <= n; page++) {
            stamper.AddAnnotation(saveAsButton, page);
            stamper.AddAnnotation(mailButton, page);
          }
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}