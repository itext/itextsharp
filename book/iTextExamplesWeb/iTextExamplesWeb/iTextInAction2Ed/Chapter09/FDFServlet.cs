/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter08;
/*
 * some of the chapter 9 examples need extra checks to allow
 * __NON__ web developers to build all the other chapter output files. 
 * again, this only runs on localhost!
 */
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class FDFServlet : IWriter {
// ===========================================================================
    protected HttpContext WebContext;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      FDFServlet x = new FDFServlet();
      x.WebContext = HttpContext.Current;
      if (x.WebContext != null) {
        Subscribe s = new Subscribe();
        byte[] pdf = s.CreatePdf();
        x.WebContext.Response.ContentType = "application/pdf";
        if (Utility.IsHttpPost()) {
          x.DoPost(pdf, stream);
        } else {
          x.DoGet(pdf, stream);
        }
      }
      else {
        x.SendCommandLine(stream);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Show keys and values passed to the query string with GET
     */    
    protected void DoGet(byte[] pdf, Stream stream) {
      // We get a resource from our web app
      PdfReader reader = new PdfReader(pdf);
      // Now we create the PDF
      using (PdfStamper stamper = new PdfStamper(reader, stream)) {
        // We add a submit button to the existing form
        PushbuttonField button = new PushbuttonField(
          stamper.Writer, new Rectangle(90, 660, 140, 690), "submit"
        );
        button.Text = "POST";
        button.BackgroundColor = new GrayColor(0.7f);
        button.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
        PdfFormField submit = button.Field;
        submit.Action = PdfAction.CreateSubmitForm(
          WebContext.Request.RawUrl, null, 0
        );
        stamper.AddAnnotation(submit, 1);
        // We add an extra field that can be used to upload a file
        TextField file = new TextField(
          stamper.Writer, new Rectangle(160, 660, 470, 690), "image"
        );
        file.Options = TextField.FILE_SELECTION;
        file.BackgroundColor = new GrayColor(0.9f);
        PdfFormField upload = file.GetTextField();
        upload.SetAdditionalActions(PdfName.U,
          PdfAction.JavaScript(
            "this.getField('image').browseForFileToSubmit();"
            + "this.getField('submit').setFocus();",
            stamper.Writer
          )
        );
        stamper.AddAnnotation(upload, 1);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Shows the stream passed to the server with POST
     */
    protected void DoPost(byte[] pdf, Stream stream) {
      using (Stream s = WebContext.Request.InputStream) {
        // Create a reader that interprets the Request's input stream
        FdfReader fdf = new FdfReader(s);
        // We get a resource from our web app
        PdfReader reader = new PdfReader(pdf);
        // Now we create the PDF
        using (PdfStamper stamper = new PdfStamper(reader, stream)) {
          // We alter the fields of the existing PDF
          AcroFields fields = stamper.AcroFields;
          fields.SetFields(fdf);
          stamper.FormFlattening = true;
          // Gets the image from the FDF file
          try {
            Image img = Image.GetInstance(fdf.GetAttachedFile("image"));
            img.ScaleToFit(100, 100);
            img.SetAbsolutePosition(90, 590);
            stamper.GetOverContent(1).AddImage(img);
          }
          catch {
            ColumnText.ShowTextAligned(
              stamper.GetOverContent(1), 
              Element.ALIGN_LEFT, 
              new Phrase("No image posted!"), 
              90, 660, 0
            );
          } 
        }
      }
    } 
// ---------------------------------------------------------------------------
    public void SendCommandLine(Stream stream) {
      using (StreamWriter w = new StreamWriter(stream, Encoding.UTF8)) {
        w.WriteLine(
          "EXAMPLE ONLY PRODUCES OUTPUT RESULT UNDER WEB CONTEXT"
        );
        w.Flush();
      }
    }             
// ===========================================================================
  }
}