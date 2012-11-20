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
 * again, this only runs on localhost!
 */
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class ShowData : IWriter {
// ===========================================================================
    protected HttpContext WebContext;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      ShowData x = new ShowData();
      x.WebContext = HttpContext.Current;
      if (x.WebContext != null) {
        x.WebContext.Response.ContentType = "text/plain";
        x.WebContext.Response.Write(
          Utility.IsHttpPost() ? x.DoPost() : x.DoGet()
        );
      }
      else {
        x.SendCommandLine(stream);
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Show keys and values passed to the query string with GET
     */    
    protected string DoGet() {
      StringBuilder sb = new StringBuilder();
      var qs = WebContext.Request.QueryString;
      foreach (string k in qs.Keys) {
        sb.AppendFormat("{0}: {1}{2}", k, qs[k], Environment.NewLine);
      }
      return sb.ToString();
    }
// ---------------------------------------------------------------------------
    /**
     * Shows the stream passed to the server with POST
     */
    protected string DoPost() {
      using (Stream s = this.WebContext.Request.InputStream) {
        long len = s.Length;
        byte[] b = new byte[len];
        int to_read = (int) len;
        int read = 0;
        while (to_read > 0) {
          int n = s.Read(b, read, 16);
          // read in small chunks ^^
          if (n == 0) break;
          read += n;
          to_read -= n;
        }
        return new UTF8Encoding().GetString(b);         
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