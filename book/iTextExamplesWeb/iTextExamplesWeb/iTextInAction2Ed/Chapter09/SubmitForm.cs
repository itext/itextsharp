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
  public class SubmitForm : IWriter {
// ===========================================================================
    protected HttpContext WebContext;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      SubmitForm x = new SubmitForm();
      x.WebContext = HttpContext.Current;
      if (x.WebContext != null) {
        Subscribe sub = new Subscribe();
        byte[] pdf = sub.CreatePdf();
        x.WebContext.Response.ContentType = "application/pdf";
        x.ManipulatePdf(pdf, stream);
      }
      else {
        x.SendCommandLine(stream);
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    // public byte[] ManipulatePdf(byte[] src) {
    public void ManipulatePdf(byte[] src, Stream stream) {
      string BaseUrl = Utility.GetServerBaseUrl();
      // create a reader
      PdfReader reader = new PdfReader(src);
      // create a stamper
      using (PdfStamper stamper = new PdfStamper(reader, stream)) {
        // create a submit button that posts the form as an HTML query string
        PushbuttonField button1 = new PushbuttonField(
          stamper.Writer, new Rectangle(90, 660, 140, 690), "post"
        );
        button1.Text = "POST";
        button1.BackgroundColor = new GrayColor(0.7f);
        button1.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
        PdfFormField submit1 = button1.Field;
        string submit_url = new Uri(
          new Uri(BaseUrl), 
          string.Format(
            "/iTextInAction2Ed/WebHandler.ashx?{0}=Chapter09&{1}=ShowData",
            Chapters.QS_CHAPTER, Chapters.QS_CLASS
          )
        ).ToString();
        submit1.Action = PdfAction.CreateSubmitForm(
          submit_url, null, 
          PdfAction.SUBMIT_HTML_FORMAT | PdfAction.SUBMIT_COORDINATES
        );
        // add the button
        stamper.AddAnnotation(submit1, 1);
        // create a submit button that posts the form as FDF
        PushbuttonField button2 = new PushbuttonField(
          stamper.Writer, new Rectangle(200, 660, 250, 690), "FDF"
        );
        button2.BackgroundColor = new GrayColor(0.7f);
        button2.Text = "FDF";
        button2.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
        PdfFormField submit2 = button2.Field;
        submit2.Action = PdfAction.CreateSubmitForm(
          submit_url, null, PdfAction.SUBMIT_EXCL_F_KEY
        );
        // add the button
        stamper.AddAnnotation(submit2, 1);
        // create a submit button that posts the form as XFDF
        PushbuttonField button3 = new PushbuttonField(
          stamper.Writer, new Rectangle(310, 660, 360, 690), "XFDF"
        );
        button3.BackgroundColor = new GrayColor(0.7f);
        button3.Text = "XFDF";
        button3.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
        PdfFormField submit3 = button3.Field;
        submit3.Action = PdfAction.CreateSubmitForm(
          submit_url, null, PdfAction.SUBMIT_XFDF
        );
        // add the button
        stamper.AddAnnotation(submit3, 1);
        // create a reset button
        PushbuttonField button4 = new PushbuttonField(
          stamper.Writer, new Rectangle(420, 660, 470, 690), "reset"
        );
        button4.BackgroundColor = new GrayColor(0.7f);
        button4.Text = "RESET";
        button4.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
        PdfFormField reset = button4.Field;
        reset.Action = PdfAction.CreateResetForm(null, 0);
        // add the button
        stamper.AddAnnotation(reset, 1);
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