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
/*
 * some of the chapter 9 examples need extra checks to allow
 * __NON__ web developers to build all the other chapter output files.
 * for this example we send:
 * [1] HTML if there is no "text" parameter (GET / POST)
 * [2] PDF if there __IS__ "text" parameter (GET / POST)
 * [3] text file if running command line
 * again, this only runs on localhost!
 */
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class PdfServlet : IWriter {
// ===========================================================================
    public const string HTML = @"
<html><head><title>iText in Action 2E: web applications</title></head>
<body>
<form method='GET' action='{0}'>
<textarea id='textGet'>Enter some text</textarea>
<div>
<input type='Submit' value='Submit with GET' 
  onclick='window.location = ""{0}&text="" + encodeURI(document.getElementById(""textGet"").value);return false;' 
/>
</div>
</form>
<form method='POST' action='{0}'>
<textarea name='text'>Enter some text</textarea>
<div><input type='Submit' value='Submit with POST' /></div>
</form>
</body></html>";
    protected HttpContext WebContext;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      PdfServlet x = new PdfServlet();
      x.WebContext = HttpContext.Current;
      if (x.WebContext != null) {
        HttpRequest Request = x.WebContext.Request;
        // Get the text that will be added to the PDF
        string text = Request["text"];
        if (text != null && text.Trim().Length > 0) {
          x.SendPdf(stream, text);
        } 
        else {
          x.SendHtml();
        }       
      }
      else {
        x.SendCommandLine(stream);
      }
    }
// ---------------------------------------------------------------------------
    public void SendHtml() {
      HttpResponse Response = WebContext.Response;
      Response.Write(string.Format(HTML, WebContext.Request.Url.ToString()));    
    }
// ---------------------------------------------------------------------------
    public void SendPdf(Stream stream, string text) {
      // setting some response headers
      HttpResponse Response = HttpContext.Current.Response;
      Response.ContentType = "application/pdf";
      Response.AppendHeader("Expires", "0");
      Response.AppendHeader(
        "Cache-Control",
        "must-revalidate, post-check=0, pre-check=0"
      );
      Response.AppendHeader("Pragma", "public");
          
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph(String.Format(
          "HTTP request using the {0} method: ",
          WebContext.Request.HttpMethod
        )));                   
        document.Add(new Paragraph(text));
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