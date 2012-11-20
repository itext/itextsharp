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
using System.Web;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter08;
/*
 * some of the chapter 9 examples need extra checks to allow
 * __NON__ web developers to build all the other chapter output files. 
 * again, this only runs on localhost!
 * 
 * this example creates a zip file; unpack the archive then click
 * on "javascript.html" to see the result
 * 
 */
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class JSForm : IWriter {
// ===========================================================================
    public const string HTML = @"
<html><head>
<script language='javascript'>
function createMessageHandler() {{
  var PDFObject = document.getElementById('myPdf');
  PDFObject.messageHandler = {{
    onMessage: function(msg) {{
      document.personal.name.value = msg[0];
      document.personal.loginname.value = msg[1];
    }},
    onError: function(error, msg) {{
      alert(error.message);
    }}
  }}
}}
function sendToPdf() {{
  var PDFObject = document.getElementById('myPdf');
  if(PDFObject!= null){{
    PDFObject.postMessage([
      document.personal.name.value,
      document.personal.loginname.value
    ]);
  }}
}}
</script></head><body onLoad='createMessageHandler();'>
<form name='personal'><table><tr>
<td>Name:</td>
<td><input type='Text' name='name'></td>
<td>Login:</td>
<td><input type='Text' name='loginname'></td>
<td><input type='Button' value='Send to PDF'
onClick='return sendToPdf();'></td>
</tr></table></form>
<object id='myPdf' type='application/pdf' data='{0}'
height='100%' width='100%'></object>
</body></html>
    ";
    
    /** The resulting PDF file. */
    public const String RESULT = "javascript.pdf";
    /** Path to the resources. */
    public static readonly String JS1 = Path.Combine(
      Utility.ResourceJavaScript, "post_from_html.js"
    );
    /** Path to the resources. */
    public static readonly String JS2 = Path.Combine(
      Utility.ResourceJavaScript, "post_to_html.js"
    );    
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(JS1, "");
        zip.AddFile(JS2, "");
        zip.AddEntry("javascript.html", string.Format(HTML, RESULT));
        Subscribe s = new Subscribe();
        byte[] pdf = s.CreatePdf();
        JSForm j = new JSForm();
        zip.AddEntry(RESULT, j.ManipulatePdf(pdf));
        zip.Save(stream);
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src)  {
      // create a reader
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        // create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Add an open action
          PdfWriter writer = stamper.Writer;
          PdfAction action = PdfAction.JavaScript(
            Utilities.ReadFileToString(
              Path.Combine(Utility.ResourceJavaScript, "post_from_html.js")
            ), 
            writer
          );
          writer.SetOpenAction(action);
          // create a submit button that posts data to the HTML page
          PushbuttonField button1 = new PushbuttonField(
            stamper.Writer, new Rectangle(90, 660, 160, 690), "post"
          );
          button1.Text = "POST TO HTML";
          button1.BackgroundColor = new GrayColor(0.7f);
          button1.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
          PdfFormField submit1 = button1.Field;
          submit1.Action = PdfAction.JavaScript(
            Utilities.ReadFileToString(Path.Combine(
              Utility.ResourceJavaScript, "post_to_html.js"
            )), 
            writer
          );
          // add the button
          stamper.AddAnnotation(submit1, 1);
        }
        return ms.ToArray();
      } 
    }  
// ===========================================================================
  }
}