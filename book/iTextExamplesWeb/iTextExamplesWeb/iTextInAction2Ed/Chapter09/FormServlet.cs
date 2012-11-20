/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
  public class FormServlet : IWriter {
// ===========================================================================
    protected HttpContext WebContext;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      FormServlet x = new FormServlet();
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
          WebContext.Request.RawUrl, null, PdfAction.SUBMIT_HTML_FORMAT
        );
        stamper.AddAnnotation(submit, 1);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Shows the stream passed to the server with POST
     */
    protected void DoPost(byte[] pdf, Stream stream) {
      // We get a resource from our web app
      PdfReader reader = new PdfReader(pdf);
      // Now we create the PDF
      using (PdfStamper stamper = new PdfStamper(reader, stream)) {
        // We alter the fields of the existing PDF
        AcroFields fields = stamper.AcroFields;
        fields.SetFieldProperty(
          "personal.password", "clrfflags", TextField.PASSWORD, null
        );
        var req_form = WebContext.Request.Form;
        foreach (string k in req_form) {
          fields.SetField(k, req_form[k]);
        }
        stamper.FormFlattening = true;
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